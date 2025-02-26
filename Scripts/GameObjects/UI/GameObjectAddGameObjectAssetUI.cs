using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAddGameObjectAssetUI : Control, IInjectable
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
        OptionButton OptionButtonTypeObject;

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

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _addUserSourceProvider;

        [Inject]
        private ISingletonProvider<FileDialogTool> _fileDialogToolProvider;

        private GameObjectAddGameObjectAssetModel _gameObjectAddUserSource;
        private FileDialogTool dialogTool;

        public string[] typeModel = { "Деревья", "Трава", "Камни", "Строения", "Животные", "Предметы", "Освещение" };

        public event EventHandler ButtonAddUserSourceDown_EventHandler;
        public event EventHandler ButtonCloseDown_EventHandler;

        private string modelPath;
        private string destPath;

        List<string> audiosTo = new List<string>();
        List<string> animationsTo = new List<string>();

        private System.Collections.Generic.Dictionary<string, string> soundResources = new System.Collections.Generic.Dictionary<string, string>();
        private System.Collections.Generic.Dictionary<string, string> animationResources = new System.Collections.Generic.Dictionary<string, string>();


        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            ButtonAddUserSource.ButtonDown += AddUserSourceButton_DownEventHandler;
            ButtonClose.ButtonDown += ButtonClose_DownEventHandler;
            ButtonOpen3DModel.ButtonDown += ButtonOpen3DModel_DownEventHandler;
            ButtonOpenPathSound.ButtonDown += ButtonOpenPathSound_DownEventHandler;
            ButtonOpenPathAnimation.ButtonDown += ButtonOpenPathAnimation_DownEventHandler;

            for (int i = 0; i < typeModel.Length; i++)
            {
                OptionButtonTypeObject.AddItem(typeModel[i]);
            }

            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectAddUserSource = await _addUserSourceProvider.GetAsync();
            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog); // await _fileDialogToolProvider.GetAsync();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonAddUserSource.ButtonDown -= AddUserSourceButton_DownEventHandler;
            ButtonClose.ButtonDown -= ButtonClose_DownEventHandler;
            ButtonOpen3DModel.ButtonDown -= ButtonOpen3DModel_DownEventHandler;
            ButtonOpenPathSound.ButtonDown -= ButtonOpenPathSound_DownEventHandler;
            ButtonOpenPathAnimation.ButtonDown -= ButtonOpenPathAnimation_DownEventHandler;
        }

        public void OnShow(bool value)
        {
            Visible = value;

            ClearData();
            LabelTittle.Text = "Загрузить объект";
            ButtonAddUserSource.Visible = true;
            ButtonSaveEditModel.Visible = false;
            ButtonRemoveModel.Visible = false;
        }

        private void ClearData()
        {
            //modelCurrent = null;

            modelPath = "";
            destPath = "";

            TextEditModelName.Text = "";
            TextEditPath3DModel.Text = "";
            TextEditPathAnimation.Text = "";
            TextEditPathSound.Text = "";

            VoxLib.RemoveAllChildren(VBoxContainerSound);
            VoxLib.RemoveAllChildren(VBoxContainerAnimation);
        }

        async void AddUserSourceButton_DownEventHandler()
        {
            ControlPopupMenu.instance._HideAllMenu();
            var model = _addUserSourceProvider != null ? await _addUserSourceProvider.GetAsync() : null;

            GameObjectAssetSources gameObjectAsset = new GameObjectAssetSources("", "", destPath);

            model.SetDestPath(destPath);

            model?.SetAddUserSourceToCollection(TextEditModelName.Text, gameObjectAsset);
        }

        async void ButtonClose_DownEventHandler()
        {
            var viewModel = _addUserSourceProvider != null ? await _addUserSourceProvider.GetAsync() : null;
            viewModel?.SetGameObjectAddUserSourceVisible(false);
        }

        void ButtonOpen3DModel_DownEventHandler()
        {
            ModelLoader.OpenObj(async (path, mesh) =>
            {
                if (mesh != null)
                {
                    modelPath = path;
                    TextEditPath3DModel.Text = path;
                    destPath = GameObjectAssetsUserSource.CollectionPath + Path.GetFileNameWithoutExtension(path) + "/";

                    var viewModel = _addUserSourceProvider != null ? await _addUserSourceProvider.GetAsync() : null;
                    viewModel?.SetModelPath(path);
                    viewModel?.SetDestPath(destPath);
                }
                else
                {
                    GD.PrintErr($"Ошибка модели {path} в {destPath}");
                }
            });

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
                    _gameObjectAddUserSource?.AddSoundResources(path);
                }
                else
                    GD.PrintErr($"Ошибка открытия звука {path} в {destPath}");
            }
, FileDialog.AccessEnum.Filesystem);
        }


        void ButtonOpenPathAnimation_DownEventHandler()
        {
            dialogTool.Open(new string[] { "*.glb ; Анимация glb" }, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    TextEditPathAnimation.Text = path;
                    animationResources.Add(Path.GetFileName(path), path);
                    RedrawAnimationsResourses();
                    _gameObjectAddUserSource?.AddAnimationResources(path);
                }
                else
                    GD.PrintErr($"Ошибка  открытия анимации {path} в {destPath}");
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
            _gameObjectAddUserSource?.RemoveSoundResources(path);
        }

        void RedrawAnimationsResourses()
        {
            VoxLib.RemoveAllChildren(VBoxContainerAnimation);

            List<string> _animationResources = new List<string>(animationResources.Keys);

            for (int i = 0; i < _animationResources.Count; i++)
            {
                Node instance = ItemLibraryObjectPrefab.Instantiate();

                ItemLibraryObjectData item = instance as ItemLibraryObjectData;

                if (item == null)
                    continue;

                item.removeEvent += AnimationItem_RemoveEventHandler;
                item.Invalidate(_animationResources[i]);

                VBoxContainerAnimation.AddChild(instance);
            }
        }

        void AnimationItem_RemoveEventHandler(string path)
        {
            animationResources.Remove(path);
            RedrawAnimationsResourses();
            _gameObjectAddUserSource?.RemoveAnimationResources(path);
        }
    }
}
