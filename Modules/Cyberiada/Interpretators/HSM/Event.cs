using System;
using System.Collections.Generic;
using Talent.Logic.Bus;

namespace Talent.Logic.HSM
{
    /// <summary>
    /// Класс, представляющий событие в иерархической машине состояний
    /// </summary>
    public class Event
    {
        private readonly IEventBus _bus;
        private readonly IEnumerable<Command> _commands;
        private readonly string _eventName;
        private readonly ConditionChecker _conditionChecker;

        /// <summary>
        /// Конструктор события
        /// </summary>
        /// <param name="bus">Шина событий</param>
        /// <param name="eventName">Имя события</param>
        /// <param name="commands">Команды, ассоциированные с данным событием</param>
        /// <param name="parameters">Опциональные параметры для вызова события</param>
        public Event(IBus bus, string eventName, IEnumerable<Command> commands, string parameters = "")
        {
            _eventName = eventName;
            _bus = bus;
            _commands = commands;

            _conditionChecker = new ConditionChecker(bus, parameters);
        }

        /// <summary>
        /// Активирует событие с помощью добавления слушателеля событий в шину событий
        /// </summary>
        public void Activate()
        {
            _bus.AddEventListener(_eventName, Receive);
        }

        /// <summary>
        /// Деактивирует событие, удаляя слушатель событий из шины событий
        /// </summary>
        public void Deactivate()
        {
            _bus.RemoveEventListener(_eventName, Receive);
        }

        private bool Receive(List<Tuple<string, string>> parameters = null)
        {
            if (!_conditionChecker.Check())
            {
                return false;
            }

            foreach (Command commandStorage in _commands)
            {
                commandStorage.Make();
            }

            return false;
        }
    }
}
