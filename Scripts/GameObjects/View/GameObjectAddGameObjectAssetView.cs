using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAddGameObjectAssetView : Node, IInjectable
    {
        [Export] GameObjectAddGameObjectAssetUI gameObjectAddUserSourceUI;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _addUserSourceProvider;

        public bool IsGameObjectAddUserSourceVisible { get; private set; } = false;

        public event EventHandler GameGameObjectAddUserSourceVisible_EventHandler;

        private GameObjectLibraryManager _commonLibrary;
        private GameObjectAddGameObjectAssetModel _addGameObjectAssetModel;

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
            _addGameObjectAssetModel = await _addUserSourceProvider.GetAsync();
            _addGameObjectAssetModel.GameGameObjectAddUserSourceVisible_EventHandler += GameObjectAddUserSourceModel_ShowAddUserSourceEventHandler;
            _addGameObjectAssetModel.GameObjectAddUserSourceToCollection_EventHandler += AddGameObjectAssetModel_GameObjectAddUserSourceToCollection_EventHandler;
        }

        private void GameObjectAddUserSourceModel_ShowAddUserSourceEventHandler(object sender, EventArgs e)
        {
            OnShow(_addGameObjectAssetModel.IsGameObjectAddUserSourceVisible);
        }

        private void OnShow(bool value)
        {
            gameObjectAddUserSourceUI.OnShow(value);
        }

        private async void AddGameObjectAssetModel_GameObjectAddUserSourceToCollection_EventHandler(object sender, EventArgs e)
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _addGameObjectAssetModel = await _addUserSourceProvider.GetAsync();

            _commonLibrary.SetItem(_addGameObjectAssetModel.modelName, _addGameObjectAssetModel._gameObjectAssetSourcesTo);
            await _commonLibrary.Save();
        }

    }
}
