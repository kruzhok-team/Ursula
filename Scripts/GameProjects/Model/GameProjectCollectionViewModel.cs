using Core.UI.Constructor;
using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.EmbeddedGames.Model
{
    public partial class GameProjectCollectionViewModel : ConstructorViewModel, IInjectable
    {
        public event EventHandler VisibleView_EventHandler;

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
            var handler = VisibleView_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
