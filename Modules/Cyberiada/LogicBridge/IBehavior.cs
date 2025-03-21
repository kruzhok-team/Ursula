namespace Talent.Logic
{
    /// <summary>
    /// Абстрактное поведение
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// Запускает поведение
        /// </summary>
        public void Start();

        /// <summary>
        /// Обновляет состояние поведения
        /// </summary>
        public void Update();

        /// <summary>
        /// Останавливает поведение
        /// </summary>
        public void Stop();
    }
}
