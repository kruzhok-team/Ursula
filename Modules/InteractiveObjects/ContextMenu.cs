using Godot;
using System;
using System.Threading.Tasks;

public partial class ContextMenu : Control
{
    Panel contextPanel;
    Panel modelActionsPanel;
    Button setModelButton;
    Panel GMLActionsPanel;
    Label GMLActionsLabel;

    Button button1;
    Button button2;
    FileDialog fileDialog;
    Label messageLabel;

    InteractiveObject interactiveObject;
    CustomObject customObject;

    public static ContextMenu instance;

    public bool isOpened = false;
    public bool isVisibleLog = true;

    FileDialogTool dialogTool;

    private void Init()
    {
        instance = this;

        dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

        contextPanel = GetNode("GMLActions") as Panel;
        modelActionsPanel = GetNode("ModelActions") as Panel;
        setModelButton = GetNode("ModelActions/SetModel") as Button;

        button1 = GetNode("GMLActions/Load") as Button;
        button2 = GetNode("GMLActions/Reload") as Button;
        GMLActionsPanel = GetNode("GMLActions/Panel") as Panel;
        GMLActionsLabel = GetNode("GMLActions/GMLActionsLabel") as Label;

        fileDialog = GetNode("FileDialog") as FileDialog;
        messageLabel = GetNode("MessageLabel") as Label;

        contextPanel.Visible = false;
        modelActionsPanel.Visible = false;
        messageLabel.Visible = false;
        messageLabel.Text = "";
        GMLActionsLabel.Text = "";
    }

    public static void ShowMessageS(string message)
    {
        //instance.ShowMessage(message);

        var print = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] > " + message;
        VoxLib.log.ShowMessage(print);
        GD.Print(print);
    }

    public async void ShowMessage(string message)
    {
        VoxLib.log.ShowMessage(message);
        await Task.Delay(0);
    }

    public static void Open(InteractiveObject target)
    {
        instance.OpenPanel(target);
    }

    public void OpenPanel(InteractiveObject target)
    {
        //interactiveObject = _target?.FindChild("InteractiveObject") as InteractiveObject;
        interactiveObject = target;

        contextPanel.Visible = true;

        VoxLib.mapManager.SetCameraCursorShow(true);

        isOpened = true;

        GMLActionsLabel.Text = interactiveObject.xmlPath;
        GMLActionsPanel.Visible = GMLActionsLabel.Text.Length > 0;
    }

    public override void _Ready()
    {
        Init();

        Close();
    }

    public static void Close()
    {
        instance.ClosePanel();
    }

    public void ClosePanel()
    {
        contextPanel.Visible = false;
        modelActionsPanel.Visible = false;
        isOpened = false;
    }

    public void LoadAlgorithm()
    {
        OpenFile();
    }

    public void RestartAlgorithm()
    {
        interactiveObject.move?.StopMoving();

        interactiveObject.ReloadAlgorithm();
        interactiveObject.StartAlgorithm();
        Close();

        ShowMessage("Алгоритм перезапущен");
    }

    public void SetModel()
    {
        Close();
        customObject?.LoadNewModel();
    }

    public void OpenFile()
    {
        fileDialog.Filters = new string[] { "*.graphml ; Graphml" };

        lastDirectory = LoadLastDirectory();

        if (!string.IsNullOrEmpty(lastDirectory))
        {
            fileDialog.CurrentDir = lastDirectory;
        }

        if (!fileDialog.IsConnected("file_selected", new Callable(this, nameof(FileProcess))))
        {
            fileDialog.Connect("file_selected", new Callable(this, nameof(FileProcess)));
        }

        fileDialog.Show();
    }

    private string lastDirectory = "";
    private string settingsPath = "user://settings.cfg";
    public void FileProcess(string path)
    {
        interactiveObject.LoadAlgorithm(path);
        ShowMessage("Алгоритм успешно загружен");
        Close();

        lastDirectory = fileDialog.CurrentDir;

        // Сохранение последнего пути в файл конфигурации
        SaveLastDirectory(lastDirectory);
    }

    // Сохранение пути в файл конфигурации
    private void SaveLastDirectory(string path)
    {
        ConfigFile config = new ConfigFile();
        config.Load(settingsPath);
        config.SetValue("FileDialog", "last_directory", path);
        config.Save(settingsPath);
    }

    // Загрузка пути из файла конфигурации
    private string LoadLastDirectory()
    {
        ConfigFile config = new ConfigFile();
        Error err = config.Load(settingsPath);
        if (err == Error.Ok)
        {
            return (string)config.GetValue("FileDialog", "last_directory", "");
        }
        return "";
    }


    public override void _Input(InputEvent @event)
    {
        if (Input.IsKeyPressed(Key.Ctrl) && @event.IsPressed())
        {
            if (isOpened)
            {
                VoxLib.mapManager.SetCameraCursorShow(false);
                Close();
            }
            else
            {
                if (Raycaster.Hit(VoxLib.mapManager.GetPlayerCamera(), GetViewport().GetMousePosition(), out Node collider, out Vector3 pos))
                {
                    var obj = collider.GetNodeOrNull("InteractiveObject");

                    if (obj == null) 
                        obj = collider.GetParent().GetNodeOrNull("InteractiveObject");

                    var io = obj as InteractiveObject;

                    if (io != null)
                    {
                        GD.Print($"Xml path: {io.xmlPath}");
                        Open(io);
                    }
                    else
                    {
                        GD.Print($"InteractiveObject is not detected");
                    }

                    customObject = collider.GetNodeOrNull("CustomObjectScript") as CustomObject;
                    modelActionsPanel.Visible = customObject != null;
                }

            }
        }
        else if (Input.IsKeyPressed(Key.Escape) && @event.IsPressed())
        {
            Close();
        }

        //if (Input.IsKeyPressed(Key.L) && @event.IsPressed())
        //{
        //    isVisibleLog = !isVisibleLog;
        //    messageLabel.Visible = isVisibleLog;
        //}
    }

    public void LoadSound()
    {
        //Close();
        //customObject?.LoadNewModel();
    }
}
