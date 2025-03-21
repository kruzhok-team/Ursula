using System;
using System.Collections.Generic;
using Talent.Logic.Bus;

namespace Talent.Logic.HSM.Builders
{
    /// <summary>
    /// Класс для создания состояний в иерархической машине состояний (ИМС)
    /// </summary>
    public class StateBuilder
    {
        private readonly string _id;
        private readonly List<Command> _enter = new List<Command>();
        private readonly List<Command> _exit = new List<Command>();
        private readonly List<EventData> _eventToCommandData = new List<EventData>();
        private readonly List<Transition> _transitions = new List<Transition>();
        private readonly State _current = new State();
        private readonly IBus _bus;

        private string _label = "";
        private HierarchicalStateMachine _owner;
        private State _parent;

        /// <summary>
        /// Конструктор строителя состояний ИМС
        /// </summary>
        /// <param name="bus">Шина для обработки переходов</param>
        /// <param name="id">Опциональный параметр для идентификатора строителя состояний</param>
        public StateBuilder(IBus bus, string id = "")
        {
            _bus = bus;
            _id = id;
        }

        /// <summary>
        ///  Устанавливает строковую метку в создаваемое состояние
        /// </summary>
        /// <param name="label">Метка</param>
        /// <returns>Обновленный строитель состояний</returns>
        public StateBuilder AddLabel(string label)
        {
            _label = label;

            return this;
        }

        /// <summary>
        /// Добавляет дочернее состояние в создаваемое состояние
        /// </summary>
        /// <param name="builder">Дочернее состояние</param>
        /// <returns>Обновленный строитель состояний</returns>
        public StateBuilder AddChildState(StateBuilder builder)
        {
            _owner ??= new HierarchicalStateMachine();

            State childState = builder
                .WithParentState(_current)
                .Build();

            _owner.AddState(childState);

            _current.SubscribeChildEvents(childState);

            return this;
        }

        /// <summary>
        /// Добавляет команду входа в создаваемое состояние
        /// </summary>
        /// <param name="commandName">Имя команд</param>
        /// <param name="parameters">Список параметров для команды</param>
        /// <returns>Обновленный строитель состояний</returns>
        public StateBuilder AddEnter(string commandName, List<Tuple<string, string>> parameters)
        {
            _enter.Add(new Command(_bus, commandName, parameters));

            return this;
        }

        /// <summary>
        /// Добавляет команду выхода в создаваемое состояние
        /// </summary>
        /// <param name="commandName">Имя команд</param>
        /// <param name="parameters">Список параметров для команды</param>
        /// <returns>Обновленный строитель состояний</returns>
        public StateBuilder AddExit(string commandName, List<Tuple<string, string>> parameters)
        {
            _exit.Add(new Command(_bus, commandName, parameters));

            return this;
        }

        /// <summary>
        /// Добавляет переход в создаваемое состояние
        /// </summary>
        /// <param name="nextStateId">Уникальный идентификатор следующего состояния</param>
        /// <param name="eventId">Идентификатор события, срабатывающего при переходе</param>
        /// <param name="parameters">Опциональные параметры для перехода</param>
        /// <returns>Обновленный строитель состояний</returns>
        public StateBuilder AddTransition(string nextStateId, string eventId, string parameters = "")
        {
            _transitions.Add(new Transition(nextStateId, eventId, _bus, parameters));

            return this;
        }

        /// <summary>
        /// Добавляет переход в создаваемое состояние
        /// </summary>
        /// <param name="transitionBuilder">Строитель переходов</param>
        /// <returns>Обновленный строитель состояний</returns>
        public StateBuilder AddTransition(TransitionBuilder transitionBuilder)
        {
            _transitions.Add(transitionBuilder.AddBus(_bus).Build());

            return this;
        }

        /// <summary>
        /// Добавляет команду, которая будет выполняться при возникновении определенного события.
        /// </summary>
        /// <param name="eventIndex">Индекс события, к которому привязывается команда</param>
        /// <param name="commandName">Имя команды</param>
        /// <param name="parameters">Список параметров для команды</param>
        /// <returns>Обновленный строитель состояний</returns>
        public void AddCommandToEvent(int eventIndex, string commandName, List<Tuple<string, string>> parameters)
        {
            _eventToCommandData[eventIndex].AddCommandStorage(commandName, parameters);
        }

        /// <summary>
        /// Добавляет событие
        /// </summary>
        /// <param name="eventId">Идентификатор события</param>
        /// <param name="condition">Условие, при котором срабатывает событие</param>
        /// <returns>Индекс добавленного события в списке</returns>
        public int AddEvent(string eventId, string condition)
        {
            EventData eventData = new EventData(eventId, condition);
            _eventToCommandData.Add(eventData);

            return _eventToCommandData.IndexOf(eventData);
        }

        /// <summary>
        /// Строит и возвращает новое состояние ИМС
        /// </summary>
        /// <returns>Новое состояние</returns>
        public State Build()
        {
            Event[] eventToCommand = null;

            if (_eventToCommandData.Count > 0)
            {
                eventToCommand = new Event[_eventToCommandData.Count];

                for (int i = 0; i < _eventToCommandData.Count; i++)
                {
                    eventToCommand[i] = new Event(
                        _bus,
                        _eventToCommandData[i].EventId,
                        _eventToCommandData[i].CreateCommands(_bus),
                        _eventToCommandData[i].Parameters);
                }
            }

            _current.Init(
                _id,
                _label,
                _parent,
                _enter.ToArray(),
                _exit.ToArray(),
                eventToCommand,
                _transitions.ToArray(),
                _owner);

            return _current;
        }

        private StateBuilder WithParentState(State parent)
        {
            _parent = parent;

            return this;
        }
    }
}
