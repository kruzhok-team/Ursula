using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Talent.Graphs
{
    /// <summary>
    /// Сравниватель графов, использующий полное сравнение
    /// </summary>
    public class VerboseGraphComparer : IEqualityComparer<CyberiadaGraph>
    {
        /// <summary>
        /// Проверяет два графа на равенство
        /// </summary>
        /// <param name="graph">Первый граф</param>
        /// <param name="otherGraph">Второй граф</param>
        /// <returns>true, если графы равны, иначе false</returns>
        public bool Equals(CyberiadaGraph graph, CyberiadaGraph otherGraph)
        {
            return ConvertToString(graph) == ConvertToString(otherGraph);
        }

        public int GetHashCode(CyberiadaGraph graph) => graph.GetHashCode();

        private string ConvertToString(CyberiadaGraph graph)
        {
            StringBuilder stringBuilder = new StringBuilder();

            Dictionary<Node, CyberiadaGraph> graphByNode = graph.Nodes.ToDictionary(node => node, _ => graph);
            LinkedList<Node> nodes = new LinkedList<Node>(graph.Nodes);

            while (nodes.Count > 0)
            {
                LinkedListNode<Node> item = nodes.First;
                stringBuilder.Append($"NODE({item.Value.ID})({item.Value.Data.Vertex}(");
                stringBuilder.AppendLine($"NodeData({item.Value.Data.VisualData.Name})({item.Value.Data.VisualData.Position.ToString("F1")})");

                foreach (Event @event in item.Value.Data.Events)
                {
                    stringBuilder.AppendLine($"{@event}\n");
                }

                stringBuilder.AppendLine(")");
                if (item.Value.ParentNode != null)
                {
                    stringBuilder.AppendLine($"{nameof(item.Value.ParentNode)}={item.Value.ParentNode.ID}");
                }

                if (item.Value.NestedGraph != null)
                {
                    CyberiadaGraph innerGraph = item.Value.NestedGraph;
                    stringBuilder.AppendLine($"GRAPH({innerGraph.ID})");
                    LinkedListNode<Node> previousItem = item;
                    foreach (var node in innerGraph.Nodes)
                    {
                        var nextItem = nodes.AddAfter(previousItem, node);
                        graphByNode.Add(node, innerGraph);
                        previousItem = nextItem;
                    }
                }

                CyberiadaGraph currentGraph = graphByNode[item.Value];

                if (currentGraph.Nodes.Last() == item.Value)
                {
                    foreach (Edge edge in currentGraph.Edges)
                    {
                        AddEdge(edge, stringBuilder);
                    }
                }

                graphByNode.Remove(item.Value);
                nodes.RemoveFirst();
            }

            return stringBuilder.ToString();
        }

        private void AddEdge(Edge edge, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("EDGE");
            stringBuilder.AppendLine($"{nameof(edge.SourceNode)}={edge.SourceNode}");
            stringBuilder.AppendLine($"{nameof(edge.TargetNode)}={edge.TargetNode}");
            stringBuilder.AppendLine($"EdgeData({edge.Data.TriggerID})({edge.Data.Condition})({edge.Data.VisualData.Position.ToString("F1")})");

            foreach (Action action in edge.Data.Actions)
            {
                stringBuilder.AppendLine($"{action}");
            }

            stringBuilder.Append('\n');
        }
    }
}
