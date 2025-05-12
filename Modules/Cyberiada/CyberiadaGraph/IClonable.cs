namespace Talent.Graphs
{
    /// <summary>
    /// Интерфейс, представляющий объекты, которые умеют копироваться 
    /// </summary>
    public interface IClonable<T>
    {
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns>Копия объекта</returns>
        T GetCopy();
    }
}
