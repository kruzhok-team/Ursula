using Godot;
using System;
using Ursula.Core.DI;


namespace Ursula.EmbeddedGames.Model
{
	public partial class ControlEmbeddedGamesProjectViewModel : Control, IInjectable
    {
        public event EventHandler ViewVisible_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public ControlEmbeddedGamesProjectViewModel SetVisibleView(bool value)
        {
            Visible = value;
            InvokeMenuVisibleEvent();
            return this;
        }

        private void InvokeMenuVisibleEvent()
        {
            var handler = ViewVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
