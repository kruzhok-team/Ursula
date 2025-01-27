using Godot;
using System;

public partial class FileDialogTool : Node
{
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

        lastDirectory = LoadLastDirectory();

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
        ConfigFile config = new ConfigFile();
        config.Load(settingsPath);
        config.SetValue("FileDialog", "last_directory", path);
        config.Save(settingsPath);
    }

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
}
