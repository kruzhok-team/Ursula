using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Controller;
using Ursula.GameObjects.Model;
using VoxLibExample;


namespace Ursula.GameObjects.View
{
    public partial class GameObjectCurrentInfoView : Control, IInjectable
    {
        [Export]
        Panel PanelContainerInfo;

        [Export]
        PackedScene ItemLibraryObjectPrefab;

        [Export]
        TextEdit TextEditModelName;

        [Export]
        TextEdit TextEditPath3DModel;

        [Export]
        Button ButtonOpenPath3DModel;

        [Export]
        OptionButton OptionButtonGroupObject;

        [Export]
        TextEdit TextEditSampleObject;

        [Export]
        Button ButtonCopySampleObject;

        [Export]
        VBoxContainer VBoxContainerGraphXmlPath;

        [Export]
        TextureRect TextureRectPreviewImage;

        [Export]
        Button ButtonOpenPreviewImage;

        [Export]
        Button ButtonOpenGraphXmlPath;

        [Export]
        Button ButtonEditGraphXmlPath;

        [Export]
        Button ButtonOpenAudioPath;

        [Export]
        VBoxContainer VBoxContainerAudios;

        [Export]
        Button ButtonOpenAnimationPath;

        [Export]
        VBoxContainer VBoxContainerAnimation;

        [Export]
        Button ButtonDeleteAsset;

        [Export]
        Button ButtonShowInfo;

        [Export]
        TextureRect TextureRectShowInfoTrue;

        [Export]
        TextureRect TextureRectShowInfoFalse;

        public event EventHandler OpenGraphXmlEvent;
        public event EventHandler GameObjectEditAssetEvent;


        [Inject]
        private ISingletonProvider<GameObjectCurrentInfoModel> _gameObjectCurrentInfoModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddGameObjectAssetModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<GameObjectCurrentInfoManager> _gameObjectCurrentInfoManagerProvider;

        private GameObjectCurrentInfoModel _gameObjectCurrentInfoModel;
        private GameObjectAddGameObjectAssetModel _gameObjectAddGameObjectAssetModel;
        private GameObjectCollectionModel _gameObjectCollectionModel;
        private GameObjectLibraryManager _gameObjectLibraryManager;
        private GameObjectCurrentInfoManager _gameObjectCurrentInfoManager;

        private FileDialogTool dialogTool;

        private string itemId;
        private bool showInfo = true;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

            TextEditModelName.MouseExited += TextEditModelName_TextChangedEventHandler;

            ButtonOpenPreviewImage.ButtonDown += ButtonOpenPreviewImage_DownEventHandler;

            OptionButtonGroupObject.ItemSelected += OptionButtonGroupObject_ItemSelectedEventHandler;
            TextEditSampleObject.MouseExited += TextEditSampleObject_TextChangedEventHandler;

            ButtonOpenGraphXmlPath.ButtonDown += ButtonOpenGraphXmlPath_DownEventHandler;
            ButtonEditGraphXmlPath.ButtonDown += ButtonEditGraphXmlPath_DownEventHandler;
            ButtonDeleteAsset.ButtonDown += ButtonDeleteAsset_DownEventHandler;
            ButtonOpenPath3DModel.ButtonDown += ButtonOpenPath3DModel_DownEventHandler;
            ButtonOpenAudioPath.ButtonDown += ButtonOpenAudioPath_DownEventHandler;
            ButtonOpenAnimationPath.ButtonDown += ButtonOpenAnimationPath_DownEventHandler;

            ButtonCopySampleObject.ButtonDown += ButtonCopySampleObject_DownEventHandler;

            ButtonShowInfo.ButtonDown += ButtonShowInfo_ButtonDownEventHandler;

            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            for (int i = 0; i < gameObjectGroups.Length; i++)
            {
                OptionButtonGroupObject.AddItem(gameObjectGroups[i]);
            }

            VoxLib.RemoveAllChildren(VBoxContainerGraphXmlPath);
            VoxLib.RemoveAllChildren(VBoxContainerAudios);
            VoxLib.RemoveAllChildren(VBoxContainerAnimation);

            ContainerInfoVisible();

            _ = SubscribeEvent();
        }

        public override void _Process(double delta)
        {
            //if (Input.MouseMode != Input.MouseModeEnum.Captured)
            //{
            //    if (TextEditModelName.HasFocus())
            //    {

            //    }
            //}
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
            _gameObjectAddGameObjectAssetModel = await _gameObjectAddGameObjectAssetModelProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
            _gameObjectCurrentInfoManager = await _gameObjectCurrentInfoManagerProvider.GetAsync();

            _gameObjectCurrentInfoModel.VisibleCurrentAssetInfoEvent += VisibleCurrentInfoView_ShowEventHandler;
        }

        public async void SetAssetInfoView(GameObjectAssetInfo currentAssetInfo)
        {
            itemId = currentAssetInfo.Id;
            TextEditModelName.Text = currentAssetInfo.Name;
            bool isUserSource = currentAssetInfo.ProviderId == GameObjectAssetsUserSource.LibId;

            if (isUserSource)
            {
                TextEditPath3DModel.Text = Path.GetFileName(currentAssetInfo.Template.Sources.Model3dFilePath);
            }
            else
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

            // Audio
            VoxLib.RemoveAllChildren(VBoxContainerAudios);
            if (currentAssetInfo.Template.Sources.Audios != null)
            {
                List<string> audios = currentAssetInfo.Template.Sources.Audios;
                for (int i = 0; i < audios.Count; i++)
                {
                    Node instance = ItemLibraryObjectPrefab.Instantiate();
                    ItemLibraryObjectData item = instance as ItemLibraryObjectData;
                    if (item != null)
                    {
                        item.removeEvent += Audio_RemoveEventHandler;
                        item.Invalidate(Path.GetFileName(audios[i]));
                        VBoxContainerAudios.AddChild(instance);
                    }
                }
            }

            // Animation
            VoxLib.RemoveAllChildren(VBoxContainerAnimation);
            if (currentAssetInfo.Template.Sources.Animations != null)
            {
                List<string> animations = currentAssetInfo.Template.Sources.Animations;
                for (int i = 0; i < animations.Count; i++)
                {
                    Node instance = ItemLibraryObjectPrefab.Instantiate();
                    ItemLibraryObjectData item = instance as ItemLibraryObjectData;
                    if (item != null)
                    {
                        item.showRemoveButton = false;
                        item.removeEvent += Animations_RemoveEventHandler;
                        item.Invalidate(Path.GetFileName(animations[i]));
                        VBoxContainerAnimation.AddChild(instance);
                    }
                }
            }

            OptionButtonGroupObject.Text = currentAssetInfo.Template.GameObjectGroup;
            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            for (int i = 0; i < gameObjectGroups.Length; i++)
            {
                if (currentAssetInfo.Template.GameObjectGroup == gameObjectGroups[i]) OptionButtonGroupObject.Select(i);
            }

            TextEditSampleObject.Text = currentAssetInfo.Template.GameObjectSample;

            TextureRectPreviewImage.Texture = await currentAssetInfo.GetPreviewImage();

            ChangeAccessElements(isUserSource);
        }

        private void RepaintSelectedAsset()
        {
            GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(_gameObjectCollectionModel.AssetSelected.Id);
            _gameObjectCollectionModel.SetGameObjectAssetSelected(assetInfo);           
        }

        private void TextEditModelName_TextChangedEventHandler()
        {
            string newName = TextEditModelName.Text;
            string itemId = $"{_gameObjectCollectionModel.AssetSelected.ProviderId}.{newName}";

            _gameObjectLibraryManager.RemoveItem(_gameObjectCollectionModel.AssetSelected.Id);
            _gameObjectAddGameObjectAssetModel.SetModelName(newName);
            _gameObjectAddGameObjectAssetModel.SetCurrentAssetToCollection();

            GameObjectAssetInfo asset = _gameObjectLibraryManager.GetItemInfo(itemId);
            _gameObjectCollectionModel.SetGameObjectAssetSelected(asset);

            TextEditModelName.ReleaseFocus();

            RepaintSelectedAsset();
        }

        private void ButtonOpenPreviewImage_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.jpg ; Файл jpg", "*.png ; Файл png" }, async (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    DeleteFile(_gameObjectCollectionModel.AssetSelected.Id, _gameObjectCollectionModel.AssetSelected.Template.PreviewImageFilePath);

                    _gameObjectAddGameObjectAssetModel.SetPreviewImageFilePath(path);
                    _gameObjectAddGameObjectAssetModel.SetCurrentAssetToCollection();

                    RepaintSelectedAsset();
                }
                else
                    GD.PrintErr($"Ошибка  открытия иконки {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }

        private void DeleteFile(string itemId, string relativePath)
        {
            string fullPath = _gameObjectLibraryManager.GetFullPath(itemId, relativePath);
            fullPath = ProjectSettings.GlobalizePath(fullPath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        private void OptionButtonGroupObject_ItemSelectedEventHandler(long index)
        {
            _gameObjectAddGameObjectAssetModel.SetGameObjectGroup(OptionButtonGroupObject.Text);
            _gameObjectAddGameObjectAssetModel.SetCurrentAssetToCollection();

            RepaintSelectedAsset();
        }

        private void TextEditSampleObject_TextChangedEventHandler()
        {
            _gameObjectAddGameObjectAssetModel.SetGameObjectSample(TextEditSampleObject.Text);
            _gameObjectAddGameObjectAssetModel.SetCurrentAssetToCollection();

            TextEditSampleObject.ReleaseFocus();

            RepaintSelectedAsset();
        }

        private void ButtonDeleteAsset_DownEventHandler()
        {
            _gameObjectLibraryManager.RemoveItem(_gameObjectCollectionModel.AssetSelected.Id);
            _= _gameObjectLibraryManager.Save();
        }

        private void ChangeAccessElements(bool isEditable)
        {
            TextEditModelName.Editable = isEditable;
            ButtonDeleteAsset.Disabled = !isEditable;
            ButtonOpenPreviewImage.Disabled = !isEditable;
            ButtonOpenPath3DModel.Disabled = !isEditable;

        }

        private void VisibleCurrentInfoView_ShowEventHandler(object sender, EventArgs e)
        {
            this.Visible = _gameObjectCurrentInfoModel.isVisibleAssetInfo;
        }

        private void ButtonOpenPath3DModel_DownEventHandler()
        {
            ModelLoader.OpenObj(async (path, mesh) =>
            {
                if (mesh != null)
                {
                    DeleteFile(_gameObjectCollectionModel.AssetSelected.Id, _gameObjectCollectionModel.AssetSelected.Template.Sources.Model3dFilePath);

                    _gameObjectAddGameObjectAssetModel?.SetModelPath(path);
                    _gameObjectAddGameObjectAssetModel?.SetCurrentAssetToCollection();

                    RepaintSelectedAsset();
                }
                else
                {
                    GD.PrintErr($"Ошибка модели {path}");
                }
            });
        }

        private void ButtonOpenGraphXmlPath_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.graphml ; Граф graphml" }, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    DeleteFile(_gameObjectCollectionModel.AssetSelected.Id, _gameObjectCollectionModel.AssetSelected.Template.GraphXmlPath);

                    _gameObjectAddGameObjectAssetModel?.SetGraphXmlPath(path);
                    _gameObjectAddGameObjectAssetModel?.SetCurrentAssetToCollection();

                    RepaintSelectedAsset();
                }
                else
                    GD.PrintErr($"Ошибка  открытия графа {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }

        private void ButtonOpenAudioPath_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.mp3 ; mp3", "*.ogg ; ogg", "*.aac ; aac" }, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    _gameObjectAddGameObjectAssetModel?.AddSoundResources(path);
                    _gameObjectAddGameObjectAssetModel?.SetCurrentAssetToCollection();

                    RepaintSelectedAsset();
                }
                else
                    GD.PrintErr($"Ошибка открытия звука {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }

        private void ButtonOpenAnimationPath_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.glb ; Анимация glb" }, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    _gameObjectAddGameObjectAssetModel?.AddAnimationResources(path);
                    _gameObjectAddGameObjectAssetModel?.SetCurrentAssetToCollection();

                    RepaintSelectedAsset();
                }
                else
                    GD.PrintErr($"Ошибка  открытия анимации {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }

        private void ButtonEditGraphXmlPath_DownEventHandler()
        {

        }

        private void ButtonCopySampleObject_DownEventHandler()
        {
            DisplayServer.ClipboardSet(TextEditSampleObject.Text);
        }

        private void GraphXml_RemoveEventHandler(string graphXmlName)
        {
            _gameObjectCurrentInfoModel.RemoveGraphXml();
            RepaintSelectedAsset();
        }

        private void Audio_RemoveEventHandler(string audioName)
        {
            _gameObjectCurrentInfoModel.RemoveAudio(audioName);
        }

        private void Animations_RemoveEventHandler(string animationName)
        {
            _gameObjectCurrentInfoModel.RemoveAnimation(animationName);
        }

        private void ButtonShowInfo_ButtonDownEventHandler()
        {
            showInfo = !showInfo;
            ContainerInfoVisible();
        }

        private void ContainerInfoVisible()
        {
            TextureRectShowInfoTrue.Visible = showInfo;
            TextureRectShowInfoFalse.Visible = !showInfo;
            PanelContainerInfo.Visible = showInfo;
        }
    }


}
