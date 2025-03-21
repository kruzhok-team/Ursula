using System;
using System.Collections.Generic;

namespace Talent.Logic.Bus
{
    /// <summary>
    /// Класс, представляющий шину команд, событий, переменных
    /// </summary>
    public class LocalBus : IBus
    {
        private const string AnyName = "Any";

        private readonly IDictionary<string, List<Listener>> _commands = new Dictionary<string, List<Listener>>();
        private readonly IDictionary<string, List<Listener>> _events = new Dictionary<string, List<Listener>>();
        private readonly IDictionary<string, IVariableGetter> _variables = new Dictionary<string, IVariableGetter>();

        /// <summary>
        /// Конструктор <see cref="LocalBus"/>
        /// </summary>
        public LocalBus()
        {
            _events.Add(AnyName, new List<Listener>());
            _commands.Add(AnyName, new List<Listener>());
        }

        /// <summary>
        /// Добавляет слушатель любой команды в шину команд
        /// </summary>
        /// <param name="listener">Добавляемый слушатель</param>
        public void AddCommandListener(Listener listener)
        {
            AddCommandListener(AnyName, listener);
        }

        /// <summary>
        /// Добавляет слушатель команды в шину команд
        /// </summary>
        /// <param name="commandName">Имя команды, которую нужно прослушать.</param>
        /// <param name="listener">Добавляемый слушатель</param>
        public void AddCommandListener(string commandName, Listener listener)
        {
            AddListener(_commands, commandName, listener);
        }

        /// <summary>
        /// Добавляет слушатель любого события в шину событий
        /// </summary>
        /// <param name="listener">Добавляемый слушатель</param>
        public void AddEventListener(Listener listener)
        {
            AddEventListener(AnyName, listener);
        }

        /// <summary>
        /// Добавляет слушатель любого события в шину событий
        /// </summary>
        /// <param name="eventName">Имя прослушиваемого события</param>
        /// <param name="listener">Добавляемый слушатель</param>
        public void AddEventListener(string eventName, Listener listener)
        {
            AddListener(_events, eventName, listener);
        }

        /// <summary>
        /// Удаляет слушатель команд из шины команд
        /// </summary>
        /// <param name="commandName">Имя команды, из которой нужно удалить слушатель.</param>
        /// <param name="listener">Удаляемый слушатель</param>
        public void RemoveCommandListener(string commandName, Listener listener)
        {
            RemoveListener(_commands, commandName, listener);
        }
        
        /// <summary>
        /// Удаляет слушатель событий из шины событий
        /// </summary>
        /// <param name="eventName">Имя события, из которой нужно удалить слушатель.</param>
        /// <param name="listener">Удаляемый слушатель</param>
        public void RemoveEventListener(string eventName, Listener listener)
        {
            RemoveListener(_events, eventName, listener);
        }

        /// <summary>
        /// Вызывает событие с определенным именем и опциональным значением
        /// </summary>
        /// <param name="eventName">Имя события</param>
        /// <param name="parameters">Необязательное значение, передаваемое слушателям событий</param>
        public void InvokeEvent(string eventName, List<Tuple<string, string>> parameters = null)
        {
            Invoke(_events, eventName, parameters);
        }

        /// <summary>
        /// Вызывает команду с определенным именем и опциональным значением
        /// </summary>
        /// <param name="commandName">Имя команды</param>
        /// <param name="value">Необязательное значение, передаваемое слушателям команд</param>
        public void InvokeCommand(string commandName, List<Tuple<string, string>> value = null)
        {
            Invoke(_commands, commandName, value);
        }

        /// <summary>
        /// Добавляет функцию получения переменной в шину переменных
        /// </summary>
        /// <typeparam name="T">Тип переменной</typeparam>
        /// <param name="variableName">Имя переменной</param>
        /// <param name="getter">Функция, возвращающая значение переменной</param>
        public void AddVariableGetter<T>(string variableName, Func<T> getter)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new ArgumentException("Variable name cannot be null or whitespace.", nameof(variableName));
            }

            if (_variables.ContainsKey(variableName))
            {
                throw new ArgumentException("Variable getter with same variable name already exists", nameof(variableName));
            }

            _variables[variableName] = new VariableGetter<T>(getter);
        }

        /// <summary>
        /// Пытается получить значение переменной по имени и типу
        /// </summary>
        /// <typeparam name="T">Тип переменной</typeparam>
        /// <param name="variableName">Имя переменной</param>
        /// <param name="value">Если значение найдено, то возвращается значение, иначе null</param>
        /// <returns>true, если переменная существует и ее значение было успешно получено, иначе false</returns>
        public bool TryGetVariableValue<T>(string variableName, out T value)
        {
            if (_variables.TryGetValue(variableName, out IVariableGetter getter))
            {
                if (getter.TryGetTypedVariable(out T variable))
                {
                    value = variable;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Удаляет функцию получения переменной из шины переменных
        /// </summary>
        /// <param name="variableName">Имя переменной</param>
        public void RemoveVariableGetter(string variableName)
        {
            _variables.Remove(variableName);
        }

        /// <summary>
        /// Пытается получить значение переменной по имени
        /// </summary>
        /// <param name="variableName">Имя переменной</param>
        /// <param name="value">Если значение найдено, то возвращается значение, иначе null</param>
        /// <returns>true, если переменная существует и ее значение было успешно получено, иначе false</returns>
        public bool TryGetVariableValue(string variableName, out string value)
        {
            if (_variables.TryGetValue(variableName, out IVariableGetter getter))
            {
                value = getter.GetStringVariable();
                return true;
            }

            value = default;
            return false;
        }

        private void AddListener<T>(IDictionary<string, List<T>> container, string name, T listener)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Listener name cannot be null or whitespace.", nameof(name));
            }

            if (container.ContainsKey(name) == false)
            {
                container.Add(name, new List<T>(1));
            }

            
            container[name].Insert(0, listener);
        }

        private void RemoveListener<T>(IDictionary<string, List<T>> container, string name, T listener)
        {
            if (container.ContainsKey(name) == false)
            {
                return;
            }

            container[name].Remove(listener);
        }

        private void Invoke(IDictionary<string, List<Listener>> container, string name, List<Tuple<string, string>> value)
        {
            if (container.ContainsKey(name) == false)
            {
                return;
            }

            foreach (Listener listener in container[name])
            {
                if (listener.Invoke(value))
                {
                    break;
                }
            }
        }
    }
}
