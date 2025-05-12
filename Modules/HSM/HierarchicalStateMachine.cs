using System.Collections.Generic;
using System.Linq;

namespace Modules.HSM
{

    public class HierarchicalStateMachine
    {
        public APIDictionary apiDictionary;

        public State currentState = null;

        public static HierarchicalStateMachine Load(string filePath, InteractiveObject interactiveObject)
        {
            var alg = new HierarchicalStateMachine();
            alg.LoadXML(filePath, interactiveObject);
            return alg;
        }

        public List<State> States;
        public List<Transition> Links;

        private void LoadXML(string filePath, InteractiveObject interactiveObject)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            apiDictionary = new APIDictionary(interactiveObject);

            var graph = GraphMLParser.ParseGraphML(filePath);

            States = ParseStates(graph, null);
            Links = ParseTransitions(graph, States);

            currentState = FindFirstState();
        }

        private State FindFirstState()
        {
            return Links.First(link => link.From.nodeType == "initial")?.To;
        }

        public void Start()
        {
            if (currentState == null)
            {
                throw new System.Exception("GML is not loaded");
            }

            var entry = currentState.Events.Find(p => p.ActionName == "entry");
            if (entry != null)
                entry.OnExecute.Invoke();
        }

        public void Stop()
        {
            currentState = FindFirstState();
        }

        public string GetPrefix()
        {
            return $"[GML {apiDictionary?._interactiveObject.GetParent().Name} | {currentState.Name}]";
        }

        private List<State> ParseStates(Graph graph, State parentState)
        {
            if (graph == null)
                return null;

            List<State> states = new List<State>();

            foreach (var node in graph.Nodes)
            {
                if (node.Id == "coreMeta")
                    continue;

                var state = new State { Id = node.Id, gml = this, ParentState = parentState };

                if (node.SubGraph != null)
                {
                    states.AddRange(ParseStates(node.SubGraph, state));
                }

                if (node.Data.ContainsKey("dName"))
                {
                    state.Name = node.Data["dName"];
                }

                if (node.Data.ContainsKey("dData"))
                {
                    var signalData = node.Data["dData"].Split("\n");
                    state.Events.AddRange(ParseEvents(signalData, state));
                }

                if (node.Data.ContainsKey("dVertex"))
                {
                    state.nodeType = node.Data["dVertex"];
                }

                states.Add(state);
            }

            return states;
        }

        private List<Event> ParseEvents(string[] eventData, State parent)
        {
            var events = new List<Event>();
            Event currentEvent = null;

            foreach (var row in eventData)
            {
                var data = row.Replace("\r", "");

                if (string.IsNullOrEmpty(data))
                    continue;

                if (data.Contains("/"))
                {
                    currentEvent?.Subscribe(); // Подписываем предыдущий сигнал 

                    currentEvent = Event.ParseFromData(data, this);
                    currentEvent.signalParent = parent;

                    events.Add(currentEvent);
                }
                else if (currentEvent != null)
                {
                    currentEvent.Methods.Add(Command.ParseFromData(data, this));
                }
            }

            currentEvent.Subscribe();


            return events;
        }

        private List<Transition> ParseTransitions(Graph graph, List<State> states)
        {
            List<Transition> transitions = new List<Transition>();

            if (graph == null)
                return transitions;

            foreach (var edge in graph.Edges)
            {
                var transition = new Transition
                {
                    From = states.Find(p => p.Id == edge.Source),
                    To = states.Find(p => p.Id == edge.Target),
                    gml = this
                };

                if (edge.Data.ContainsKey("dData"))
                {
                    transition.When = Event.ParseFromData(edge.Data["dData"], this);

                    transition.When.linkParent = transition;

                    transition.When.SubscribeForLinks();

                    transition.Subscribe();
                }

                transitions.Add(transition);
            }

            return transitions;
        }
    }
}


