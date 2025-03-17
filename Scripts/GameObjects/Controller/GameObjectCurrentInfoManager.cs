using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
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
            _gameObjectCurrentInfoModel.RemoveCurrentInfoAudioEvent += _gameObjectCurrentInfoModel_RemoveCurrentInfoAudioEventHandler;
            _gameObjectCurrentInfoModel.RemoveCurrentInfoAnimationEvent += _gameObjectCurrentInfoModel_RemoveCurrentInfoAnimationEventHandler;

        }

        private void GameObjectCollectionModel_GameObjectAssetSelected_EventHandler(object sender, EventArgs e)
        {
            currentItemId = _gameObjectCollectionModel.AssetSelected.Id;
            GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(currentItemId);

            string destPath = "";

            if (_gameObjectCollectionModel.AssetSelected.ProviderId == GameObjectAssetsUserSource.LibId)
            {
                destPath = ProjectSettings.GlobalizePath(GameObjectAssetsUserSource.CollectionPath + Path.GetFileNameWithoutExtension(assetInfo.Template.Folder) + "/");
                _gameObjectAddGameObjectAssetModel.Provider = GameObjectAssetsUserSource.LibId;
            }
            else if (_gameObjectCollectionModel.AssetSelected.ProviderId == GameObjectAssetsEmbeddedSource.LibId)
            {
                destPath = ProjectSettings.GlobalizePath(GameObjectAssetsEmbeddedSource.CollectionPath + Path.GetFileNameWithoutExtension(assetInfo.Template.Folder) + "/");
                _gameObjectAddGameObjectAssetModel.Provider = GameObjectAssetsEmbeddedSource.LibId;
            }

            if (string.IsNullOrEmpty(destPath)) return;

            _gameObjectAddGameObjectAssetModel.SetModelName(assetInfo.Name);
            _gameObjectAddGameObjectAssetModel.SetDestPath(destPath);
            _gameObjectAddGameObjectAssetModel.SetGraphXmlPath(destPath + assetInfo.Template.GraphXmlPath);
            _gameObjectAddGameObjectAssetModel.SetModelPath(destPath + assetInfo.Template.Sources.Model3dFilePath);
            _gameObjectAddGameObjectAssetModel.SetPreviewImageFilePath(destPath + assetInfo.Template.PreviewImageFilePath);
            _gameObjectAddGameObjectAssetModel.SetGameObjectGroup(assetInfo.Template.GameObjectGroup);
            _gameObjectAddGameObjectAssetModel.SetGameObjectClass(assetInfo.Template.GameObjectClass);
            _gameObjectAddGameObjectAssetModel.SetGameObjectSample(assetInfo.Template.GameObjectSample);

            _gameObjectAddGameObjectAssetModel.ClearAudios();
            List<string> audios = assetInfo.Template.Sources.Audios;
            if (audios != null)
            {
                for (int i = 0; i < audios.Count; i++)
                {
                    string path = $"{destPath}{audios[i]}";
                    _gameObjectAddGameObjectAssetModel.AddSoundResources(path);
                }
            }

            _gameObjectAddGameObjectAssetModel.ClearAnimations();
            List<string> animations = assetInfo.Template.Sources.Animations;
            if (animations != null)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    string path = $"{destPath}{animations[i]}";
                    _gameObjectAddGameObjectAssetModel.AddAnimationResources(path);
                }
            }
        }

        private async void _gameObjectCurrentInfoModel_RemoveCurrentInfoGraphXmlEventHandler(object sender, EventArgs e)
        {
            GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(currentItemId);

            string pathGraphXml = _gameObjectLibraryManager.GetGraphXmlPath(assetInfo.Id);

            _DeleteFile(ProjectSettings.GlobalizePath(pathGraphXml));

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

        private void _DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private async void _gameObjectCurrentInfoModel_RemoveCurrentInfoAudioEventHandler(object sender, EventArgs e)
        {
            GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(currentItemId);

            string audioName = _gameObjectCurrentInfoModel.AudioName;
            string relativePath = MapManager.PATHAUDIO + "/" + audioName;

            List<string> audios = assetInfo.Template.Sources.Audios;
            audios.Remove(relativePath);
            string path = _gameObjectLibraryManager.GetFullPath(currentItemId, relativePath);

            _DeleteFile(ProjectSettings.GlobalizePath(path));

            _gameObjectAddGameObjectAssetModel.RemoveSoundResources(ProjectSettings.GlobalizePath(path));

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

        private async void _gameObjectCurrentInfoModel_RemoveCurrentInfoAnimationEventHandler(object sender, EventArgs e)
        {
            GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(currentItemId);

            string animationName = _gameObjectCurrentInfoModel.AnimationName;
            string relativePath = MapManager.PATHANIMATION + "/" + animationName;

            List<string> animations = assetInfo.Template.Sources.Animations;
            animations.Remove(relativePath);
            string path = _gameObjectLibraryManager.GetFullPath(currentItemId, relativePath);

            _DeleteFile(ProjectSettings.GlobalizePath(path));

            _gameObjectAddGameObjectAssetModel.RemoveAnimationResources(ProjectSettings.GlobalizePath(path));

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
        
    }
}
