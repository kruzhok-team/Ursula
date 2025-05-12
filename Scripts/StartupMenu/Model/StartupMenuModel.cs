using Core.UI.Constructor;
using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.StartupMenu.Model
{
    public partial class StartupMenuModel : ConstructorViewModel, IInjectable
    {
        public event EventHandler StartupMenuVisible_EventHandler;
        public event EventHandler StartupMenuMouseFilterEvent_EventHandler;

        public event EventHandler ButtonCreateGame_EventHandler;
        public event EventHandler ButtonLoadGame_EventHandler;
        public event EventHandler LoadLibrary_Event;


        void IInjectable.OnDependenciesInjected()
        {

        }

        public StartupMenuModel SetVisibleView(bool value)
        {
            Visible = value;
            InvokeMenuVisibleEvent();
            return this;
        }

        public StartupMenuModel SetStartupMenuMouseFilter(bool value)
        {
            InvokeMenuMouseFilterEvent();
            return this;
        }

        public StartupMenuModel SetCreateGame()
        {
            InvokeButtonCreateGameEvent();
            return this;
        }

        public StartupMenuModel SetLoadGame()
        {
            InvokeButtonLoadGameEvent();
            return this;
        }

        public StartupMenuModel SetLoadLibrary()
        {
            InvokeLoadLibraryEvent();
            return this;
        }

        private void InvokeMenuVisibleEvent()
        {
            var handler = StartupMenuVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeMenuMouseFilterEvent()
        {
            var handler = StartupMenuMouseFilterEvent_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeButtonCreateGameEvent()
        {
            var handler = ButtonCreateGame_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeButtonLoadGameEvent()
        {
            var handler = ButtonLoadGame_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeLoadLibraryEvent()
        {
            var handler = LoadLibrary_Event;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
