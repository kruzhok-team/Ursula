using System.Collections.Generic;

namespace Talent.Graphs
{
    public interface IExecutionContextSource
    {
        IEnumerable<ActionData> GetActions();
        IEnumerable<string> GetEvents();
        IEnumerable<string> GetVariables();
    }

    /// <summary>
    /// Класс, представляющий данные поведения
    /// </summary>
    public class ActionData
    {
        /// <summary>
        /// Идентификатор поведения
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Список параметров поведения
        /// </summary>
        public List<ActionParameter> Parameters { get; }

        /// <summary>
        /// Конструктор данных поведения
        /// </summary>
        /// <param name="id">Идентификатор поведения</param>
        /// <param name="parameters">Список параметров поведения</param>
        public ActionData(string id, List<ActionParameter> parameters = null)
        {
            ID = id;
            Parameters = parameters;
        }
    }

    /// <summary>
    /// Класс, представляющий параметр поведения
    /// </summary>
    public class ActionParameter
    {
        /// <summary>
        /// Имя параметра
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Тип параметра
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Принимаемые значения параметром
        /// /// </summary>
        public string[] Values { get; }

        /// <summary>
        /// Конструктор параметра поведения
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <param name="type">Тип параметра</param>
        /// <param name="values">Список принимаемых значений параметром</param>
        public ActionParameter(string name, string type, params string[] values)
        {
            Name = name;
            Type = type;
            Values = values;
        }
    }
}