using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Talent.Graphs
{
    /// <summary>
    /// Класс, представляющий состояние узла
    /// </summary>
    public class NodeData : IClonable<NodeData>
    {
        /// <summary>
        /// Начальное псевдосостояние
        /// </summary>
        public const string Vertex_Initial = "initial";

        /// <summary>
        /// Конечное состояние
        /// </summary>
        public const string Vertex_Final = "final";

        /// <summary>
        /// Псевдосостояние выбора
        /// </summary>
        public const string Vertex_Choice = "choice";

        /// <summary>
        /// Завершающее псевдосостояние («исключение»)
        /// </summary>
        public const string Vertex_Terminate = "terminate";

        /// <summary>
        /// Визуальное представление узла
        /// </summary>
        public NodeVisualData VisualData { get; private set; } = new();
       
        /// <summary>
        /// Тип вершины
        /// </summary>
        public string Vertex { get; private set; }

        private readonly List<Event> _events = new();

        /// <summary>
        /// Список переходов состояния
        /// </summary>
        public IReadOnlyList<Event> Events => _events;

        /// <summary>
        /// Конструктор состояния
        /// </summary>
        /// <param name="vertex">Тип вершины</param>
        public NodeData(string vertex = "")
        {
            Vertex = vertex;
        }

        /// <summary>
        /// Создает копию состояния
        /// </summary>
        /// <returns>Копия состояния</returns>
        public NodeData GetCopy()
        {
            NodeData resultData = new NodeData(Vertex);

            foreach (Event nodeEvent in _events)
            {
                resultData.AddEvent(nodeEvent.GetCopy());
            }

            resultData.VisualData.Name = VisualData.Name;
            resultData.VisualData.Position = VisualData.Position;

            return resultData;
        }

        #region Events API

        /// <summary>
        /// Добавляет новый переход в состояние
        /// </summary>
        /// <param name="nodeEvent">Добавляемый переход</param>
        public void AddEvent(Event nodeEvent)
        {
            if (!_events.Contains(nodeEvent))
            {
                _events.Add(nodeEvent);
            }
        }

        /// <summary>
        /// Удаляет существующий переход из состояния
        /// </summary>
        /// <param name="nodeEvent">Удаляемый переход</param>
        public void RemoveEvent(Event nodeEvent)
        {
            _events.Remove(nodeEvent);
        }

        #endregion
    }

    /// <summary>
    /// Визуальное представление узла
    /// </summary>
    public class NodeVisualData
    {
        /// <summary>
        /// Имя узла
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Визуальная позиция узла
        /// </summary>
        public Vector2 Position { get; set; }
    }
}