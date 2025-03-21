namespace Talent.Logic.Bus
{
    /// <summary>
    /// Обобщённый интерфейс шин событий, переменных, команд
    /// </summary>
    public interface IBus : IEventBus, IVariableBus, ICommandBus
    {
    }
}
