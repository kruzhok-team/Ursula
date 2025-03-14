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

        private GameObjectCurrentInfoModel _gameObjectCurrentInfoModel;
        private GameObjectLibraryManager _gameObjectLibraryManager;
        private GameObjectCollectionModel _gameObjectCollectionModel;

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

            _gameObjectCollectionModel.GameObjectAssetSelectedEvent += GameObjectCollectionModel_GameObjectAssetSelected_EventHandler;
        }

        public void RemoveGraphXml(string path)
        {

        }

        private void GameObjectCollectionModel_GameObjectAssetSelected_EventHandler(object sender, EventArgs e)
        {
            _gameObjectCurrentInfoModel.SetAssetInfoView(_gameObjectCollectionModel.AssetSelected, true);
            GameObjectCurrentInfoView.SetAssetInfoView(_gameObjectCollectionModel.AssetSelected);
        }

        private GameObjectAssetInfo GetGameObjectAssetInfo(string id)
        {
            return _gameObjectLibraryManager.GetItemInfo(id);
        }
    }
}
