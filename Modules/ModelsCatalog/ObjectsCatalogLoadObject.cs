using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;


public partial class ObjectsCatalogLoadObject : Control
{
    FileDialogTool dialogTool;

    private string modelPath;
    private string destPath;

    private System.Collections.Generic.Dictionary<string, string> soundResources = new System.Collections.Generic.Dictionary<string, string>();
    private System.Collections.Generic.Dictionary<string, string> animationResources = new System.Collections.Generic.Dictionary<string, string>();

    public string[] typeModel = { "Деревья", "Трава", "Камни", "Строения", "Животные", "Предметы", "Освещение" };

    [Export]
    Label LabelTittle;

    [Export]
    TextEdit TextEditModelName;

    [Export]
    TextEdit TextEditPath3DModel;

    [Export]
    OptionButton OptionButtonTypeObject;

    [Export]
    TextEdit TextEditPathSound;

    [Export]
    VBoxContainer VBoxContainerSound;

    [Export]
    PackedScene ItemLibraryObjectPrefab;

    [Export]
    TextEdit TextEditPathAnimation;

    [Export]
    VBoxContainer VBoxContainerAnimation;

    [Export]
    Button ButtonLoadModel;

    [Export]
    Button ButtonSaveEditModel;

    [Export]
    Button ButtonRemoveModel;

    ObjectData modelCurrent;

    public override void _Ready()
    {
        dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

        for (int i = 0; i < typeModel.Length; i++)
        {
            OptionButtonTypeObject.AddItem(typeModel[i]);
        }

        ButtonLoadModel.ButtonDown += OnLoadModel;
        ButtonSaveEditModel.ButtonDown += OnSaveEditModel;
        ButtonRemoveModel.ButtonDown += OnRemoveModel;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ButtonLoadModel.ButtonDown -= OnLoadModel;
        ButtonSaveEditModel.ButtonDown -= OnSaveEditModel;
        ButtonRemoveModel.ButtonDown -= OnRemoveModel;
    }

    public void OnOpenLoadObject()
    {
        Visible = true;
        ClearData();
        LabelTittle.Text = "Загрузить объект";
        ButtonLoadModel.Visible = true;
        ButtonSaveEditModel.Visible = false;
        ButtonRemoveModel.Visible = false;
    }

    public void OnOpenModel()
    {
        ModelLoader.OpenObj((path, mesh) =>
        {
            if (mesh != null)
            {
                modelPath = path;
                TextEditPath3DModel.Text = path;
                destPath = VoxLib.mapManager.GetProjectFolderPath() + MapManager.PATHCATALOG + Path.GetFileNameWithoutExtension(path) + "/";
            }
            else
            {
                GD.PrintErr($"Ошибка модели {path} в {destPath}");
            }
        });
    }


    List<string> audiosTo = new List<string>();
    List<string> animationsTo = new List<string>();

    public void OnOpenSound()
    {
        VoxLib.mapManager.SetCameraCursorShow(true);

        dialogTool.Open(new string[] { "*.mp3 ; mp3", "*.ogg ; ogg", "*.aac ; aac" }, (path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                TextEditPathSound.Text = path;
                soundResources.Add( Path.GetFileName(path), path );
                RedrawSoundResourses();
            }
            else
                GD.PrintErr($"Ошибка открытия звука {path} в {destPath}");
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

            item.removeEvent += SoundItem_removeEventHandler;
            item.Invalidate(_soundResources[i]);

            VBoxContainerSound.AddChild(instance);
        }
    }

    void SoundItem_removeEventHandler(string path)
    {
        soundResources.Remove(path);
        RedrawSoundResourses();
    }

    public void OnOpenAnimation()
    {
        VoxLib.mapManager.SetCameraCursorShow(true);
        dialogTool.Open(new string[] { "*.glb ; Анимация glb" }, (path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                TextEditPathAnimation.Text = path;
                animationResources.Add(Path.GetFileName(path), path);
                RedrawAnimationsResourses();
            }
            else
                GD.PrintErr($"Ошибка  открытия анимации {path} в {destPath}");
        }
, FileDialog.AccessEnum.Filesystem);
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

            item.removeEvent += AnimationItem_removeEventHandler;
            item.Invalidate(_animationResources[i]);

            VBoxContainerAnimation.AddChild(instance);
        }
    }

    void AnimationItem_removeEventHandler(string path)
    {
        animationResources.Remove(path);
        RedrawAnimationsResourses();
    }

    public void OnLoadModel()
    {
        MapManager.CreateDir(destPath);

        ModelLoader.CopyModel(modelPath, destPath);
        audiosTo = new List<string>();
        List<string> audios = new List<string>(soundResources.Keys);
        for (int i = 0; i < audios.Count; i++)
        {
            string key = audios[i];
            if (soundResources.ContainsKey(key))
            {
                string pathFrom = soundResources[key];
                string pathTo = destPath + Path.GetFileName(soundResources[key]);
                File.Copy(pathFrom, ProjectSettings.GlobalizePath(pathTo), true);
                audiosTo.Add(pathTo);
            }              
        }

        List<string> animations = new List<string>(animationResources.Keys);
        animationsTo = new List<string>();
        for (int i = 0; i < animations.Count; i++)
        {
            string key = animations[i];
            if (animationResources.ContainsKey(key))
            {
                string pathFrom = animationResources[key];
                string pathTo = destPath + Path.GetFileName(animationResources[key]);
                File.Copy(pathFrom, ProjectSettings.GlobalizePath(pathTo), true);
                animationsTo.Add(pathTo);
            }
        }

        var model = new ObjectData
        {
            Id = Guid.NewGuid().ToString(),
            Name = TextEditModelName.Text,
            modelPath = destPath + Path.GetFileName(modelPath),
            Type = OptionButtonTypeObject.Text,
            Sounds = audiosTo,
            Animations = animationsTo,
        };

        ObjectsCatalog.instance.AddModel(model);

        Visible = false;
        ClearData();
    }

    private ObjectData GetDataModel()
    {
        var model = new ObjectData
        {
            Id = Guid.NewGuid().ToString(),
            Name = TextEditModelName.Text,
            modelPath = destPath + Path.GetFileName(modelPath),
            Type = OptionButtonTypeObject.Text,
            Sounds = audiosTo,
            Animations = animationsTo,
        };

        return model;
    }

    private void ClearData()
    {
        modelCurrent = null;

        modelPath = "";
        destPath = "";

        TextEditModelName.Text = "";
        TextEditPath3DModel.Text = "";
        TextEditPathAnimation.Text = "";
        TextEditPathSound.Text = "";

        VoxLib.RemoveAllChildren(VBoxContainerSound);
        VoxLib.RemoveAllChildren(VBoxContainerAnimation);
    }

    public void OpenEditModel(ObjectData model)
    {
        ClearData();

        LabelTittle.Text = "Отредактировать объект";

        modelCurrent = model;

        TextEditModelName.Text = model.Name;
        modelPath = model.modelPath;
        TextEditPath3DModel.Text = modelPath;
        OptionButtonTypeObject.Text = model.Type;

        audiosTo = model.Sounds;
        animationsTo = model.Animations;

        ButtonLoadModel.Visible = false;
        ButtonSaveEditModel.Visible = true;
        ButtonRemoveModel.Visible = true;
    }

    public void OnSaveEditModel()
    {
        ObjectsCatalog.instance.RemoveModel(modelCurrent.Id);
        modelCurrent = GetDataModel();
        ObjectsCatalog.instance.AddModel(modelCurrent);
    }

    public void OnRemoveModel()
    {
        ObjectsCatalog.instance.RemoveModel(modelCurrent.Id);
        ClearData();
    }
}
