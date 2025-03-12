using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using VoxLibExample;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectCurrentInfoView : Control, IInjectable
    {

        [Export]
        TextEdit TextEditModelName;

        [Export]
        TextEdit TextEditPath3DModel;

        [Export]
        OptionButton OptionButtonGroupObject;

        [Export]
        TextEdit TextEditSampleObject;

        [Export]
        TextEdit TextEditGraphXmlPath;

        [Export]
        TextureRect TextureRectPreviewImage;

        [Export]
        Button ButtonOpenGraphXmlPath;

        [Export]
        Button ButtonEditAsset;

        public event EventHandler OpenGraphXmlEvent;
        public event EventHandler GameObjectEditAssetEvent;


        [Inject]
        private ISingletonProvider<GameObjectCurrentInfoModel> _gameObjectCurrentInfoModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddGameObjectAssetModelProvider;

        private FileDialogTool dialogTool;

        private GameObjectCurrentInfoModel _gameObjectCurrentInfoModel;
        private GameObjectLibraryManager _gameObjectLibraryManager;
        private GameObjectCollectionModel _gameObjectCollectionModel;
        private GameObjectAddGameObjectAssetModel _gameObjectAddGameObjectAssetModel;

        GameObjectAssetInfo currentAssetInfo;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

            ButtonOpenGraphXmlPath.ButtonDown += ButtonOpenGraphXmlPath_DownEventHandler;
            ButtonEditAsset.ButtonDown += ButtonEditAsset_DownEventHandler;

            _ = SubscribeEvent();
        }



        private GameObjectAssetInfo GetGameObjectAssetInfo(string id)
        {
            return _gameObjectLibraryManager.GetItemInfo(id);
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectCurrentInfoModel = await _gameObjectCurrentInfoModelProvider.GetAsync();
            _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _gameObjectAddGameObjectAssetModel = await _gameObjectAddGameObjectAssetModelProvider.GetAsync();

            _gameObjectCollectionModel.GameObjectAssetSelectedEvent += GameObjectCollectionModel_GameObjectAssetSelected_EventHandler;
        }

        private void GameObjectCollectionModel_GameObjectAssetSelected_EventHandler(object sender, EventArgs e)
        {
            SetAssetInfoView();
        }

        private async void SetAssetInfoView()
        {
            currentAssetInfo = GetGameObjectAssetInfo(_gameObjectCollectionModel.AssetSelected.Id);

            TextEditModelName.Text = currentAssetInfo.Name;

            if (currentAssetInfo.ProviderId == GameObjectAssetsUserSource.LibId)
                TextEditPath3DModel.Text = Path.GetFileName(currentAssetInfo.Template.Sources.Model3dFilePath);
            else if (currentAssetInfo.ProviderId == GameObjectAssetsEmbeddedSource.LibId)
            {
                int idEmbeddedAsset = -1;
                int.TryParse(currentAssetInfo.Template.Sources.Model3dFilePath, out idEmbeddedAsset);
                if (idEmbeddedAsset >= 0 && idEmbeddedAsset < VoxLib.mapAssets.gameItemsGO.Length)
                    TextEditPath3DModel.Text = Path.GetFileName(VoxLib.mapAssets.gameItemsGO[idEmbeddedAsset].ResourcePath);             
            }

            TextEditGraphXmlPath.Text = Path.GetFileName(currentAssetInfo.Template.GraphXmlPath);

            OptionButtonGroupObject.Clear();
            OptionButtonGroupObject.AddItem(currentAssetInfo.Template.GameObjectGroup);

            TextEditSampleObject.Text = currentAssetInfo.Template.GameObjectSample;

            TextureRectPreviewImage.Texture = await currentAssetInfo.GetPreviewImage();
        }

        private void ButtonEditAsset_DownEventHandler()
        {
            _gameObjectAddGameObjectAssetModel.SetEditAsset(currentAssetInfo);
        }

        private void ButtonOpenGraphXmlPath_DownEventHandler()
        {

        }

    }
}
