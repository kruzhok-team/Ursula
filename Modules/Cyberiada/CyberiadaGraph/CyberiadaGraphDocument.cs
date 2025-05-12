namespace Talent.Graphs
{
    /// <summary>
    /// Класс, представляющий CyberiadaGraphML документ
    /// </summary>
    public class CyberiadaGraphDocument
    {
        /// <summary>
        /// Корневой граф документа
        /// </summary>
        public CyberiadaGraph RootGraph { get; set; }

        /// <summary>
        /// Целевой дрон, к которому применяется корневой документ
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Имя документа
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Уникальный идентификатор оригинального корневого графа
        /// </summary>
        public string ReferenceGraphId { get; set; }

        /// <summary>
        /// Создает копию документа CyberiadaGraphML
        /// </summary>
        /// <param name="newID">Новый идентификатор корневого графа, если отсутствует, используется оригинальный идентификатор </param>
        /// <returns>Возвращает копию документа</returns>
        public CyberiadaGraphDocument GetCopy(string newID = null)
        {
            CyberiadaGraphDocument document = new CyberiadaGraphDocument
            {
                RootGraph = RootGraph.GetCopy(RootGraph.Data.GetCopy(), null, newID),
                Target = Target,
                Name = Name,
                ReferenceGraphId = ReferenceGraphId
            };

            return document;
        }
    }
}