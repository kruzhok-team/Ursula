using System.Collections.Generic;

namespace Talent.Graphs
{
    /// <summary>
    /// Класс, представляющий граф в документе CyberiadaGraphML
    /// </summary>
    public class CyberiadaGraph
    {
        /// <summary>
        /// Уникальный идентификатор графа
        /// </summary>
        public string ID { get; }

        private readonly Dictionary<string, Node> _nodes = new();

        /// <summary>
        /// Корневые узлы, которые содержит граф, исключая дочерние узлы
        /// </summary>
        public IReadOnlyCollection<Node> Nodes => _nodes.Values;

        private readonly HashSet<Edge> _edges = new();

        /// <summary>
        /// Корневые ребра, которые содержит граф
        /// </summary>
        public IReadOnlyCollection<Edge> Edges => _edges;

        /// <summary>
        /// Данные графа
        /// </summary>
        public GraphData Data { get; }

        public CyberiadaGraph(string id, GraphData data)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentNullException($"Can't create Graph with id '{id}'. ID can't be null or empty");
            }

            ID = id;
            Data = data;
        }

        /// <summary>
        /// Создает копию графа
        /// </summary>
        /// <param name="data">Исходные данные графа</param>
        /// <param name="parentNode">Родительский узел графа, устанавливается как родитель для всех узлов данного графа</param>
        /// <param name="newID">Новый идентификатор графа, если отсутствует, используется оригинальный идентификатор</param>
        /// <returns>Копия графа</returns>
        public CyberiadaGraph GetCopy(GraphData data, Node parentNode = null, string newID = null)
        {
            if (newID == "")
            {
                throw new System.ArgumentNullException($"Can't copy Graph with newID '{newID}'. ID can't be null or empty");
            }

            CyberiadaGraph resultGraph = new CyberiadaGraph(newID ?? ID, data);

            foreach (Node node in Nodes)
            {
                resultGraph.AddNode(node.GetCopy(node.Data.GetCopy(), parentNode));
            }

            foreach (Edge edge in Edges)
            {
                resultGraph.AddEdge(edge.GetCopy(edge.Data.GetCopy()));
            }

            return resultGraph;
        }

        /// <summary>
        /// Проверяет граф на равенство с другим графом
        /// </summary>
        /// <param name="graph">Другой граф</param>
        /// <param name="comparer">Сравниватель графов</param>
        /// <returns>true если графы равны, иначе false</returns>
        public bool IsGraphEqual(CyberiadaGraph graph, IEqualityComparer<CyberiadaGraph> comparer)
        {
            return comparer.Equals(this, graph);
        }

        #region Nodes API

        /// <summary>
        /// Проверяет, есть ли в графе узел с определенным уникальным идентификатором
        /// </summary>
        /// <param name="id">Уникальный идентификатор узла</param>
        /// <returns>true если узел найден, иначе false</returns>
        public bool HasNode(string id)
        {
            return id != null && _nodes.ContainsKey(id);
        }

        /// <summary>
        /// Пытается найти узел с определенным уникальным идентификатором в графе
        /// </summary>
        /// <param name="id">Уникальный идентификатор узла</param>
        /// <param name="node">Возвращает узел, если узел с таким соответствующим идентификатором есть в графе, иначе null</param>
        /// <returns>true если узел найден, иначе false</returns>
        public bool TryGetNode(string id, out Node node)
        {
            return _nodes.TryGetValue(id, out node);
        }

        /// <summary>
        /// Добавляет новый узел в граф
        /// </summary>
        /// <param name="node">Добавляемый узел</param>
        public void AddNode(Node node)
        {
            _nodes[node.ID] = node;
        }

        /// <summary>
        /// Удаляет существующий узел из графа
        /// </summary>
        /// <param name="node">Удаляемый узел</param>
        public void DeleteNode(Node node)
        {
            if (HasNode(node.ID))
            {
                _nodes.Remove(node.ID);
            }
        }

        #endregion

        #region Edges API

        /// <summary>
        /// Добавляет новое ребро в граф
        /// </summary>
        /// <param name="edge">Добавляемое ребро</param>
        public void AddEdge(Edge edge)
        {
            _edges.Add(edge);
        }

        /// <summary>
        /// Удаляет существующее ребро из графа
        /// </summary>
        /// <param name="edge">Удаляемое ребро</param>
        public void DeleteEdge(Edge edge)
        {
            _edges.Remove(edge);
        }

        #endregion
    }
}