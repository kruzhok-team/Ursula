using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

namespace Ursula.GameObjects.Controller
{
    public partial class GameObjectCurrentInfoManager : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<GameObjectCurrentInfoModel> _gameObjectCurrentInfoModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddGameObjectAssetModelProvider;
        

        private GameObjectCurrentInfoModel _gameObjectCurrentInfoModel;
        private GameObjectLibraryManager _gameObjectLibraryManager;
        private GameObjectCollectionModel _gameObjectCollectionModel;
        private GameObjectAddGameObjectAssetModel _gameObjectAddGameObjectAssetModel;

        private string currentItemId;

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
            _gameObjectAddGameObjectAssetModel = await _gameObjectAddGameObjectAssetModelProvider.GetAsync();

            _gameObjectCollectionModel.GameObjectAssetSelectedEvent += GameObjectCollectionModel_GameObjectAssetSelected_EventHandler;

            _gameObjectCurrentInfoModel.RemoveCurrentInfoGraphXmlEvent += _gameObjectCurrentInfoModel_RemoveCurrentInfoGraphXmlEventHandler;
        }

        private void GameObjectCollectionModel_GameObjectAssetSelected_EventHandler(object sender, EventArgs e)
        {
            currentItemId = _gameObjectCollectionModel.AssetSelected.Id;
            GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(currentItemId);

            //_gameObjectAddGameObjectAssetModel.SetEditAsset(_gameObjectCollectionModel.AssetSelected);

            string destPath = ProjectSettings.GlobalizePath(GameObjectAssetsUserSource.CollectionPath + Path.GetFileNameWithoutExtension(assetInfo.Template.Folder) + "/");
            
            _gameObjectAddGameObjectAssetModel.SetModelName(assetInfo.Name);
            _gameObjectAddGameObjectAssetModel.SetDestPath(destPath);
            _gameObjectAddGameObjectAssetModel.SetGraphXmlPath(destPath + assetInfo.Template.GraphXmlPath);
            _gameObjectAddGameObjectAssetModel.SetModelPath(destPath + assetInfo.Template.Sources.Model3dFilePath);
            _gameObjectAddGameObjectAssetModel.SetPreviewImageFilePath(destPath + assetInfo.Template.PreviewImageFilePath);

        }

        private async void _gameObjectCurrentInfoModel_RemoveCurrentInfoGraphXmlEventHandler(object sender, EventArgs e)
        {
            GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(currentItemId);

            string pathGraphXml = _gameObjectLibraryManager.GetGraphXmlPath(assetInfo.Id);

            DeleteFile(ProjectSettings.GlobalizePath(pathGraphXml));

            _gameObjectAddGameObjectAssetModel.SetGraphXmlPath("");
            _gameObjectLibraryManager.RemoveItem(currentItemId);

            _gameObjectAddGameObjectAssetModel.SetAddGameObjectAssetToCollection
                (
                    assetInfo.Name,
                    assetInfo.Template.GameObjectGroup,
                    assetInfo.Template.GameObjectClass,
                    assetInfo.Template.GameObjectSample
                );

            await _gameObjectLibraryManager.Save();
        }

        private void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
