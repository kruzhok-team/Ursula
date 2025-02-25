using Godot;
using System;
using System.Diagnostics.Tracing;
using Ursula.Core.DI;
using Ursula.GameObjects.View;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAddUserSourceUI : Control, IInjectable
    {
        [Export]
        public Button ButtonAddUserSource;

        [Export]
        public Button ButtonClose;

        [Inject]
        private ISingletonProvider<GameObjectAddUserSourceModel> _addUserSourceProvider;

        public event EventHandler ButtonAddUserSourceDown_EventHandler;
        public event EventHandler ButtonCloseDown_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            ButtonAddUserSource.ButtonDown += AddUserSourceButton_DownEventHandler;
            ButtonClose.ButtonDown += ButtonClose_DownEventHandler;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonAddUserSource.ButtonDown -= AddUserSourceButton_DownEventHandler;
            ButtonClose.ButtonDown -= ButtonClose_DownEventHandler;
        }

        public void OnShow(bool value)
        {
            Visible = value;
        }

        async void AddUserSourceButton_DownEventHandler()
        {
            ControlPopupMenu.instance._HideAllMenu();
            var viewModel = _addUserSourceProvider != null ? await _addUserSourceProvider.GetAsync() : null;
            EventArgs eventArgs = new EventArgs();
            viewModel?.SetAddUserSourceToCollection(eventArgs);
        }

        async void ButtonClose_DownEventHandler()
        {
            var viewModel = _addUserSourceProvider != null ? await _addUserSourceProvider.GetAsync() : null;
            EventArgs eventArgs = new EventArgs();
            viewModel?.SetGameObjectAddUserSourceVisible(false);
        }

    }
}
