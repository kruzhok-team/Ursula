using System;
using System.Collections.Generic;

namespace Talent.Logic.Bus
{
    /// <summary>
    /// Интерфейс, представляющий шину событий
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Добавляет слушатель любого события в шину событий
        /// </summary>
        /// <param name="listener">Добавляемый слушатель</param>
        void AddEventListener(Listener listener);

        /// <summary>
        /// Добавляет слушатель конкретного события с конкретным именем в шину событий
        /// </summary>
        /// <param name="eventName">Имя прослушиваемого события</param>
        /// <param name="listener">Добавляемый слушатель</param>
        void AddEventListener(string eventName, Listener listener);

        /// <summary>
        /// Удаляет слушатель событий из шины событий
        /// </summary>
        /// <param name="eventName">Имя события, из которой нужно удалить слушатель.</param>
        /// <param name="listener">Удаляемый слушатель</param>
        void RemoveEventListener(string eventName, Listener listener);

        
        /// <summary>
        /// Вызывает событие с определенным именем и опциональным значением
        /// </summary>
        /// <param name="eventName">Имя события</param>
        /// <param name="parameters">Необязательное значение, передаваемое слушателям событий</param>
        void InvokeEvent(string eventName, List<Tuple<string, string>> parameters = null);
    }
}
