using System.Collections.Generic;

namespace Talent.Graphs
{
    /// <summary>
    /// Класс, представляющий переход в машине состояний 
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Идентификатор события перехода
        /// </summary>
        public string TriggerID { get; private set; }
        /// <summary>
        /// Условие перехода
        /// </summary>
        public string Condition { get; private set; }

        private readonly List<Action> _actions = new();

        /// <summary>
        /// Список поведений перехода
        /// </summary>
        public IReadOnlyList<Action> Actions => _actions;

        /// <summary>
        /// Конструктор перехода
        /// </summary>
        /// <param name="triggerID">Заданное событие</param>
        public Event(string triggerID)
        {
            TriggerID = triggerID;
        }

        /// <summary>
        /// Создает копию перехода
        /// </summary>
        /// <returns>Копия перехода</returns>
        public Event GetCopy()
        {
            Event resultEvent = new Event(TriggerID);
            resultEvent.SetCondition(Condition);

            foreach (Action action in _actions)
            {
                resultEvent.AddAction(action.GetCopy());
            }

            return resultEvent;
        }

        /// <summary>
        /// Изменяет идентификатор события для перехода
        /// </summary>
        /// <param name="triggerID">Идентификатор перехода</param>
        public void SetTrigger(string triggerID)
        {
            TriggerID = triggerID;
        }

        /// <summary>
        /// Изменяет условие для перехода
        /// </summary>
        /// <param name="condition">Заданное условие</param>
        public void SetCondition(string condition)
        {
            Condition = condition;
        }

        #region EventAction API

        /// <summary>
        /// Добавляет новое поведение для перехода
        /// </summary>
        /// <param name="action">Добавляемое поведение</param>
        public void AddAction(Action action)
        {
            _actions.Add(action);
        }

        /// <summary>
        /// Удаляет существующее поведение для перехода
        /// </summary>
        /// <param name="eventAction">Удаляемое поведение</param>
        public void RemoveAction(Action eventAction)
        {
            if (_actions.Contains(eventAction))
            {
                _actions.Remove(eventAction);
            }
        }

        #endregion

        /// <summary>
        /// Возвращает строковое представление перехода
        /// </summary>
        /// <returns>Строковое представление</returns>
        public override string ToString()
        {
            return $"{TriggerID}\n{string.Join("\n", Actions)}";
        }
    }
}
