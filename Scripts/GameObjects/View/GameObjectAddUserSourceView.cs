using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAddUserSourceView : Node, IInjectable
    {
        [Export] GameObjectAddUserSourceUI gameObjectAddUserSourceUI;

        [Inject]
        private ISingletonProvider<GameObjectAddUserSourceModel> _addUserSourceProvider;

        public bool IsGameObjectAddUserSourceVisible { get; private set; } = false;

        public event EventHandler GameGameObjectAddUserSourceVisible_EventHandler;

        private GameObjectAddUserSourceModel _AddUserSourceModel;

        void IInjectable.OnDependenciesInjected()
        {
            
        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _AddUserSourceModel = await _addUserSourceProvider.GetAsync();
            _AddUserSourceModel.GameGameObjectAddUserSourceVisible_EventHandler += GameObjectAddUserSourceModel_ShowAddUserSourceEventHandler;
        }
        private void GameObjectAddUserSourceModel_ShowAddUserSourceEventHandler(object sender, EventArgs e)
        {
            OnShow(_AddUserSourceModel.IsGameObjectAddUserSourceVisible);
        }

        private void OnShow(bool value)
        {
            gameObjectAddUserSourceUI.OnShow(value);
        }
    }
}
