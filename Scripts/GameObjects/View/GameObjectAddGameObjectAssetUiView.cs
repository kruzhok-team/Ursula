using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.GameProjects.Model;
using VoxLibExample;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAddGameObjectAssetUiView : Control, IInjectable
    {
        [Export]
        Label LabelTittle;

        [Export]
        Button ButtonOpen3DModel;

        [Export]
        TextEdit TextEditModelName;

        [Export]
        TextEdit TextEditPath3DModel;

        [Export]
        OptionButton OptionButtonGroupObject;

        [Export]
        OptionButton OptionButtonClassObject;

        [Export]
        TextEdit TextEditSampleObject;

        [Export]
        Button ButtonOpenPathSound;

        [Export]
        TextEdit TextEditPathSound;

        [Export]
        VBoxContainer VBoxContainerSound;

        [Export]
        PackedScene ItemLibraryObjectPrefab;

        [Export]
        Button ButtonOpenPathAnimation;

        [Export]
        TextEdit TextEditPathAnimation;

        [Export]
        VBoxContainer VBoxContainerAnimation;

        [Export]
        Button ButtonSaveEditModel;

        [Export]
        Button ButtonRemoveModel;

        [Export]
        public Button ButtonAddUserSource;

        [Export]
        public Button ButtonClose;

        [Export]
        TextEdit TextEditGraphXmlPath;

        [Export]
        Button ButtonOpenGraphXmlPath;

        [Export]
        Button ButtonOpenPreviewPath;

        [Export]
        public TextureRect TextureRectPreviewImage;

        public event EventHandler ButtonAddUserSourceDown_EventHandler;
        public event EventHandler ButtonCloseDown_EventHandler;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _addGameObjectAssetProvider;

        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _gameProjectLibraryManagerProvider;

        private GameObjectAddGameObjectAssetModel _addGameObjectAssetModel;
        private GameProjectLibraryManager _gameProjectLibraryManager;

        private FileDialogTool dialogTool;

        private string modelPath;
        private string destPath;
        private string graphXmlPath;
        private string previewImagePath;

        List<string> audiosTo = new List<string>();
        List<string> animationsTo = new List<string>();

        private System.Collections.Generic.Dictionary<string, string> soundResources = new System.Collections.Generic.Dictionary<string, string>();
        //private System.Collections.Generic.Dictionary<string, string> animationResources = new System.Collections.Generic.Dictionary<string, string>();


        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

            ButtonAddUserSource.ButtonDown += AddGameObjectAssetButton_DownEventHandler;
            ButtonClose.ButtonDown += ButtonClose_DownEventHandler;
            ButtonOpen3DModel.ButtonDown += ButtonOpen3DModel_DownEventHandler;
            ButtonOpenPathSound.ButtonDown += ButtonOpenPathSound_DownEventHandler;
            ButtonOpenPathAnimation.ButtonDown += ButtonOpenPathAnimation_DownEventHandler;
            ButtonOpenGraphXmlPath.ButtonDown += ButtonOpenGraphXmlPath_DownEventHandler;
            ButtonOpenPreviewPath.ButtonDown += ButtonOpenIconPath_DownEventHandler;

            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            for (int i = 0; i < gameObjectGroups.Length; i++)
            {
                OptionButtonGroupObject.AddItem(gameObjectGroups[i]);
            }

            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _addGameObjectAssetModel = await _addGameObjectAssetProvider.GetAsync();
            _gameProjectLibraryManager = await _gameProjectLibraryManagerProvider.GetAsync();
            _addGameObjectAssetModel.GameObjectAddAssetToCollection_EventHandler += AddGameObjectAssetModel_GameObjectAddAssetToCollection_EventHandler;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonAddUserSource.ButtonDown -= AddGameObjectAssetButton_DownEventHandler;
            ButtonClose.ButtonDown -= ButtonClose_DownEventHandler;
            ButtonOpen3DModel.ButtonDown -= ButtonOpen3DModel_DownEventHandler;
            ButtonOpenPathSound.ButtonDown -= ButtonOpenPathSound_DownEventHandler;
            ButtonOpenPathAnimation.ButtonDown -= ButtonOpenPathAnimation_DownEventHandler;
            ButtonOpenGraphXmlPath.ButtonDown -= ButtonOpenGraphXmlPath_DownEventHandler;
        }

        public void OnShow(bool value)
        {
            Visible = value;

            bool isEditMode = _addGameObjectAssetModel.IsEditMode;

            if (!isEditMode)
            {
                ClearData();
                LabelTittle.Text = "Загрузить объект";
            }
            else
            {
                RedrawAll();
            }

            ButtonAddUserSource.Visible = !isEditMode;
            ButtonSaveEditModel.Visible = isEditMode;
            ButtonRemoveModel.Visible = isEditMode;
        }

        private void ClearData()
        {
            //modelCurrent = null;

            modelPath = "";
            destPath = "";
            graphXmlPath = "";

            TextEditModelName.Text = "";
            TextEditPath3DModel.Text = "";
            TextEditPathAnimation.Text = "";
            TextEditPathSound.Text = "";
            TextEditGraphXmlPath.Text = "";

            previewImagePath = "";
            TextureRectPreviewImage.Texture = null;

            VoxLib.RemoveAllChildren(VBoxContainerSound);
            VoxLib.RemoveAllChildren(VBoxContainerAnimation);
        }

        async void AddGameObjectAssetButton_DownEventHandler()
        {
            if (TextEditModelName.Text.Length <= 0)
            {
                VoxLib.ShowMessage("Нет названия.");
                return;
            }

            if (TextEditPath3DModel.Text.Length <= 0)
            {
                VoxLib.ShowMessage("Нет файла модели.");
                return;
            }

            ControlPopupMenu.instance._HideAllMenu();
            var model = _addGameObjectAssetProvider != null ? await _addGameObjectAssetProvider.GetAsync() : null;

            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            string gameObjectGroup = gameObjectGroups[OptionButtonGroupObject.Selected];
            string gameObjectSample = TextEditSampleObject.Text;

            destPath = _gameProjectLibraryManager.currentProjectInfo.GetProjectPath() + "/" + GameObjectAssetsUserSource.CollectionPath + TextEditModelName.Text + "/";
            model.SetDestPath(destPath);
            model.SetGraphXmlPath(graphXmlPath);
            model.Provider = GameObjectAssetsUserSource.LibId;

            model.SetAddGameObjectAssetToCollection(TextEditModelName.Text, gameObjectGroup, OptionButtonClassObject.Selected, gameObjectSample);

            VoxLib.ShowMessage("Объект загружен в библиотеку.");
        }

        async void ButtonClose_DownEventHandler()
        {
            var viewModel = _addGameObjectAssetProvider != null ? await _addGameObjectAssetProvider.GetAsync() : null;
            viewModel?.SetGameObjectAddGameObjectAssetVisible(false);
        }

        void ButtonOpen3DModel_DownEventHandler()
        {
            ModelLoader.OpenObj(async (path, mesh) =>
            {
                if (mesh != null)
                {
                    modelPath = path;
                    TextEditPath3DModel.Text = path;

                    var viewModel = _addGameObjectAssetProvider != null ? await _addGameObjectAssetProvider.GetAsync() : null;
                    viewModel?.SetModelPath(path);

                    FillAnimationListFromGlb(mesh);
                }
                else
                {
                    GD.PrintErr($"Ошибка модели {path} в {destPath}");
                }
            });

        }

        private void FillAnimationListFromGlb(Node glbMeshNode)
        {
            var animationPlayer = GLTFLoader.GetAnimationPlayer(glbMeshNode);

            if (animationPlayer == null) return;

            var list = animationPlayer.GetAnimationList();

            _addGameObjectAssetModel?.ClearAnimations();

            foreach (var animation in list)
            {
                _addGameObjectAssetModel?.AddAnimationResources(animation);
            }
            RedrawAnimationsResourses();

        }

        void ButtonOpenPathSound_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.mp3 ; mp3", "*.ogg ; ogg", "*.aac ; aac" }, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    TextEditPathSound.Text = path;
                    soundResources.Add(Path.GetFileName(path), path);
                    RedrawSoundResourses();
                    _addGameObjectAssetModel?.AddSoundResources(path);
                }
                else
                    GD.PrintErr($"Ошибка открытия звука {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }

        void ButtonOpenPathAnimation_DownEventHandler()
        {
//            dialogTool.Open(new string[] { "*.glb ; Анимация glb" }, (path) =>
//            {
//                if (!string.IsNullOrEmpty(path))
//                {
//                    TextEditPathAnimation.Text = path;
//                    animationResources.Add(Path.GetFileName(path), path);
//                    RedrawAnimationsResourses();
//                    _addGameObjectAssetModel?.AddAnimationResources(path);
//                }
//                else
//                    GD.PrintErr($"Ошибка  открытия анимации {path}");
//            }
//, FileDialog.AccessEnum.Filesystem);
        }

        private void ButtonOpenGraphXmlPath_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.graphml ; Граф graphml" }, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {                   
                    graphXmlPath = path;
                    TextEditGraphXmlPath.Text = path;
                }
                else
                    GD.PrintErr($"Ошибка  открытия графа {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }

        async void ButtonOpenIconPath_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.jpg ; Файл jpg", "*.png ; Файл png" }, async (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    previewImagePath = path;

                    Image img = new Image();
                    var err = img.Load(path);

                    if (err != Error.Ok)
                    {
                        GD.Print("Failed to load image from path: " + path);
                    }
                    else
                    {
                        TextureRectPreviewImage.Texture = ImageTexture.CreateFromImage(img);
                        var viewModel = _addGameObjectAssetProvider != null ? await _addGameObjectAssetProvider.GetAsync() : null;
                        viewModel?.SetPreviewImageFilePath(path);
                    }
                }
                else
                    GD.PrintErr($"Ошибка  открытия иконки {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }

        void RedrawSoundResourses()
        {
            VoxLib.RemoveAllChildren(VBoxContainerSound);

            List<string> _soundResources = new List<string>(soundResources.Keys);

            for (int i = 0; i < _soundResources.Count; i++)
            {
                Node instance = ItemLibraryObjectPrefab.Instantiate();

                ItemLibraryObjectData item = instance as ItemLibraryObjectData;

                if (item == null)
                    continue;

                item.removeEvent += SoundItem_RemoveEventHandler;
                item.Invalidate(_soundResources[i]);

                VBoxContainerSound.AddChild(instance);
            }
        }

        void SoundItem_RemoveEventHandler(string path)
        {
            soundResources.Remove(path);
            RedrawSoundResourses();
            _addGameObjectAssetModel?.RemoveSoundResources(path);
        }

        void RedrawAnimationsResourses()
        {
            VoxLib.RemoveAllChildren(VBoxContainerAnimation);

            //List<string> _animationResources = new List<string>(animationResources.Keys);
            List<string> _animationResources = new List<string>(_addGameObjectAssetModel.GetAnimationResources());

            for (int i = 0; i < _animationResources.Count; i++)
            {
                Node instance = ItemLibraryObjectPrefab.Instantiate();

                ItemLibraryObjectData item = instance as ItemLibraryObjectData;

                item.showRemoveButton = false;

                if (item == null)
                    continue;

                item.removeEvent += AnimationItem_RemoveEventHandler;
                item.Invalidate(_animationResources[i]);

                VBoxContainerAnimation.AddChild(instance);
            }
        }

        private void AnimationItem_RemoveEventHandler(string path)
        {
            //animationResources.Remove(path);
            //RedrawAnimationsResourses();
            //_addGameObjectAssetModel?.RemoveAnimationResources(path);
        }


        private void AddGameObjectAssetModel_GameObjectAddAssetToCollection_EventHandler(object sender, EventArgs e)
        {
            ClearData();
            OnShow(false);
        }

        private async void RedrawAll()
        {
            TextEditModelName.Text = _addGameObjectAssetModel.ModelName;

            TextEditPath3DModel.Text = _addGameObjectAssetModel.template.Sources.Model3dFilePath;
            TextEditSampleObject.Text = _addGameObjectAssetModel.template.GameObjectSample;
            TextEditGraphXmlPath.Text = _addGameObjectAssetModel.template.GraphXmlPath;

            TextureRectPreviewImage.Texture = await _addGameObjectAssetModel.assetInfo.GetPreviewImage();

            RedrawSoundResourses();
            RedrawAnimationsResourses();
        }
    }
}
