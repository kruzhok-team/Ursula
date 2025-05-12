namespace Talent.Logic.Bus
{
    /// <summary>
    /// Интерфейс получения переменных
    /// </summary>
    public interface IVariableGetter
    {
        /// <summary>
        /// Пытается получить значение переменной по типу
        /// </summary>
        /// <param name="result">Если найдена переменная с указанным типом, то возвращается значение, иначе null</param>
        /// <typeparam name="T">Тип переменной</typeparam>
        /// <returns>true, если переменная c указанным типом успешно получена, иначе false</returns>
        bool TryGetTypedVariable<T>(out T result);

        /// <summary>
        /// Получает строковое представление переменной
        /// </summary>
        /// <returns>Строковое представление</returns>
        string GetStringVariable();
    }
}
