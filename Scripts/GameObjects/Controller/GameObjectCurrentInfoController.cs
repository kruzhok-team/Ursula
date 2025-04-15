using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

namespace Ursula.GameObjects.Controller
{
    public partial class GameObjectCurrentInfoController : Node, IInjectable
    {
        [Export]
        GameObjectCurrentInfoView GameObjectCurrentInfoView;

        [Inject]
        private ISingletonProvider<GameObjectCurrentInfoModel> _gameObjectCurrentInfoModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _AddGameObjectAssetProvider;

        private GameObjectCurrentInfoModel _gameObjectCurrentInfoModel;
        private GameObjectLibraryManager _gameObjectLibraryManager;
        private GameObjectCollectionModel _gameObjectCollectionModel;
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
            _gameObjectCurrentInfoModel = await _gameObjectCurrentInfoModelProvider.GetAsync();
            _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _addGameObjectAssetModel = await _AddGameObjectAssetProvider.GetAsync();

            _gameObjectCollectionModel.GameObjectAssetSelectedEvent += GameObjectCollectionModel_GameObjectAssetSelected_EventHandler;           
            _addGameObjectAssetModel.GameObjectAddAssetToCollection_EventHandler += AddGameObjectAssetModel_GameObjectAddAssetToCollection_EventHandler;
        }

        public void RemoveGraphXml(string path)
        {

        }

        private void GameObjectCollectionModel_GameObjectAssetSelected_EventHandler(object sender, EventArgs e)
        {
            _addGameObjectAssetModel.ProviderId = _gameObjectCollectionModel.AssetSelected.ProviderId;
            _gameObjectCurrentInfoModel.SetAssetInfoView(_gameObjectCollectionModel.AssetSelected, true);
            GameObjectCurrentInfoView.SetAssetInfoView(_gameObjectCollectionModel.AssetSelected);
        }


        private void AddGameObjectAssetModel_GameObjectAddAssetToCollection_EventHandler(object sender, EventArgs e)
        {
            if (_gameObjectCurrentInfoModel.currentAssetInfo != null && _gameObjectCurrentInfoModel.currentAssetInfo.Id == _gameObjectCollectionModel.AssetSelected.Id)
            {
                GameObjectCurrentInfoView.SetAssetInfoView(_gameObjectCollectionModel.AssetSelected);
            }
        }
    }
}
