using Core.UI.Constructor;
using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.GameProjects.Model
{
    public partial class GameProjectCollectionViewModel : ConstructorViewModel, IInjectable
    {
        public event EventHandler ViewVisible_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {

        }

        public GameProjectCollectionViewModel SetVisibleView(bool value)
        {
            Visible = value;
            InvokeViewVisibleEvent();
            return this;
        }

        private void InvokeViewVisibleEvent()
        {
            var handler = ViewVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
