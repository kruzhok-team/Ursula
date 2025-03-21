namespace Talent.Graphs
{
    /// <summary>
    /// Класс, представляющий данные графа
    /// </summary>
    public class GraphData : IClonable<GraphData>
    { 
        /// <summary>
        /// Создает копию данных графа
        /// </summary>
        /// <returns>Копия данных графа</returns>
        public GraphData GetCopy()
        {
            GraphData resultData = new GraphData();
            return resultData;
        }
    }
}