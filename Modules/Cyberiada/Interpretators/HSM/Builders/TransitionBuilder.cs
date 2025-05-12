using System;
using System.Collections.Generic;
using Talent.Logic.Bus;

namespace Talent.Logic.HSM.Builders
{
    /// <summary>
    /// Класс, используемый для создания переходов в иерархической машине состояний (ИМС)
    /// </summary>
    public class TransitionBuilder
    {
        private readonly CommandsData _commandsData = new CommandsData();

        private IBus _bus;
        private string _nextStateId = "";
        private string _eventId = "";
        private string _parameters = "";
        private string _id = "";

        /// <summary>
        /// Добавляет шину, ассоциированный с создаваемым переходом
        /// </summary>
        /// <param name="bus">Шина</param>
        /// /// <returns>Обновленный строитель переходов</returns>
        public TransitionBuilder AddBus(IBus bus)
        {
            _bus = bus;

            return this;
        }

        /// <summary>
        /// Устанавливает следующий идентификатор состояния и события для перехода
        /// </summary>
        /// <param name="nextStateId">Следующий идентификатор состояния</param>
        /// <param name="eventId">Идентификатор события</param>
        /// <returns>Обновленный строитель переходов</returns>
        public TransitionBuilder ToNextStateOnEvent(string nextStateId, string eventId)
        {
            _nextStateId = nextStateId;
            _eventId = eventId;

            return this;
        }

        /// <summary>
        /// Устанавливает параметры для условия в создаваемом переходе
        /// </summary>
        /// <param name="parameters">Параметры условия</param>
        /// <returns>Обновленный строитель переходов</returns>
        public TransitionBuilder WithCondition(string parameters)
        {
            _parameters = parameters;

            return this;
        }

        /// <summary>
        /// Добавляет команду для создаваемого перехода
        /// </summary>
        /// <param name="commandName">Имя команды</param>
        /// <param name="parameters">Опциональный список параметров команды</param>
        /// <returns>Обновленный строитель переходов</returns>
        public TransitionBuilder AddCommand(string commandName, List<Tuple<string, string>> parameters = null)
        {
            _commandsData.AddCommandStorage(commandName, parameters);

            return this;
        }

        /// <summary>
        /// Добавляет идентификатор отладки для создаваемого перехода
        /// </summary>
        /// <param name="id">Идентификатор отладки</param>
        /// <returns>Обновленный строитель переходов</returns>
        public TransitionBuilder AddDebugId(string id)
        {
            _id = id;

            return this;
        }

        /// <summary>
        /// Создает новый переход для ИМС
        /// </summary>
        /// <returns>Новый переход</returns>
        public Transition Build()
        {
            if (string.IsNullOrEmpty(_nextStateId))
                throw new Exception($"Next state is not set {_id}");

            if (string.IsNullOrEmpty(_eventId))
                throw new Exception($"Event id is not set {_id}");

            if (_bus == null)
                throw new Exception($"Bus id is not set {_id}");

            return new Transition(_nextStateId, _eventId, _bus, _parameters, _commandsData.CreateCommands(_bus));
        }
    }
}
