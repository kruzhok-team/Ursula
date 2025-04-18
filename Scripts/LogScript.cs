using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ursula.Core.DI;
using Ursula.Log.Model;
using Ursula.Log.View;
using Ursula.StartupMenu.Model;


public partial class LogScript : LogModel, IInjectable
{
    [Export]
    public Control LogControl;

    [Export]
    public FileDialog fileDialog;

    [Export]
    public TextEdit filterText;

    [Export]
    public OptionButton optionFilterButton;

    [Export]
    public MenuButton FilterMenuButton;

    [Export]
    private Button PauseButton;

    [Export]
    PackedScene LogItemViewPrefab;

    [Export]
    private VBoxContainer VBoxContainerLogItemViews;

    [Inject]
    private ISingletonProvider<LogModel> _LogModelProvider;

    public static bool isLogEntered = false;

    private string optionFilter;
    private List<int> selectedItems = new List<int>();
    private PopupMenu popupMenu;

    private List<LogItemView> _cachedLogItemsView = new List<LogItemView>();

    private string currentFile = "";

    private LogModel _logModel { get; set; }

    private List<string> filteredLogTXT = new List<string>();

    private float accumulator = 0f;

    private bool isPaused = false;
    private bool isUpdatedLog = false;

    void IInjectable.OnDependenciesInjected()
    {
    }

    public override void _Ready()
    {
        base._Ready();
        Init();
        _ = SubscribeEvent();
    }

    private async GDTask SubscribeEvent()
    {
        _logModel = await _LogModelProvider.GetAsync();
        _logModel.ViewVisible_EventHandler += LogModel_ViewVisible_EventHandler;
        _logModel.SetMessage_EventHandler += LogModel_SetMessage_EventHandler;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (isPaused) return;

        accumulator += (float)delta;

        if (accumulator >= 0.3f)
        {
            accumulator = 0f;

            _= SetFilterText();
        }
    }

    private void Init()
    {
        VoxLib.log = this;

        VoxLib.RemoveAllChildren(VBoxContainerLogItemViews);

        filteredLogTXT = new List<string>();

        popupMenu = optionFilterButton.GetPopup();

        popupMenu = FilterMenuButton.GetPopup();
        popupMenu.Connect("id_pressed", new Callable(this, nameof(ChangeFilter)));
        selectedItems.Add(0);

        PauseButton.ButtonDown += PauseButton_ButtonDownEvent;
    }

    public void ClearLog()
    {
        SetClearLogs();
        VoxLib.RemoveAllChildren(VBoxContainerLogItemViews);
    }

    public override void _Input(InputEvent @event)
    {
        if ((Input.IsKeyPressed(Key.Asciitilde) || Input.IsKeyPressed(Key.Quoteleft)) && @event.IsPressed())
        {
             _logModel?.SetVisibleView(!_logModel.isVisibleLog);
        }
    }

    public void ChangeFilter(int value)
    {
        if (value > 0 && selectedItems.Contains(0)) selectedItems.Remove(0);

        if (selectedItems.Contains(value))
        {
            selectedItems.Remove(value);
            if (value == 0)
            {
                for (int i = 1; i < popupMenu.ItemCount; i++) selectedItems.Add(i);
            }
        }
        else
        {
            selectedItems.Add(value);
        }

        if (selectedItems.Contains(0) || selectedItems.Count == 0)
        {
            selectedItems.Clear();
            selectedItems.Add(0);
        }

        for (int i = 0; i < popupMenu.ItemCount; i++)
        {
            bool isChecked = selectedItems.Contains(i);
            popupMenu.SetItemChecked(i, isChecked);
        }

        _= SetFilterText();
    }

    public async GDTask SetFilterText()
    {
        if (isPaused) return;
        if (!isUpdatedLog) return;

        await GetFilterLog();

        if (filteredLogTXT == null) return;

        if (_logModel.isVisibleLog)
        {
            VoxLib.RemoveAllChildren(VBoxContainerLogItemViews);
            SetItemsLog(filteredLogTXT);
        }

        isUpdatedLog = false;
    }

    public void OnSelectFilterMenuButton()
    {

    }

    public void SaveLog()
    {
        fileDialog.Filters = new string[] { "*.csv ; csv" };

        fileDialog.CurrentFile = currentFile;

        VoxLib.mapManager.lastDirectory = VoxLib.mapManager.LoadLastDirectory;

        if (!string.IsNullOrEmpty(VoxLib.mapManager.lastDirectory))
        {
            fileDialog.CurrentDir = VoxLib.mapManager.lastDirectory;
        }

        if (!fileDialog.IsConnected("file_selected", new Callable(this, nameof(SaveToFile))))
        {
            fileDialog.Connect("file_selected", new Callable(this, nameof(SaveToFile)));
        }

        fileDialog.Show();
    }

    public void LogEntered()
    {
        isLogEntered = true;
    }

    public void LogExited()
    {
        isLogEntered = false;
        filterText.ReleaseFocus();
    }

    public bool isDialogOpen
    {
        get
        {
            return fileDialog.Visible;
        }
    }






    private async GDTask GetFilterLog()
    {
        string log = "";
        int count = 0;

        if (_logModel == null) _logModel = await _LogModelProvider.GetAsync();

        if (_logModel == null)
        {
            GD.PrintErr($"{typeof(LogScript).Name}: {typeof(LogModel).Name} is not instantiated!");
            return;
        }

        string logTXT = _logModel?.GetLogs();

        filteredLogTXT = new List<string>();

        using (StringReader reader = new StringReader(logTXT))
        {
            int id = 0;
            foreach (string line in logTXT.Split('\n').Reverse())
            {
                bool isAdd = false;

                if (!selectedItems.Contains(0))
                {
                    if (string.IsNullOrEmpty(filterText.Text) && filterText.Text != "" && line.Contains(filterText.Text)) isAdd = true;

                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        string expression = popupMenu.GetItemText(selectedItems[i]);
                        isAdd |= line.Contains(expression);
                    }
                }
                else
                {
                    if (line.Contains(filterText.Text)) isAdd = true;
                }
                id++;

                if (isAdd)
                {
                    log += line + "\n";
                    count++;

                    filteredLogTXT.Add(line);

                    if (count > LOG_LINE_COUNT_VISIBLE) break;
                }
            }
        }
    }


    private void ResetFilter()
    {
        for (int i = 0; i < popupMenu.ItemCount; i++)
        {
            popupMenu.SetItemChecked(i, false);
        }
        popupMenu.SetItemChecked(0, true);
    }

    private void SaveToFile(string file)
    {
        if (_logModel == null)
        {
            GD.PrintErr($"{typeof(LogScript).Name}: {typeof(LogModel).Name} is not instantiated!");
            return;
        }

        string savePath = file;
        string logTXT = _logModel?.GetLogs();

        using (StreamWriter writer = new StreamWriter(savePath, false, Encoding.UTF8))
        {
            writer.Write(logTXT);
        }

        VoxLib.mapManager.lastDirectory = VoxLib.mapManager.fileDialog.CurrentDir;
        VoxLib.mapManager.SaveLastDirectory(VoxLib.mapManager.lastDirectory);
        currentFile = fileDialog.CurrentFile;
        GD.Print("Log saved to: " + savePath);
    }

    private void SetItemsLog(List<string> logs)
    {
        for (int i = 0; i < logs.Count; i++)
        {
            DrawItem(logs[i]);
        }
    }

    private void DrawItem(string logText, int type = 0)
    {
        if (string.IsNullOrEmpty(logText)) return;

        Node instance = LogItemViewPrefab.Instantiate();
        LogItemView item = instance as LogItemView;

        if (item == null)
            return;

        item.SetTextLog(logText, type);

        VBoxContainerLogItemViews.AddChild(instance);
    }

    private void PauseButton_ButtonDownEvent()
    {
        isPaused = !isPaused;
    }



    private void LogModel_ViewVisible_EventHandler(object sender, EventArgs e)
    {
        if (_logModel == null) return;
        LogControl.Visible = _logModel.isVisibleLog;
    }

    private void LogModel_SetMessage_EventHandler(object sender, EventArgs e)
    {
        isUpdatedLog = true;
    }


}
