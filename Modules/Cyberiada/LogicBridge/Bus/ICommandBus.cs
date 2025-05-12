using System;
using System.Collections.Generic;

namespace Talent.Logic.Bus
{
    /// <summary>
    /// Интерфейс, представляющий шину команд
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Добавляет слушатель любой команды в шину команд
        /// </summary>
        /// <param name="listener">Добавляемый слушатель</param>
        void AddCommandListener(Listener listener);

        /// <summary>
        /// Добавляет слушатель конкретной команды с конкретным именем в шину команд
        /// </summary>
        /// <param name="commandName">Имя команды, которую нужно прослушать.</param>
        /// <param name="listener">Добавляемый слушатель</param>
        void AddCommandListener(string commandName, Listener listener);

        /// <summary>
        /// Удаляет слушатель команд из шины команд
        /// </summary>
        /// <param name="commandName">Имя команды, из которой нужно удалить слушатель.</param>
        /// <param name="listener">Удаляемый слушатель</param>
        void RemoveCommandListener(string commandName, Listener listener);

        /// <summary>
        /// Вызывает команду с определенным именем и опциональным значением
        /// </summary>
        /// <param name="commandName">Имя команды</param>
        /// <param name="parameters">Необязательное значение, передаваемое слушателям команд</param>
        void InvokeCommand(string commandName, List<Tuple<string, string>> parameters = null);
    }
}