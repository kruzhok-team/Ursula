using Godot;
using System;
using Ursula.Core.DI;
using Ursula.Environment.Settings;

public partial class FileDialogTool : Node
{
    [Inject]
    private ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider;

    FileDialog _fileDialog;

    public FileDialogTool(FileDialog fileDialog)
    {
        _fileDialog = fileDialog;

        fileDialog.Visible = false;

        CheckButton checkButton = _fileDialog.FindChild("AccessCheckButton", true, false) as CheckButton;

        if (checkButton != null)
        {
            checkButton.Connect("toggled", new Callable(this, nameof(OnCheckButtonToggled)));
        }   
        else
        {
            GD.Print("CheckButton не найден среди дочерних узлов.");
        }
    }

    private string lastDirectory = "";
    private string settingsPath = "user://settings.cfg";

    Action<string> _onFileSelected;

    private void OnCheckButtonToggled(bool buttonPressed)
    {
        if (_fileDialog.Visible)
        {
            _fileDialog.Hide();
        }

        if (buttonPressed)
        {
            _fileDialog.Access = FileDialog.AccessEnum.Filesystem;
            GD.Print("Access: Filesystem");
        }
        else
        {
            _fileDialog.Access = FileDialog.AccessEnum.Userdata;
            GD.Print("Access: Userdata");
        }

        _fileDialog.Show();
    }

    public void Open(string[] filters, Action<string> onFileSelected, FileDialog.AccessEnum access = FileDialog.AccessEnum.Userdata)
    {
        _onFileSelected = onFileSelected;

        _fileDialog.Filters = filters;

        _fileDialog.Access = access;

        lastDirectory = LoadLastDirectory;

        if (!string.IsNullOrEmpty(lastDirectory))
        {
            _fileDialog.CurrentPath = lastDirectory;
        }

        if (!_fileDialog.IsConnected("file_selected", new Callable(this, nameof(OnFileSelected))))
        {
            _fileDialog.Connect("file_selected", new Callable(this, nameof(OnFileSelected)));
        }

        if (!_fileDialog.IsConnected("canceled", new Callable(this, nameof(OnDialogClosed))))
        {
            _fileDialog.Connect("canceled", new Callable(this, nameof(OnDialogClosed)));
        }

        _fileDialog.Show();
    }

    void OnFileSelected(string selectedFile)
    {
        _onFileSelected?.Invoke(selectedFile);

        SaveLastDirectory(selectedFile);
    }

    void OnDialogClosed()
    {
        _onFileSelected?.Invoke("");

        SaveLastDirectory(_fileDialog.CurrentDir + "/" + _fileDialog.CurrentFile);
    }

    private void SaveLastDirectory(string path)
    {
        if (TryGetSettingsModel(out var settingsModel))
            settingsModel.SetLastDirectory(path).Save();
    }

    private string LoadLastDirectory
    {
        get
        {
            if (!TryGetSettingsModel(out var settingsModel))
                return "";
            return settingsModel.LastDirectory;
        }
    }

    public void DirContents(string path)
    {
        using var dir = DirAccess.Open(path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (dir.CurrentIsDir())
                {
                    GD.Print($"Found directory: {fileName}");
                }
                else
                {
                    GD.Print($"Found file: {fileName}");
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.Print("An error occurred when trying to access the path.");
        }

        string[] files = dir.GetFiles();
    }

    private bool TryGetSettingsModel(out EnvironmentSettingsModel model, bool errorIfNotExist = false)
    {
        model = null;

        if (!(_settingsModelProvider?.TryGet(out model) ?? false))
        {
            if (errorIfNotExist)
                GD.PrintErr($"{typeof(FileDialogTool).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
        }
        return model != null;
    }
}
