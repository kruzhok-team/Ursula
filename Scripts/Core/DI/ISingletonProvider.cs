using Fractural.Tasks;
using System;
using System.Threading;

namespace Ursula.Core.DI
{
    public interface ISingletonProvider<T> where T : class
    {
        event Action ValueClearedEvent;
        event Action<T> ValueInitializedEvent;
        event Action<T> ValueUpdatedEvent;

        GDTask<T> GetAsync(CancellationToken cancellationToken = default);
        bool TryGet(out T value);
    }
}