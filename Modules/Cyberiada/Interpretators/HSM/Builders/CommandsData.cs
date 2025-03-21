using System;
using System.Collections.Generic;
using Talent.Logic.Bus;

namespace Talent.Logic.HSM.Builders
{
    /// <summary>
    /// Класс ответственный за создание команд и хранение данных для этих команд
    /// </summary>
    public class CommandsData
    {
        private readonly List<(string commandName, List<Tuple<string, string>> parameters)> _commandsData =
            new List<(string commandName, List<Tuple<string, string>> parameters)>();

        /// <summary>
        /// Добавляет данные для создания команд в хранилище
        /// </summary>
        /// <param name="commandName">Имя команды</param>
        /// <param name="parameters">Список параметров команды</param>
        public void AddCommandStorage(string commandName, List<Tuple<string, string>> parameters)
        {
            _commandsData.Add((commandName, parameters));
        }

        /// <summary>
        /// Создает команды, используя данные, добавленные в хранилище
        /// </summary>
        /// <param name="bus">Шина команд, ассоциированный с создаваемыми командами</param>
        /// <returns>Перечисление команд</returns>
        public IEnumerable<Command> CreateCommands(ICommandBus bus)
        {
            if (_commandsData == null || _commandsData.Count == 0)
            {
                return null;
            }

            Command[] commandStorage = new Command[_commandsData.Count];

            for (int i = 0; i < _commandsData.Count; i++)
            {
                commandStorage[i] = new Command(bus, _commandsData[i].commandName, _commandsData[i].parameters);
            }

            return commandStorage;
        }
    }
}
