using System;

namespace Talent.Graphs
{
    /// <summary>
    /// Элемент графа, представляющий узел
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Уникальный идентификатор узла
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Родительский узел данного узла
        /// </summary>
        public Node ParentNode { get; set; }

        /// <summary>
        /// Подграф хранимый в данном узле
        /// </summary>
        public CyberiadaGraph NestedGraph { get; set; }

        /// <summary>
        /// Данные узла
        /// </summary>
        public NodeData Data { get; }

        /// <summary>
        /// Конструктор узла
        /// </summary>
        /// <param name="id">Уникальный идентификатор узла</param>
        /// <param name="data">Состояние узла</param>
        /// <exception cref="ArgumentNullException">Если идентификатор равен null, выбрасывается исключение</exception>
        public Node(string id, NodeData data)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException($"Can't create Node with id '{id}'. ID can't be null or empty");
            }

            ID = id;
            Data = data;
        }

        /// <summary>
        /// Создает копию узла
        /// </summary>
        /// <param name="data">Состояние узла</param>
        /// <param name="parentNode">Родительский узел для копии узла</param>
        /// <param name="newID">Новый идентификатор узла, если отсутствует, используется оригинальный идентификатор</param>
        /// <returns>Копия узла</returns>
        public Node GetCopy(NodeData data, Node parentNode = null, string newID = null)
        {
            if (newID == "")
            {
                throw new ArgumentNullException($"Can't copy Node with newID '{newID}'. ID can't be null or empty");
            }

            Node resultNode = new Node(newID ?? ID, data);

            if (ParentNode != null)
            {
                resultNode.ParentNode = parentNode;
            }

            if (NestedGraph != null)
            {
                resultNode.NestedGraph = NestedGraph.GetCopy(NestedGraph.Data.GetCopy(), resultNode);
            }

            return resultNode;
        }
    }
}
