using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAddGameObjectAssetView : Node, IInjectable
    {
        [Export] GameObjectAddGameObjectAssetUiView gameObjectAddUserSourceUI;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _AddGameObjectAssetProvider;

        public bool IsGameObjectAddUserSourceVisible { get; private set; } = false;

        public event EventHandler GameGameObjectAddUserSourceVisible_EventHandler;

        private GameObjectLibraryManager _commonLibrary;
        private GameObjectAddGameObjectAssetModel _AddGameObjectAssetModel;

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
            _AddGameObjectAssetModel = await _AddGameObjectAssetProvider.GetAsync();
            _AddGameObjectAssetModel.GameGameObjectAddGameObjectAssetVisible_EventHandler += GameObjectAddGameObjectAssetModel_ShowAddGameObjectAssetVisible_EventHandler;
            _AddGameObjectAssetModel.GameObjectAddAssetToCollection_EventHandler += AddGameObjectAssetModel_GameObjectAddAssetToCollection_EventHandler;
        }

        private void GameObjectAddGameObjectAssetModel_ShowAddGameObjectAssetVisible_EventHandler(object sender, EventArgs e)
        {
            OnShow(_AddGameObjectAssetModel.IsGameObjectAddUserSourceVisible);
        }

        private void OnShow(bool value)
        {
            gameObjectAddUserSourceUI.OnShow(value);
        }

        private async void AddGameObjectAssetModel_GameObjectAddAssetToCollection_EventHandler(object sender, EventArgs e)
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _AddGameObjectAssetModel = await _AddGameObjectAssetProvider.GetAsync();

            _commonLibrary.SetItem(_AddGameObjectAssetModel.modelName, _AddGameObjectAssetModel._gameObjectAssetSourcesTo, GameObjectAssetsUserSource.LibId);
            await _commonLibrary.Save();
        }

    }
}
