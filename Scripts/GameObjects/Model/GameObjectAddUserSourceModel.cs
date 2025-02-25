using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAddUserSourceModel : IInjectable
    {

        public bool IsGameObjectAddUserSourceVisible { get; private set; } = false;

        public event EventHandler GameGameObjectAddUserSourceVisible_EventHandler;
        public event EventHandler GameObjectAddUserSourceToCollection_EventHandler;



        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectAddUserSourceModel SetAddUserSourceToCollection(EventArgs eventArgs)
        {
            InvokeGameObjectAddUserSourceToCollectionEvent(eventArgs);
            return this;
        }

        public GameObjectAddUserSourceModel SetGameObjectAddUserSourceVisible(bool value)
        {
            IsGameObjectAddUserSourceVisible = value;
            EventArgs eventArgs = new EventArgs();
            InvokeGameObjectAddUserSourceVisibleEvent(eventArgs);
            return this;
        }

        private void InvokeGameObjectAddUserSourceToCollectionEvent(EventArgs eventArgs)
        {
            var handler = GameObjectAddUserSourceToCollection_EventHandler;
            handler?.Invoke(this, eventArgs);
        }

        private void InvokeGameObjectAddUserSourceVisibleEvent(EventArgs eventArgs)
        {
            var handler = GameGameObjectAddUserSourceVisible_EventHandler;
            handler?.Invoke(this, eventArgs);
        }
    }
}
