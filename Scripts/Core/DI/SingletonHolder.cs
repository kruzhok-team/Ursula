using Fractural.Tasks;
using Godot;
using System;
using System.Threading;

namespace Ursula.Core.DI
{
    public class SingletonHolder<T> : ISingletonProvider<T> where T : class
    {
        public event Action<T> ValueUpdatedEvent;
        public event Action<T> ValueInitializedEvent;
        public event Action ValueClearedEvent;

        private T _value;

        public SingletonHolder(T value)
        {
            _value = value;
        }

        public bool TryGet(out T value)
        {
            value = _value;
            return value != null;
        }

        public async GDTask<T> GetAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                while (_value == null && cancellationToken.IsCancellationRequested)
                {
                    await GDTask.Yield();
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Exception catched while {GetType().Name}.GetAsync():\r\n{e.Message}");
            }
            return _value;
        }

        public void Set(T value)
        {
            if (value == _value)
                return;

            var valueBefore = _value;
            _value = value;

            if (_value == null)
                InvokeValueClearedEvent();
            else if (valueBefore == null)
                InvokeValueInitializedEvent();
            InvokeValueUpdatedEvent();
        }

        private void InvokeValueInitializedEvent()
        {
            var handler = ValueInitializedEvent;
            handler?.Invoke(_value);
        }

        private void InvokeValueUpdatedEvent()
        {
            var handler = ValueInitializedEvent;
            handler?.Invoke(_value);
        }

        private void InvokeValueClearedEvent()
        {
            var handler = ValueClearedEvent;
            handler?.Invoke();
        }
    }
}
