using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static GMLAlgorithm;

public class VariableHolder<T>
{
    public T Value { get; set; }

    public VariableHolder(T value)
    {
        Value = value;
    }
}

public class GMLAlgorithm
{
    public APIDictionary apiDictionary;

    public State currentState = null;

    public static GMLAlgorithm Load(string filePath, InteractiveObject interactiveObject)
    {
        var alg = new GMLAlgorithm();
        alg.LoadXML(filePath, interactiveObject);
        return alg;
    }

    public List<State> States;
    public List<Link> Links;

    private void LoadXML(string filePath, InteractiveObject interactiveObject)
    {
        if (string.IsNullOrWhiteSpace(filePath)) 
            return;

        //bool isExist = File.Exists(filePath);

        //if (!isExist)
        //{
        //    GD.PrintErr($"Файл графа не найден: {filePath}");
        //    return;
        //}

        apiDictionary = new APIDictionary(interactiveObject);

        var graph = GraphMLParser.ParseGraphML(filePath);

        States = ParseStates(graph);
        Links = ParseLinks(graph, States);

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

        var entry = currentState.Signals.Find(p => p.ActionName == "entry");
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

    private List<State> ParseStates(Graph graph)
    {
        if (graph == null) return null;

        List<State> states = new List<State>();

        foreach (var node in graph.Nodes)
        {
            if (node.Id == "coreMeta") continue;

            var state = new State { Id = node.Id, gml = this };

            if (node.SubGraph != null)
            {
                states.AddRange(ParseStates(node.SubGraph));
            }

            if (node.Data.ContainsKey("dName"))
            {
                state.Name = node.Data["dName"];
            }

            if (node.Data.ContainsKey("dData"))
            {
                var signalData = node.Data["dData"].Split("\n");
                state.Signals.AddRange(ParseSignals(signalData, state));
            }

            if (node.Data.ContainsKey("dVertex"))
            {
                state.nodeType = node.Data["dVertex"];
            }

            states.Add(state);
        }

        return states;
    }

    private List<Signal> ParseSignals(string[] signalData, State parent)
    {
        var signals = new List<Signal>();
        Signal currentSignal = null;

        foreach (var row in signalData)
        {
            var data = row.Replace("\r", "");

            if (string.IsNullOrEmpty(data)) continue;

            if (data.Contains("/"))
            {
                currentSignal?.Subscribe(); // Подписываем предыдущий сигнал 

                currentSignal = Signal.ParseFromData(data, this);
                currentSignal.signalParent = parent;

                signals.Add(currentSignal);
            }
            else if (currentSignal != null)
            {
                currentSignal.Methods.Add(ModuleMethod.ParseFromData(data, this));
            }
        }

        currentSignal.Subscribe();


        return signals;
    }

    private List<Link> ParseLinks(Graph graph, List<State> states)
    {
        List<Link> links = new List<Link>();

        if (graph == null) return links;

        foreach (var edge in graph.Edges)
        {
            var link = new Link
            {
                From = states.Find(p => p.Id == edge.Source),
                To = states.Find(p => p.Id == edge.Target),
                gml = this
            };

            if (edge.Data.ContainsKey("dData"))
            {
                link.When = Signal.ParseFromData(edge.Data["dData"], this);

                link.When.linkParent = link;

                link.When.SubscribeForLinks();

                link.Subscribe();
            }

            links.Add(link);
        }

        return links;
    }

    public static string[] ExtractArguments(string input)
    {
        string pattern = @"\((.*?)\)";
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            string arguments = match.Groups[1].Value;

            if (!string.IsNullOrWhiteSpace(arguments))
            {
                return arguments.Split(',');
            }
        }

        return new string[0];
    }

    public static string FixFormat(string row)
    {
        var source = row.Replace("\r", "").Replace(".(", "(");

        var split = source.Split('(');
        var path = split[0];

        var result = path + "(";

        var values = new List<string>();
        for (int i = 1; i < split.Length; i++)
        {
            var param = split[i].Replace(")", "");
            if (param.Contains('.'))
            {
                var value = param.Split('.');
                values.Add(value[1]);
            }
            else if (!string.IsNullOrEmpty(param)) values.Add(param);
        }
        var parameters = string.Join(",", values.ToArray());
        result += parameters + ")";

        return result;
    }

    public class Signal
    {
        public GMLAlgorithm gml;

        public string Module { get; set; }
        public string ActionName { get; set; }
        public Condition _Condition { get; set; }
        public List<ModuleMethod> Methods { get; set; } = new List<ModuleMethod>();

        public GMLActionHolder OnExecute = null;

        public State signalParent = null; // если пустой значит событие в линке
        public Link linkParent = null;

        private void ConnectAction()
        {
            if (gml.apiDictionary.HasSignal(Module, ActionName))
                OnExecute = gml.apiDictionary.SignalDictionary[Module][ActionName];
            else
            if (OnExecute == null)
                OnExecute = new(ActionName);
        }

        public void SubscribeForLinks()
        {
            ConnectAction();
        }

        public void Subscribe()
        {
            ConnectAction();

            OnExecute.Subscribe(this, () =>
            {
                ExecuteMethodsPayload();
            });
        }

        public void ExecuteMethodsPayload()
        {
            var availableSignal = signalParent == null || gml.currentState == signalParent;
            var availableSignalByLink = linkParent == null || gml.currentState == linkParent.From;

            if (availableSignal && availableSignalByLink)
            {
                ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вызван сигнал {Module}.{ActionName}{(signalParent != null ? $" в состоянии [{signalParent.Name}]" : $" в связи [{linkParent.From.Name}] -> [{linkParent.To.Name}]")}");

                bool cond = true;
                if (_Condition != null)
                {
                    cond = _Condition.EvaluateCondition();
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вычислено условие выполнения сигнала {_Condition} -> {cond}");
                }

                if (cond)
                {
                    foreach (var method in Methods)
                    {
                        method.Execute();
                    }
                }
                else
                {
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Сигнал {Module}.{ActionName} отозван условие не выполнено");
                }
            }
        }

        public static Signal ParseFromData(string row, GMLAlgorithm gml)
        {
            var signal = new Signal();

            var splitedRow = new string[1];

            if (row.Contains("\n")) // значит что этот сигнал находится в линке, в этом случае добавляем в массив методов все эти методы
            {
                splitedRow = row.Split("\n");

                for (int i = 1; i < splitedRow.Length; i++)
                {
                    var method = ModuleMethod.ParseFromData(splitedRow[i], gml);
                    signal.Methods.Add(method);
                }
            }
            else
            {
                splitedRow[0] = row;
            }

            if (splitedRow[0].Contains('['))
            {
                var s = splitedRow[0].Split('[');
                signal._Condition = Condition.ParseFromData(s[1], gml);
                signal._Condition.gml = gml;

                splitedRow[0] = s[0]; // строка с сигналом без условий
            }

            if (splitedRow[0].Contains('.'))
            {
                var s = splitedRow[0].Split('.');
                signal.Module = s[0];
                signal.ActionName = s[1].Replace("/", "").Replace("\r", "");
            }
            else
            {
                signal.ActionName = splitedRow[0].Replace("/", "").Replace("\r", "");
            }

            signal.gml = gml;

            return signal;
        }

        public override string ToString()
        {
            return $"(Signal) {Module}.{ActionName}";
        }
    }

    public class Condition
    {
        public Variable LeftVar { get; set; }
        public Variable RightVar { get; set; }
        public string Operator { get; set; }

        public GMLAlgorithm gml;

        public override string ToString()
        {
            return $"{LeftVar} {Operator} {RightVar}";
        }

        public static Condition ParseFromData(string condition, GMLAlgorithm gml)
        {
            var result = new Condition();
            result.gml = gml;

            var formated = condition.Replace("/", "").Replace("]", "").Replace(" ", "");
            var splited = SplitByEx(formated);

            result.LeftVar = Variable.ParseFromData(splited.splits[0], gml);
            result.RightVar = Variable.ParseFromData(splited.splits[1], gml);

            result.Operator = splited.oper;

            return result;
        }

        private static (string[] splits, string oper) SplitByEx(string row)
        {
            string[] operators = new[] { "!=", ">=", "<=", ">", "<", "=" };

            foreach (var op in operators)
            {
                if (row.Contains(op))
                {
                    return (row.Split(op), op);
                }
            }

            return (null, null);
        }

        public bool EvaluateCondition()
        {
            var v1 = LeftVar.GetValue();
            var v2 = RightVar.GetValue();
            var value1 = float.Parse(v1);
            var value2 = float.Parse(v2);

            switch (Operator)
            {
                case "!=":
                    return !value1.Equals(value2);
                case ">=":
                    return value1 >= value2;
                case "<=":
                    return value1 <= value2;
                case ">":
                    return value1 > value2;
                case "<":
                    return value1 < value2;
                case "=":
                    return value1 == value2;
                default:
                    throw new ArgumentException("Unsupported operator: " + Operator);
            }
        }
    }

    public class Variable
    {
        public string ValueOnly { get; set; }
        public string Module { get; set; }
        public string VarName { get; set; }

        public GMLAlgorithm gml;

        public static Variable ParseFromData(string data, GMLAlgorithm gml)
        {
            var variable = new Variable();
            variable.gml = gml;

            if (data.Contains('.'))
            {
                var s = data.Split('.');
                variable.Module = s[0];
                variable.VarName = s[1].Replace("/", "").Replace("\r", "");
            }
            else
            {
                variable.ValueOnly = data;
            }

            return variable;
        }

        public string GetValue()
        {
            if (!string.IsNullOrEmpty(ValueOnly))
            {
                return ValueOnly;
            } 
            else if (gml.apiDictionary.HasVar(Module, VarName))
            {
                return gml.apiDictionary.VariableDictionary[Module][VarName].Value.ToString();
            }
            else
            {
                GD.Print($"Variable {ToString()} is not defined");
                return "0";
            }
             
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ValueOnly))
            {
                return $"{ValueOnly}";
            }
            else
            {
                return $"[{Module}.{VarName}] {GetValue()}";
            }
        }
    }

    public class ModuleMethod
    {
        public GMLAlgorithm gml;

        public string Name { get; set; }
        public string Method { get; set; }

        private string[] parameters;

        public static ModuleMethod ParseFromData(string row, GMLAlgorithm gml)
        {
            row = FixFormat(row);

            var method = new ModuleMethod();

            method.parameters = ExtractArguments(row);

            if (row.Contains('.'))
            {
                var s = row.Split('.');
                method.Name = s[0];
                method.Method = s[1].Replace("/", "");
            }
            else
            {
                method.Method = row.Replace("/", "");
            }

            if (method.Method.Contains("("))
                method.Method = method.Method.Split('(')[0];

            method.gml = gml;

            return method;
        }

        public void Execute()
        {
            ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вызов метода {Name}.{Method}({string.Join(',', parameters)})");

            var p1 = parameters.Length > 0 ? parameters[0] : "";
            var p2 = parameters.Length > 1 ? parameters[1] : "";

            gml.apiDictionary.GetMethod(Name, Method).Invoke(p1, p2);
        }

        public override string ToString()
        {
            return $"(Method) {Name}.{Method}";
        }
    }

    public class State
    {
        public string Name { get; set; }

        public GMLAlgorithm gml;

        public string nodeType = "";

        public string Id { get; set; }
        public List<Signal> Signals { get; set; } = new List<Signal>(); // Сигналы, которые выполняются внутри состояния (обычно это entry и exit)

        public override string ToString()
        {
            return $"{Id}({Name})";
        }

    }

    public class Link
    {
        public GMLAlgorithm gml;

        public Signal When { get; set; }
        public State From { get; set; }
        public State To { get; set; }

        public void Subscribe()
        {
            When.OnExecute.Subscribe(When, () =>
            {
                bool cond = true;
                if (When._Condition != null)
                {
                    cond = When._Condition.EvaluateCondition();
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вычислено условие перехода состояний {When._Condition} -> {cond}");
                }

                if (gml.currentState == From)
                {
                    if (cond)
                    {
                        //ContextMenu.ShowMessageS($"{gml.GetId()} Переход состояния из {From.nodeType} в {To.nodeType} по сигналу {When.Module}.{When.ActionName}");
                        ContextMenu.ShowMessageS($"{gml.GetPrefix()} Переход из [{From.Name}] в [{To.Name}] по событию {When.Module}.{When.ActionName}");                   

                        var exit = From.Signals.Find(p => p.ActionName == "exit");
                        if (exit != null)
                            exit.OnExecute.Invoke();

                        When.ExecuteMethodsPayload();

                        gml.currentState = To;

                        var entry = To.Signals.Find(p => p.ActionName == "entry");
                        if (entry != null)
                            entry.OnExecute.Invoke();


                    }
                }
                else 
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Отмена перехода из [{From.Name}] в [{To.Name}] по событию {When.Module}.{When.ActionName} так как алгоритм уже перешёл в состояние {gml.currentState.Name}");

            });
        }

        public override string ToString()
        {
            return $"{From} -> {To}: {When?.Module}.{When?.ActionName}";
        }
    }
}


