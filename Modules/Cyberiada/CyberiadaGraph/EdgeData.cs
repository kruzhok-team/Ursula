using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Talent.Graphs
{
    /// <summary>
    /// Класс, представляющий переход, содержащийся в ребре
    /// </summary>
    public class EdgeData : IClonable<EdgeData>
    {
        /// <summary>
        /// Событие перехода
        /// </summary>
        public string TriggerID { get; private set; }
        /// <summary>
        /// Условие перехода
        /// </summary>
        public string Condition { get; private set; }
        /// <summary>
        /// Визуальное представление перехода
        /// </summary>
        public EdgeVisualData VisualData { get; set; } = new();

        private readonly List<Action> _actions = new();

        /// <summary>
        /// Список поведений перехода
        /// </summary>
        public IReadOnlyList<Action> Actions => _actions;

        /// <summary>
        /// Конструктор перехода
        /// </summary>
        /// <param name="triggerID">Заданное событие</param>
        public EdgeData(string triggerID)
        {
            TriggerID = triggerID;
        }

        /// <summary>
        /// Создает копию перехода
        /// </summary>
        /// <returns>Копия перехода</returns>
        public EdgeData GetCopy()
        {
            EdgeData resultData = new EdgeData(TriggerID);
            resultData.SetCondition(Condition);

            foreach (var action in _actions)
            {
                resultData.AddAction(action.GetCopy());
            }

            resultData.VisualData.Position = VisualData.Position;

            return resultData;
        }

        /// <summary>
        /// Изменяет событие перехода
        /// </summary>
        /// <param name="triggerID">Новое событие</param>
        public void SetTrigger(string triggerID)
        {
            TriggerID = triggerID;
        }

        /// <summary>
        /// Изменяет условие перехода
        /// </summary>
        /// <param name="condition">Новое условие</param>
        public void SetCondition(string condition)
        {
            Condition = condition;
        }

        #region EventAction API

        /// <summary>
        /// Добавляет новое действие к переходу
        /// </summary>
        public void AddAction(Action eventAction)
        {
            _actions.Add(eventAction);
        }

        /// <summary>
        /// Удаляет существующее действие из перехода
        /// </summary>
        public void RemoveAction(Action eventAction)
        {
            if (_actions.Contains(eventAction))
            {
                _actions.Remove(eventAction);
            }
        }

        #endregion
    }

    /// <summary>
    /// Визуальное представление ребра
    /// </summary>
    public class EdgeVisualData
    {
        /// <summary>
        /// Визуальная позиция ребра
        /// </summary>
        public Vector2 Position { get; set; }
    }
}