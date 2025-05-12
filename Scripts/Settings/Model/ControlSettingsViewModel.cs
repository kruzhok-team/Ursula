using Core.UI.Constructor;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.StartupMenu.Model;

namespace Ursula.Settings.Model
{
    public partial class ControlSettingsViewModel : ConstructorViewModel, IInjectable
    {
        public event EventHandler ShowView_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public ControlSettingsViewModel SetShowVisibleView(bool value)
        {
            Visible = value;
            InvokeShowViewVisibleEvent();
            return this;
        }

        private void InvokeShowViewVisibleEvent()
        {
            var handler = ShowView_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
