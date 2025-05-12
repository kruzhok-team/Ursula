namespace Talent.Logic
{
    public interface ILogicInterpreter<in TSource, in TBus>
    {
        IBehavior Process(TSource source, TBus bus);
    }
}
