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
        PackedScene ItemLibraryObjectPrefab;

        [Export]
        TextEdit TextEditModelName;

        [Export]
        TextEdit TextEditPath3DModel;

        [Export]
        OptionButton OptionButtonGroupObject;

        [Export]
        TextEdit TextEditSampleObject;

        [Export]
        VBoxContainer VBoxContainerGraphXmlPath;

        [Export]
        TextureRect TextureRectPreviewImage;

        [Export]
        Button ButtonOpenGraphXmlPath;

        [Export]
        Button ButtonEditGraphXmlPath;

        public event EventHandler OpenGraphXmlEvent;
        public event EventHandler GameObjectEditAssetEvent;


        [Inject]
        private ISingletonProvider<GameObjectCurrentInfoModel> _gameObjectCurrentInfoModelProvider;

        private FileDialogTool dialogTool;

        private GameObjectCurrentInfoModel _gameObjectCurrentInfoModel;

        private string itemId;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

            ButtonOpenGraphXmlPath.ButtonDown += ButtonOpenGraphXmlPath_DownEventHandler;
            ButtonEditGraphXmlPath.ButtonDown += ButtonEditGraphXmlPath_DownEventHandler;

            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            for (int i = 0; i < gameObjectGroups.Length; i++)
            {
                OptionButtonGroupObject.AddItem(gameObjectGroups[i]);
            }

            VoxLib.RemoveAllChildren(VBoxContainerGraphXmlPath);

            _ = SubscribeEvent();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonOpenGraphXmlPath.ButtonDown -= ButtonOpenGraphXmlPath_DownEventHandler;
            ButtonEditGraphXmlPath.ButtonDown -= ButtonEditGraphXmlPath_DownEventHandler;

            _gameObjectCurrentInfoModel.VisibleCurrentAssetInfoEvent -= VisibleCurrentInfoView_ShowEventHandler;
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectCurrentInfoModel = await _gameObjectCurrentInfoModelProvider.GetAsync();

            _gameObjectCurrentInfoModel.VisibleCurrentAssetInfoEvent += VisibleCurrentInfoView_ShowEventHandler;
        }

        public async void SetAssetInfoView(GameObjectAssetInfo currentAssetInfo)
        {
            itemId = currentAssetInfo.Id;
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

            // GraphXml
            VoxLib.RemoveAllChildren(VBoxContainerGraphXmlPath);
            if (!string.IsNullOrEmpty(currentAssetInfo.Template.GraphXmlPath))
            {
                Node instance = ItemLibraryObjectPrefab.Instantiate();
                ItemLibraryObjectData item = instance as ItemLibraryObjectData;
                if (item != null)
                {
                    item.removeEvent += GraphXml_RemoveEventHandler;
                    item.Invalidate(Path.GetFileName(currentAssetInfo.Template.GraphXmlPath));
                    VBoxContainerGraphXmlPath.AddChild(instance);
                }
            }

            OptionButtonGroupObject.Text = currentAssetInfo.Template.GameObjectGroup;

            TextEditSampleObject.Text = currentAssetInfo.Template.GameObjectSample;

            TextureRectPreviewImage.Texture = await currentAssetInfo.GetPreviewImage();
        }

        private void VisibleCurrentInfoView_ShowEventHandler(object sender, EventArgs e)
        {
            this.Visible = _gameObjectCurrentInfoModel.isVisibleAssetInfo;
        }

        private void ButtonOpenGraphXmlPath_DownEventHandler()
        {

        }

        private void ButtonEditGraphXmlPath_DownEventHandler()
        {

        }

        private void GraphXml_RemoveEventHandler(string graphXmlName)
        {
            _gameObjectCurrentInfoModel.RemoveGraphXml();
        }

        
    }
}
