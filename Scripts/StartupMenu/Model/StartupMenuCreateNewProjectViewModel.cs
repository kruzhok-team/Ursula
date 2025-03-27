using Core.UI.Constructor;
using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.StartupMenu.Model
{
    public partial class StartupMenuCreateNewProjectViewModel : ConstructorViewModel, IInjectable
    {
        public bool isVisibleView { get; private set; }

        public event EventHandler StartupMenuCreateNewProjectViewVisible_EventHandler;
        public event EventHandler StartCreatingProject_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {

        }

        public StartupMenuCreateNewProjectViewModel SetVisibleCreateNewProjectView(bool value)
        {
            isVisibleView = value;
            InvokeVisibleCreateNewProjectView();
            return this;
        }

        public StartupMenuCreateNewProjectViewModel StartCreatingProject()
        {
            InvokeStartCreatingProjectEvent();
            return this;
        }

        private void InvokeVisibleCreateNewProjectView()
        {
            var handler = StartupMenuCreateNewProjectViewVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeStartCreatingProjectEvent()
        {
            var handler = StartCreatingProject_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
