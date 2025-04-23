using Core.UI.Constructor;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.EmbeddedGames.Model;

namespace Ursula.StartupMenu.Model
{
    public partial class StartupMenuCreateNewProjectViewModel : ConstructorViewModel, IInjectable
    {
        public string GameName { get; private set; }

        public event EventHandler ViewVisible_EventHandler;
        public event EventHandler StartCreatingProject_EventHandler;


        private string DestPath { get; set; }


        void IInjectable.OnDependenciesInjected()
        {

        }

        public StartupMenuCreateNewProjectViewModel SetVisibleView(bool value)
        {
            Visible = value;
            InvokeVisibleViewEvent();
            return this;
        }

        public StartupMenuCreateNewProjectViewModel StartCreatingProject(string value)
        {
            GameName = value;
            InvokeStartCreatingProjectEvent();
            return this;
        }

        private void InvokeVisibleViewEvent()
        {
            var handler = ViewVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeStartCreatingProjectEvent()
        {
            var handler = StartCreatingProject_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

    }
}
