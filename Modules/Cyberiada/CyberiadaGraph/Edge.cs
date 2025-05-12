using System;

namespace Talent.Graphs
{
    /// <summary>
    /// Элемент графа, представляющий ребро
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// Уникальный идентификатор ребра
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Уникальный идентификатор входящего узла
        /// </summary>
        public string SourceNode { get; private set; }
        
        /// <summary>
        /// Уникальный идентификатор исходящего узла
        /// </summary>
        public string TargetNode { get; private set; }

        /// <summary>
        /// Данные ребра
        /// </summary>
        public EdgeData Data { get; }

        /// <summary>
        /// Конструктор ребра
        /// </summary>
        /// <param name="id">Уникальный идентификатор ребра</param>
        /// <param name="sourceNode">Уникальный идентификатор входящего узла</param>
        /// <param name="targetNode">Уникальный идентификатор исходящего узла</param>
        /// <param name="data">Переход</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Edge(string id, string sourceNode, string targetNode, EdgeData data)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException($"Can't create Edge with id '{id}'. ID can't be null or empty");
            }

            if (string.IsNullOrEmpty(sourceNode))
            {
                throw new ArgumentNullException($"Can't create Edge with source node id '{sourceNode}'. ID can't be null or empty");
            }

            if (string.IsNullOrEmpty(targetNode))
            {
                throw new ArgumentNullException($"Can't create Edge with target node id '{targetNode}'. ID can't be null or empty");
            }

            ID = id;
            SourceNode = sourceNode;
            TargetNode = targetNode;
            Data = data;
        }

        /// <summary>
        /// Создает копию ребра
        /// </summary>
        /// <param name="data">Новый переход ребра</param>
        /// <param name="newID">Новый идентификатор ребра, если отсутствует, используется оригинальный идентификатор</param>
        /// <returns>Копия ребра</returns>
        public Edge GetCopy(EdgeData data, string newID = null)
        {
            if (newID == "")
            {
                throw new ArgumentNullException($"Can't copy Edge with newID '{newID}'. ID can't be null or empty");
            }

            Edge resultEdge = new Edge(newID ?? ID, SourceNode, TargetNode, data);

            return resultEdge;
        }
    }
}
