using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


public partial class LogScript : Node
{
    static int LINECOUNT = 20;

    [Export]
    public Control LogControl;

    [Export]
    public FileDialog fileDialog;

    [Export]
    public Label messageLabel;
    private string logTXT;

    [Export]
    public TextEdit filterText;

    [Export]
    public OptionButton optionFilterButton;
    private string optionFilter;

    [Export]
    public MenuButton FilterMenuButton;
    System.Collections.Generic.Dictionary<int, bool> FilterItems = new System.Collections.Generic.Dictionary<int, bool>();

    public bool isVisibleLog = true;

    private List<int> selectedItems = new List<int>();
    PopupMenu popupMenu;

    public static bool isLogEntered = false;

    public override void _Ready()
    {
        Init();
    }

    private void Init()
    {
        VoxLib.log = this;

        messageLabel.Visible = false;
        messageLabel.Text = "";

        popupMenu = optionFilterButton.GetPopup();

        //optionFilterButton.Clear();
        //optionFilterButton.AddItem("Нет фильтров");
        //optionFilterButton.AddItem("ИгрокОбнаружен");

        popupMenu = FilterMenuButton.GetPopup();
        popupMenu.Connect("id_pressed", new Callable(this, nameof(ChangeFilter)));
        selectedItems.Add(0);
    }

    public void ClearLog()
    {
        messageLabel.Text = "";
        logTXT = "";
    }

    public override void _Input(InputEvent @event)
    {
        if ((Input.IsKeyPressed(Key.Asciitilde) || Input.IsKeyPressed(Key.Quoteleft)) && @event.IsPressed())
        {
            isVisibleLog = !isVisibleLog;
            LogControl.Visible = isVisibleLog;
        }

        //foreach (Key key in Enum.GetValues(typeof(Key)))
        //{
        //    if (Input.IsKeyPressed(key))
        //    {
        //        GD.Print("Key pressed: " + key.ToString());
        //    }
        //}
    }

    public void HideLog()
    {
        isVisibleLog = false;
        LogControl.Visible = isVisibleLog;
    }

    public void ShowMessage(string message)
    {
        messageLabel.Visible = true;
        messageLabel.Text += message + "\n";
        logTXT += message + "\n";

        //if (messageLabel.GetLineCount() > LINECOUNT)
        //{
        //    int newLinePosition = messageLabel.Text.IndexOf("\n");
        //    if (newLinePosition != -1)
        //    {
        //        string currentText = messageLabel.Text.Substring(newLinePosition + 1);
        //        messageLabel.Text = currentText;
        //    }
        //}

        SetFilterText();
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

        SetFilterText();
    }

    void ResetFilter()
    {
        for (int i = 0; i < popupMenu.ItemCount; i++)
        {
            popupMenu.SetItemChecked(i, false);
        }
        popupMenu.SetItemChecked(0, true);
    }

    public void SetFilterText()
    {
        messageLabel.Text = GetFilterLog();
    }

    private string GetFilterLog()
    {
        string log = "";
        int count = 0;

        if (string.IsNullOrEmpty(logTXT)) return "";

        using (StringReader reader = new StringReader(logTXT))
        {
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

                if (isAdd)
                {
                    log += line + "\n";
                    count++;
                    if (count > LINECOUNT) break;
                }

                //if (line.Contains(filterText.Text))
                //{
                //    if (optionFilterButton.Selected > 0)
                //    {
                //        if (line.Contains(optionFilter))
                //        {
                //            log += line + "\n";
                //            count++;
                //            if (count > LINECOUNT) break;
                //        }
                //    }
                //    else
                //    {
                //        log += line + "\n";
                //        count++;
                //        if (count > LINECOUNT) break;
                //    }
                //}


            }
        }

        string log2 = "";
        foreach (string line in log.Split('\n').Reverse())
        {
            log2 += line + "\n";
        }

        return log2;
    }

    public void OnSelectFilterMenuButton()
    {

    }

    string currentFile = "";
    public void SaveLog()
    {
        fileDialog.Filters = new string[] { "*.csv ; csv" };

        fileDialog.CurrentFile = currentFile;

        VoxLib.mapManager.lastDirectory = VoxLib.mapManager.LoadLastDirectory();

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

    private void SaveToFile(string file)
    {
        string savePath = file;

        using (StreamWriter writer = new StreamWriter(savePath, false, Encoding.UTF8))
        {
            writer.Write(logTXT);
        }

        VoxLib.mapManager.lastDirectory = VoxLib.mapManager.fileDialog.CurrentDir;
        VoxLib.mapManager.SaveLastDirectory(VoxLib.mapManager.lastDirectory);
        currentFile = fileDialog.CurrentFile;
        GD.Print("Log saved to: " + savePath);
    }

    public void LogEntered()
    {
        isLogEntered = true;
    }

    public void LogExited()
    {
        isLogEntered = false;
        //filterText.DeselectOnFocusLossEnabled = true;
        filterText.ReleaseFocus();
    }

    public bool isDialogOpen
    {
        get
        {
            return fileDialog.Visible;
        }
    }
}
