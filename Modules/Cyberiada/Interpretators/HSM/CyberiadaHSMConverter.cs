using System;
using System.Collections.Generic;
using System.Linq;
using Talent.Graphs;
using Talent.Logic.Bus;
using Talent.Logic.HSM.Builders;
using Action = Talent.Graphs.Action;

namespace Talent.Logic.HSM
{
    /// <summary>
    /// Класс, конвертирующий исходный Cyberiada граф в иерархическую машину состояний (ИМС)
    /// </summary>
    public class CyberiadaHSMConverter : ILogicInterpreter<CyberiadaGraph, IBus>
    {
        private const string EnterEventId = "entry";
        private const string ExitEventId = "exit";

        private CyberiadaGraph _sourceGraph;

        /// <summary>
        /// Обрабатывает исходный граф для строительства ИМС
        /// </summary>
        /// <param name="source">Исходный граф</param>
        /// <param name="bus">Шина</param>
        /// <returns>Построенная ИМС</returns>
        public IBehavior Process(CyberiadaGraph source, IBus bus)
        {
            StateBuilder builder = new StateBuilder(bus, source.ID);
            _sourceGraph = source;

            foreach (Node node in source.Nodes)
            {
                IEnumerable<Edge> edges = GetEdges(node.ID);

                StateBuilder childBuilder = CreateChildBuilder(node, edges, bus);
                builder.AddChildState(childBuilder);
            }

            State state = builder.Build();
            return new HSMBehavior(state, bus, NodeData.Vertex_Initial);
        }

        private StateBuilder CreateChildBuilder(
            Node node,
            IEnumerable<Edge> edges,
            IBus bus)
        {
            StateBuilder builder = new StateBuilder(bus, node.ID);

            builder.AddLabel(node.Data.VisualData.Name);
            AddCommandsToEvent(node, builder);
            AddTransitions(edges, builder);
            AddSubHsm(node, bus, builder);

            return builder;
        }

        private void AddCommandsToEvent(Node node, StateBuilder builder)
        {
            foreach (Graphs.Event @event in node.Data.Events)
            {
                int eventId = builder.AddEvent(@event.TriggerID, @event.Condition);

                foreach (Action action in @event.Actions)
                {
                    (string module, string command, List<Tuple<string, string>> parameters) data =
                        ParseActionData(action);
                    string commandName = GetFullCommandName(data);

                    switch (@event.TriggerID)
                    {
                        case EnterEventId:
                            builder.AddEnter(commandName, data.parameters);

                            break;

                        case ExitEventId:
                            builder.AddExit(commandName, data.parameters);

                            break;
                        default:
                            builder.AddCommandToEvent(eventId, commandName, data.parameters);

                            break;
                    }
                }
            }
        }

        private void AddTransitions(IEnumerable<Edge> edges, StateBuilder builder)
        {
            foreach (Edge edge in edges)
            {
                TransitionBuilder transitionBuilder = new TransitionBuilder();
                transitionBuilder.AddDebugId($"{edge.SourceNode}:{edge.TargetNode}");

                transitionBuilder.ToNextStateOnEvent(
                    edge.TargetNode,
                    edge.Data.TriggerID == "" ? NodeData.Vertex_Initial : edge.Data.TriggerID);

                transitionBuilder.WithCondition(edge.Data.Condition);

                foreach (Action action in edge.Data.Actions)
                {
                    (string module, string command, List<Tuple<string, string>> parameters) actionData =
                        ParseActionData(action);
                    transitionBuilder.AddCommand(GetFullCommandName(actionData), actionData.parameters);
                }

                builder.AddTransition(transitionBuilder);
            }
        }

        private void AddSubHsm(Node rootNode, IBus bus, StateBuilder builder)
        {
            if (rootNode.NestedGraph == null)
                return;

            foreach (Node node in rootNode.NestedGraph.Nodes)
            {
                StateBuilder childBuilder = CreateChildBuilder(node, GetEdges(node.ID), bus);
                builder.AddChildState(childBuilder);
            }
        }

        private string GetFullCommandName((string module, string command, List<Tuple<string, string>> parameters) data)
        {
            return $"{data.module}.{data.command}";
        }

        private (string module, string command, List<Tuple<string, string>> parameters) ParseActionData(Action action)
        {
            string[] actionId = action.ID.Split('.');
            string module = actionId[0];
            string command = actionId[1];
            List<Tuple<string, string>> parameters = action.Parameters;

            return new ValueTuple<string, string, List<Tuple<string, string>>>(module, command, parameters);
        }

        private IEnumerable<Edge> GetEdges(string bySourceNodeId)
        {
            IEnumerable<Edge> edges = _sourceGraph.Edges.Where(edge => edge.SourceNode == bySourceNodeId);
            return edges;
        }
    }
}
