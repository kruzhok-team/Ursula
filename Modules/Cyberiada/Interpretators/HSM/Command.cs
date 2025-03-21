using System;
using System.Collections.Generic;
using Talent.Logic.Bus;

namespace Talent.Logic.HSM
{
    /// <summary>
    /// Класс, представляющий команду, которая может быть вызвана при помощи шины событий
    /// </summary>
    public class Command
    {
        private readonly ICommandBus _bus;
        private readonly string _commandName;
        private readonly List<Tuple<string, string>> _parameters;

        /// <summary>
        /// Конструктор команды
        /// </summary>
        /// <param name="bus">Шина команд</param>
        /// <param name="commandName">Имя команды</param>
        /// <param name="parameters">Список параметров команды</param>
        public Command(ICommandBus bus, string commandName, List<Tuple<string, string>> parameters = null)
        {
            _bus = bus;
            _commandName = commandName;
            _parameters = parameters;
        }

        /// <summary>
        /// Вызывает команду с определенными параметрами
        /// </summary>
        public void Make()
        {
            _bus.InvokeCommand(_commandName, _parameters);
        }

        public string GetCommandName()
        {
            return _commandName;
        }

        public List<Tuple<string, string>> GetParameters()
        {
            return _parameters;
        }
    }
}