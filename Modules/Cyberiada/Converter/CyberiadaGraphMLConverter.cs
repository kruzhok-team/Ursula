using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Talent.Graphs
{
    /// <summary>
    /// Конвертер CyberiadaGraphML документа
    /// </summary>
    public class CyberiadaGraphMLConverter
    {
        private const string StandardVersion = "1.0";
        private readonly string _platform;
        private readonly string _platformVersion;

        /// <summary>
        /// Конструктор CyberiadaGraphML конвертера
        /// </summary>
        /// <param name="platform">Имя целевой платформы</param>
        /// <param name="platformVersion">Версия целевой платформы</param>
        public CyberiadaGraphMLConverter(string platform, string platformVersion)
        {
            _platform = platform;
            _platformVersion = platformVersion;
        }

        /// <summary>
        /// Десериализует XML элемент в CyberiadaGraphML документ
        /// </summary>
        /// <param name="xElement">Исходный XML элемент</param>
        /// <returns>CyberiadaGraphML документ</returns>
        /// <exception cref="ArgumentNullException">Исходный XML элемент равен null</exception>
        public CyberiadaGraphDocument Deserialize(XElement xElement)
        {
            if (xElement == null)
                throw new ArgumentNullException(nameof(xElement));

            XElement graphElement = xElement.Element(FullName("graph"));
            var graphDocument = CreateGraphDocument(graphElement);
            return graphDocument;
        }

        /// <summary>
        /// Десериализурует файл с XML разметкой в CyberiadaGraphML документ
        /// </summary>
        /// <param name="filePath">Путь до файла</param>
        /// <returns>CyberiadaGraphML документ</returns>
        public CyberiadaGraphDocument DeserializeFromFile(string filePath)
        {
            XDocument xml = XDocument.Load(filePath);
            return Deserialize(xml.Root);
        }

        /// <summary>
        /// Сериализует CyberiadaGraphML документ в XML элемент
        /// </summary>
        /// <param name="graphDocument">CyberiadaGraphML документ</param>
        /// <returns>XML элемент</returns>
        /// <exception cref="ArgumentNullException">Исходный CyberiadaGraphML документ равен null</exception>
        public XElement Serialize(CyberiadaGraphDocument graphDocument)
        {
            if (graphDocument == null)
                throw new ArgumentNullException(nameof(graphDocument));

            XNamespace nameSpace = "http://graphml.graphdrawing.org/xmlns";
            var root = new XElement(nameSpace + "graphml");
            var label = new XElement(FullName("data"), new XAttribute("key", "gFormat"), "Cyberiada-GraphML-1.0");
            root.Add(label);
            XElement documentElement = CreateXmlGraphDocument(graphDocument, root);
            CreateXmlEdges(graphDocument.RootGraph, documentElement);

            return root;
        }

        /// <summary>
        /// Сериализует CyberiadaGraphML документ в файл с XML разметкой
        /// </summary>
        /// <param name="graphDocument">CyberiadaGraphML документ</param>
        /// <param name="filePath">Путь до файла</param>
        public void SerializeToFile(CyberiadaGraphDocument graphDocument, string filePath)
        {
            XElement xmlElement = Serialize(graphDocument);
            var xmlDoc = new XDocument
            {
                Declaration = new XDeclaration("1.0", "UTF-8", null)
            };
            xmlDoc.Add(xmlElement);
            xmlDoc.Save(filePath);
        }

        #region Serialization

        private XElement CreateXmlGraphDocument(CyberiadaGraphDocument document, XElement parentElement)
        {
            var graphElement = CreateXmlGraph(document.RootGraph, parentElement);
            var stateMachineDefinition = new XElement(FullName("data"), new XAttribute("key", "dStateMachine"));
            graphElement.AddFirst(stateMachineDefinition);
            var metaDataElement = CreateXmlMetaData(document);
            stateMachineDefinition.AddAfterSelf(metaDataElement);
            if (!string.IsNullOrEmpty(document.ReferenceGraphId))
            {
                var referenceId = new XElement(FullName("data"), new XAttribute("key", "referenceGraphID"),
                    document.ReferenceGraphId);
                stateMachineDefinition.AddAfterSelf(referenceId);
            }

            if (!string.IsNullOrEmpty(document.Name))
            {
                var graphName = new XElement(FullName("data"), new XAttribute("key", "dName"), document.Name);
                stateMachineDefinition.AddAfterSelf(graphName);
            }

            return graphElement;
        }

        private XElement CreateXmlGraph(CyberiadaGraph graph, XElement parentElement)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            var graphElement = new XElement(FullName("graph"), new XAttribute("id", graph.ID),
                new XAttribute("edgedefault", "directed"));
            CreateXmlNodes(graph, graphElement);
            parentElement.Add(graphElement);
            return graphElement;
        }

        private XElement CreateXmlMetaData(CyberiadaGraphDocument document)
        {
            var metaDataElement = new XElement(FullName("node"), new XAttribute("id", "coreMeta"));
            AddNoteTypeToXmlElement(metaDataElement, "formal");
            AddNameToXmlElement(metaDataElement, "CGML_META");
            var sb = new StringBuilder();
            sb.AppendLine($"standardVersion/ {StandardVersion}");
            if (!string.IsNullOrEmpty(document.Target))
            {
                sb.Append('\n');
                sb.AppendLine($"target/ {document.Target}");
            }

            if (!string.IsNullOrEmpty(_platform))
            {
                sb.Append('\n');
                sb.AppendLine($"platform/ {_platform}");
            }

            if (!string.IsNullOrEmpty(_platformVersion))
            {
                sb.Append('\n');
                sb.AppendLine($"platformVersion/ {_platformVersion}");
            }

            AddDataToXmlElement(metaDataElement, sb.ToString());
            return metaDataElement;
        }

        private void CreateXmlEdges(CyberiadaGraph graph, XElement parentElement)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            foreach (Edge edge in graph.Edges)
                CreateXmlEdge(edge, parentElement);
        }

        private void CreateXmlEdge(Edge edge, XContainer parentElement)
        {
            var edgeElement = new XElement(FullName("edge"),
                new XAttribute("id", edge.ID),
                new XAttribute("source", edge.SourceNode),
                new XAttribute("target", edge.TargetNode));

            AddGeometryToXmlElement(edgeElement, "dLabelGeometry", edge.Data.VisualData.Position);

            if (string.IsNullOrEmpty(edge.Data.TriggerID) == false)
            {
                var sb = new StringBuilder();

                sb.Append(edge.Data.TriggerID);
                sb.Append(string.IsNullOrEmpty(edge.Data.Condition) ? "" : $"[{edge.Data.Condition}]");
                sb.AppendLine("/");

                if (edge.Data.Actions.Count > 0)
                {
                    foreach (Action action in edge.Data.Actions)
                    {
                        AppendActionLine(action, sb);
                    }

                    sb.AppendLine();
                }

                AddDataToXmlElement(edgeElement, sb.ToString());
            }

            parentElement.Add(edgeElement);
        }

        private void CreateXmlNodes(CyberiadaGraph graph, XElement parentElement)
        {
            foreach (Node node in graph.Nodes)
                CreateXmlNode(node, parentElement);
        }

        private void CreateXmlNode(Node node, XContainer parentElement)
        {
            var nodeElement = new XElement(FullName("node"), new XAttribute("id", node.ID));

            if (IsInitialNode(node))
                MarkAsInitial(nodeElement);

            AddNameToXmlElement(nodeElement, node.Data.VisualData.Name);
            AddGeometryToXmlElement(nodeElement, "dGeometry", node.Data.VisualData.Position);

            var sb = new StringBuilder();
            for (var i = 0; i < node.Data.Events.Count; i++)
            {
                var @event = node.Data.Events[i];
                sb.Append(@event.TriggerID);
                sb.Append(string.IsNullOrEmpty(@event.Condition) ? "" : $"[{@event.Condition}]"); // copied
                sb.AppendLine("/");

                foreach (Action action in @event.Actions)
                {
                    AppendActionLine(action, sb);
                }

                if (i < node.Data.Events.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            AddDataToXmlElement(nodeElement, sb.ToString());

            if (HasSubGraph(node))
            {
                CreateXmlGraph(node.NestedGraph, nodeElement);
            }

            parentElement.Add(nodeElement);
        }

        private void AddDataToXmlElement(XElement nodeElement, string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            var dataElement = new XElement(FullName("data"), new XAttribute("key", "dData"), data);
            nodeElement.Add(dataElement);
        }

        private void AddGeometryToXmlElement(XElement nodeElement, string keyName, Vector2 position)
        {
            XElement geometryElement = new XElement(FullName("data"), new XAttribute("key", keyName));
            geometryElement.Add(new XElement(FullName("point"),
                new XAttribute("x", position.x),
                new XAttribute("y", position.y)));

            nodeElement.Add(geometryElement);
        }

        private void AddNameToXmlElement(XElement nodeElement, string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            nodeElement.Add(new XElement(FullName("data"), new XAttribute("key", "dName"), name));
        }

        private void AddNoteTypeToXmlElement(XElement noteElement, string noteType) =>
            noteElement.Add(new XElement(FullName("data"), new XAttribute("key", "dNote"), noteType));

        private bool IsInitialNode(Node node) =>
            node.Data.Vertex == "initial";

        private void MarkAsInitial(XContainer nodeElement)
        {
            nodeElement.Add(new XElement(FullName("data"), new XAttribute("key", "dVertex"), "initial"));
        }

        private bool HasSubGraph(Node node) =>
            node.NestedGraph != null;

        private void AppendActionLine(Action action, StringBuilder target)
        {
            target.Append($"{action.ID}(");

            if (action.Parameters != null)
            {
                for (int n = 0; n < action.Parameters.Count; n++)
                {
                    if (n > 0) target.Append(", ");
                    target.Append($"{action.Parameters[n].Item2}");
                }
            }

            target.AppendLine(")");
        }

        #endregion

        #region Deserialization

        private void CreateNodes(
            XElement xElement, CyberiadaGraph graph,
            Node parentNode = null)
        {
            IEnumerable<XElement> nodes = xElement.Elements(FullName("node")).SelectMany(node=>node.Elements(FullName("data")).Where(data =>
                data.Attribute("key")?.Value == "dName" && data.Value != "CGML_META")).Select(data=>data.Parent);

            foreach (XElement nodeElement in nodes)
            {
                if (nodeElement!.Attribute("id")?.Value == "")
                {
                    continue;
                }

                Node node = CreateNode(nodeElement);
                node.ParentNode = parentNode;

                foreach (XElement subGraph in nodeElement.Elements(FullName("graph")))
                    node.NestedGraph = CreateGraph(subGraph, node);

                graph.AddNode(node);
            }
        }

        private Node CreateNode(XElement nodeElement)
        {
            string nodeId = nodeElement.Attribute("id")?.Value ?? "";
            NodeData data = CreateNodeData(nodeElement);

            foreach (XElement dataElement in nodeElement.Elements(FullName("data")))
            {
                string name = dataElement.Attribute("key")?.Value ?? "";
                switch (name)
                {
                    case "dName":
                        data.VisualData.Name = dataElement.Value;
                        break;
                    case "dGeometry":
                        data.VisualData.Position = GetGeometryData(dataElement).position;
                        break;
                    case "dData":
                        string dataString = dataElement.Value.Trim();

                        if (string.IsNullOrEmpty(dataString))
                        {
                            break;
                        }

                        string[] lines = dataString.Split(new[] { "\n\n" }, StringSplitOptions.None);

                        foreach (string line in lines)
                        {
                            string trigger = line[..line.IndexOf('/')];

                            int conditionStart = trigger.IndexOf('[');

                            string condition = null;
                            if (conditionStart > 0)
                            {
                                condition = trigger[(conditionStart + 1)..^1];
                                trigger = trigger[..conditionStart];
                            }

                            var nodeEvent = new Event(trigger);
                            nodeEvent.SetCondition(condition);
                            data.AddEvent(nodeEvent);

                            foreach (Action a in ParseActions(line))
                            {
                                nodeEvent.AddAction(a);
                            }
                        }

                        break;
                }
            }

            var node = new Node(nodeId, data);
            return node;
        }

        private NodeData CreateNodeData(XElement nodeElement)
        {
            XElement dataElement = nodeElement.Elements(FullName("data"))
                .FirstOrDefault(element => element.Attribute("key")?.Value == "dVertex");

            if (dataElement != null)
                return new NodeData(dataElement.Value);

            return new NodeData();
        }

        private void CreateEdges(XElement xElement, CyberiadaGraph graph)
        {
            IEnumerable<XElement> edges = xElement.Elements(FullName("edge"));

            foreach (XElement edgeElement in edges)
            {
                Edge edge = CreateEdge(edgeElement);

                graph.AddEdge(edge);
            }
        }

        private Edge CreateEdge(XElement edgeElement)
        {
            string edgeId = edgeElement.Attribute("id")?.Value ?? "";
            string sourceNodeId = edgeElement.Attribute("source")?.Value ?? "";
            string targetNodeId = edgeElement.Attribute("target")?.Value ?? "";

            EdgeData data = CreateEdgeData(edgeElement);

            foreach (XElement dataElement in edgeElement.Elements(FullName("data")))
            {
                string name = dataElement.Attribute("key")?.Value ?? "";

                switch (name)
                {
                    case "dLabelGeometry":
                        data.VisualData.Position = GetGeometryData(dataElement).position;
                        break;
                }
            }

            var edge = new Edge(edgeId, sourceNodeId, targetNodeId, data);
            return edge;
        }

        private EdgeData CreateEdgeData(XElement edgeElement)
        {
            XElement dataElement = edgeElement.Elements(FullName("data"))
                .FirstOrDefault(data => data.Attribute("key")?.Value == "dData");

            var edgeData = new EdgeData("");

            if (dataElement == null)
                return edgeData;

            string dataString = dataElement.Value;
            string trigger;
            string condition = "";

            Regex regex = new Regex(@"\[(.*?)\]");
            Match match = regex.Match(dataString);

            if (match.Success)
            {
                condition = match.Groups[1].Value;
                trigger = dataString[..match.Index];
            }
            else
            {
                trigger = dataString[..dataString.IndexOf('/')];
            }

            edgeData.SetTrigger(trigger);
            edgeData.SetCondition(condition);

            foreach (Action a in ParseActions(dataString))
            {
                edgeData.AddAction(a);
            }

            return edgeData;
        }

        private CyberiadaGraphDocument CreateGraphDocument(XElement graphElement)
        {
            var document = new CyberiadaGraphDocument();
            document.Name = graphElement.Elements()
                .FirstOrDefault(element => element.Attribute("key")?.Value == "dName")?.Value ?? "";
            document.ReferenceGraphId = graphElement.Elements()
                .FirstOrDefault(element => element.Attribute("key")?.Value == "referenceGraphID")?.Value ?? "";
            var metaDataElements = graphElement.Descendants(FullName("node"))
                .SelectMany(node => node.Elements(FullName("data"))).Where(data =>
                    data.Attribute("key")?.Value == "dName" && data.Value == "CGML_META").ToArray();

            if (metaDataElements.Length == 0)
            {
                document.Target = "";
            }
            else
            {
                if (metaDataElements.Length != 1)
                {
                    throw new ArgumentException($"Expected exactly one metadata node, but got {metaDataElements.Length}",
                        nameof(graphElement));
                }

                if (metaDataElements.First().Parent?.Parent != graphElement)
                {
                    throw new ArgumentException("Expected metadata node on top level of graphml document", nameof(graphElement));
                }

                var serializedMetaData = metaDataElements.First().Parent!.Elements(FullName("data"))
                    .FirstOrDefault(e => e.Attribute("key")?.Value == "dData")?.Value;
                if (serializedMetaData == null)
                {
                    throw new ArgumentException("Invalid metadata format", nameof(graphElement));
                }
            
                var metaData = new Dictionary<string, string>();
                var entries = serializedMetaData.Trim().Split("\n\n").ToList();
                foreach (var entry in entries)
                {
                    const string delimiter = "/ ";
                    var delimiterPosition = entry.IndexOf(delimiter, StringComparison.Ordinal);
                    var key = entry[..delimiterPosition];
                    var value = entry[(delimiterPosition + delimiter.Length)..];
                    metaData.Add(key, value);
                }

                document.Target = metaData.GetValueOrDefault("target", "");    
            }
            
            document.RootGraph = CreateGraph(graphElement);
            CreateEdges(graphElement, document.RootGraph);
            return document;
        }

        private CyberiadaGraph CreateGraph(
            XElement graphElement, Node parentNode = null)
        {
            string graphId = graphElement.Attribute("id")?.Value ?? "";
            var graphData = new GraphData();
            var graph = new CyberiadaGraph(graphId, graphData);
            CreateNodes(graphElement, graph, parentNode);
            return graph;
        }

        private (Vector2 position, Vector2 size) GetGeometryData(XElement dataElement)
        {
            XElement rectElement = dataElement.Element(FullName("rect"));
            XElement pointElement = dataElement.Element(FullName("point"));

            if (pointElement != null)
            {
                Vector2 position = ParseVector2("x", "y", pointElement);
                return (position, Vector2.zero);
            }

            if (rectElement != null)
            {
                Vector2 position = ParseVector2("x", "y", rectElement);
                Vector2 size = ParseVector2("width", "height", rectElement);
                return (position, size);
            }

            return (Vector2.zero, Vector2.zero);
        }

        private Vector2 ParseVector2(string firstKey, string secondKey, XElement dataElement)
        {
            string x = dataElement.Attribute(firstKey)?.Value ?? "";
            string y = dataElement.Attribute(secondKey)?.Value ?? "";
            return new Vector2(
                float.Parse(x, CultureInfo.InvariantCulture),
                float.Parse(y, CultureInfo.InvariantCulture));
        }

        private IEnumerable<Action> ParseActions(string source)
        {
            const string actionsPattern = @"(?<action>.*?)\((?<args>.*?)\)";
            const string argsPattern = @"(.+?)(?:,\s*|$)"; // TODO combine, together with trigger and condition?
            MatchCollection actionMatches = Regex.Matches(source, actionsPattern);

            List<Action> actions = new List<Action>(); // TODO remove lists

            foreach (Match actionMatch in actionMatches)
            {
                if (!actionMatch.Success) continue;

                Group argsGroup = actionMatch.Groups["args"];

                List<Tuple<string, string>> args = new List<Tuple<string, string>>();

                if (argsGroup != null && argsGroup.Success && argsGroup.Length > 0)
                {
                    MatchCollection argsMatches = Regex.Matches(argsGroup.Value, argsPattern);

                    foreach (Match argMatch in argsMatches)
                    {
                        if (!argMatch.Success) continue;

                        string parameterName = ""; // TODO get from config data

                        args.Add(new(parameterName, argMatch.Groups[1].Value));
                    }
                }

                actions.Add(new Action(actionMatch.Groups["action"].Value, args));
            }

            return actions;
        }

        #endregion

        private XName FullName(string localName) =>
            XName.Get(localName, "http://graphml.graphdrawing.org/xmlns");
    }
}