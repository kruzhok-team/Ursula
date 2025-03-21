using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Godot;

namespace Modules.HSM
{
    public static class GraphMLParser
    {
        private static readonly string NamespaceUri = "http://graphml.graphdrawing.org/xmlns";

        // Основной метод для парсинга GraphML файла
        public static Graph ParseGraphML(string filePath)
        {
            string fullPath = ProjectSettings.GlobalizePath(filePath);

            //bool isExist = File.Exists(filePath);

            //if (!isExist)
            //{
            //    GD.PrintErr($"Файл графа не найден: {filePath}");
            //    return null;
            //}

            XmlDocument doc = new XmlDocument();
            doc.Load(fullPath);

            // Создаем менеджер пространств имён и добавляем используемое пространство
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("g", NamespaceUri);

            // Получаем корневой элемент с пространством имён
            XmlElement root = doc.DocumentElement;
            if (root == null || root.Name != "graphml")
            {
                GD.PrintErr("Неверный формат файла GraphML.");
                return null;
            }

            // Создаем объект графа
            Graph graph = new Graph();

            // Парсим основной граф с учётом пространства имён
            XmlNode graphNode = root.SelectSingleNode("g:graph", nsmgr);
            if (graphNode != null)
            {
                graph.Id = graphNode.Attributes["id"]?.Value ?? "Unnamed";
                ParseGraphContent(graphNode, graph, nsmgr);
            }

            return graph;
        }

        // Парсинг содержимого графа (узлы, рёбра и вложенные графы)
        private static void ParseGraphContent(XmlNode graphNode, Graph graph, XmlNamespaceManager nsmgr)
        {
            // Чтение узлов с учётом пространства имён
            XmlNodeList nodes = graphNode.SelectNodes("g:node", nsmgr);
            foreach (XmlNode node in nodes)
            {
                NodeData nodeData = ParseNode(node, nsmgr);
                graph.Nodes.Add(nodeData);

                // Проверка на наличие вложенного графа
                XmlNode subGraphNode = node.SelectSingleNode("g:graph", nsmgr);
                if (subGraphNode != null)
                {
                    Graph subGraph = new Graph { Id = subGraphNode.Attributes["id"]?.Value ?? nodeData.Id };
                    nodeData.SubGraph = subGraph;
                    ParseGraphContent(subGraphNode, subGraph, nsmgr);
                }
            }

            // Чтение рёбер с учётом пространства имён
            XmlNodeList edges = graphNode.SelectNodes("g:edge", nsmgr);
            foreach (XmlNode edge in edges)
            {
                EdgeData edgeData = ParseEdge(edge, nsmgr);
                graph.Edges.Add(edgeData);
            }
        }

        // Парсинг узлов графа
        private static NodeData ParseNode(XmlNode node, XmlNamespaceManager nsmgr)
        {
            NodeData nodeData = new NodeData();
            nodeData.Id = node.Attributes["id"].Value;

            XmlNodeList dataNodes = node.SelectNodes("g:data", nsmgr);
            foreach (XmlNode data in dataNodes)
            {
                string key = data.Attributes["key"].Value;
                string value = data.InnerText.Trim();
                nodeData.Data.Add(key, value);
            }

            return nodeData;
        }

        // Парсинг рёбер графа
        private static EdgeData ParseEdge(XmlNode edge, XmlNamespaceManager nsmgr)
        {
            EdgeData edgeData = new EdgeData();
            edgeData.Id = edge.Attributes["id"].Value;
            edgeData.Source = edge.Attributes["source"].Value;
            edgeData.Target = edge.Attributes["target"].Value;

            XmlNodeList dataNodes = edge.SelectNodes("g:data", nsmgr);
            foreach (XmlNode data in dataNodes)
            {
                string key = data.Attributes["key"].Value;
                string value = data.InnerText.Trim();
                edgeData.Data.Add(key, value);
            }

            return edgeData;
        }
    }

    // Класс для графа
    public class Graph
    {
        public string Id { get; set; }
        public List<NodeData> Nodes { get; set; } = new List<NodeData>();
        public List<EdgeData> Edges { get; set; } = new List<EdgeData>();
    }

    // Класс для узла
    public class NodeData
    {
        public string Id { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public Graph SubGraph { get; set; } // Вложенный граф (если существует)
    }

    // Класс для ребра
    public class EdgeData
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}