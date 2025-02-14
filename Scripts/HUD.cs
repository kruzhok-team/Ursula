using Godot;
using Ursula.Core.DI;
using Ursula.Environment.Settings;

public partial class HUD : Control, IInjectable
{
    [Export]
    public Label _labelFile;

    [Export]
    public Control _cross;

    [Export]
    public Label _labelCoordinates;

    [Export]
    public Control SettingsGO;

    [Export]
    public Slider SliderMouseSence;

    [Export]
    public OptionButton optionButtonShadow;

    [Export]
    public Label labelLengthOfDay;

    [Export]
    public Slider SliderLengthOfDay;

    [Export]
    public Control ControlProjectGO;

    [Export]
    public Control ControlGamesProjectGO;

    [Export]
    public CheckButton[] CheckButtonAlgoritm;

    [Export]
    public Slider SliderPlatoSize;

    [Export]
    public Slider SliderPlatoOffsetX;

    [Export]
    public Slider SliderPlatoOffsetZ;

    [Inject]
    private ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider;

    private float _defaultSensitivity = 0.5f;

    public float sensitivity
    {
        get 
        {
            if (!TryGetSettingsModel(out var settingsModel))
                return _defaultSensitivity;
            return settingsModel.Sensitivity;
        }
    }

    void IInjectable.OnDependenciesInjected()
    {
    }

    public override void _Ready()
    {
        base._Ready();

        ApplySettingsFromConfig();

        VoxLib.hud = this;
        SettingsGO.Visible = false;
    }

    Vector3 coord;
    public void SetCoordinate(Vector3 coord)
    {
        if (this.coord != coord)
        {
            this.coord = coord;
            _labelCoordinates.Text = $"Координаты: x={(int)coord.X} y={(int)coord.Z} z={(int)coord.Y}";
        }
    }

    public void SetInfo(string info)
    {
        if (_labelCoordinates.Text != info)
            _labelCoordinates.Text = info;
    }

    public void OnOpenSettings()
    {
        ControlPopupMenu.HideAllMenu();
        SettingsGO.Visible = true;
        SliderLengthOfDay.Value = LoadLengthOfDay();
    }

    public void OnCloseSettings()
    {
        SettingsGO.Visible = false;
    }

    public void OnOpenControlProject()
    {
        ControlPopupMenu.HideAllMenu();
        ControlProjectGO.Visible = true;
    }

    public void OnCloseControlProject()
    {
        ControlProjectGO.Visible = false;
    }

    public void OnOpenControlGameProject()
    {
        ControlPopupMenu.HideAllMenu();
        ControlGamesProjectGO.Visible = true;
        VoxLib.instance.CGP.Instantiate();
    }

    public void OnCloseControlGameProject()
    {
        ControlGamesProjectGO.Visible = false;
    }

    public void SetSensitivity(float sence)
    {
        if (TryGetSettingsModel(out var settingsModel))
            settingsModel.SetSensitivity(sence).Save();
    }

    public void SaveLengthOfDay(float value)
    {
        DayNightCycle.instance.FullDayLength = value;
        SliderLengthOfDay.Value = value;
        labelLengthOfDay.Text = $"Длительность суток: {value}c";
    }

    private float LoadLengthOfDay()
    {
        labelLengthOfDay.Text = $"Длительность суток: {DayNightCycle.instance.FullDayLength}c";
        return DayNightCycle.instance.FullDayLength;
    }

    public void SetShadowEnabled(int value)
    {
        if (TryGetSettingsModel(out var settingsModel))
            settingsModel.SetShadowEnabled(value).Save();
        SetShadow();
    }

    private void SetShadow()
    {
        if (!TryGetSettingsModel(out var settingsModel))
            return;

        var scene = GetTree().CurrentScene;
        var directionalLight = FindDirectionalLight(scene);

        directionalLight.ShadowEnabled = settingsModel.ShadowEnabled > 0;
    }

    private DirectionalLight3D FindDirectionalLight(Node node)
    {
        if (node is DirectionalLight3D directionalLight)
            return directionalLight;

        foreach (Node child in node.GetChildren())
        {
            var light = FindDirectionalLight(child);
            if (light != null)
                return light;
        }

        return null;
    }

    public static void ItemProcessing(Node collider, Vector3 raycastPos, out ItemPropsScript itemPropS, out Node parent)
    {
        parent = collider.GetParent();
        itemPropS = collider as ItemPropsScript;

        int _x = Mathf.RoundToInt(raycastPos.X);
        int _y = Mathf.RoundToInt(raycastPos.Y);
        int _z = Mathf.RoundToInt(raycastPos.Z);

        if (itemPropS == null) itemPropS = parent as ItemPropsScript;
        if (itemPropS == null)
        {
            Node ips = collider.FindChild("ItemPropsScript", true, true);
            if (ips == null) ips = collider.GetParent().FindChild("ItemPropsScript", true, true);
            if (ips != null)
            {
                itemPropS = ips as ItemPropsScript;
                parent = ips.GetParent();
            }
        }
    }    

    public static string GetCordsInfo(Node collider, Vector3 raycastPos, ItemPropsScript itemPropS, Node parent)
    {
        string name = "";
        string info = "";
        if (itemPropS != null)
        {
            name = (parent != null) ? parent.Name : collider.Name;
            info = $"Имя={name}, Координаты: x={(int)itemPropS.x} y={(int)itemPropS.z} z={(int)itemPropS.y}";
        }
        else
        {
            info = $"Координаты: x={(int)raycastPos.X} y={(int)raycastPos.Z} z={(int)raycastPos.Y}";
        }

        return info;
    }

    public void OnExportProject()
    {
        VoxLib.mapManager.ExportProject();
    }

    public void OnImportProject()
    {
        VoxLib.mapManager.ImportProject();
    }

    bool algoritmStart = false;
    public async void OnAlgoritmStartStopChange()
    {
        algoritmStart = !algoritmStart;

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        for (int i = 0; i < CheckButtonAlgoritm.Length; i++)
            if (CheckButtonAlgoritm[i].ButtonPressed != algoritmStart) CheckButtonAlgoritm[i].ButtonPressed = algoritmStart;

        OnAlgoritmStartStopChange(algoritmStart);
    }

    public void OnAlgoritmStartStopChange(bool value)
    {
        algoritmStart = value;
        ControlPopupMenu.HideAllMenu();

        if (algoritmStart) RunAllObjects();
        else StopAllObjects();
    }

    public void RunAllObjects()
    {
        InteractiveObjectsManager.Instance.RunAllObjects();
    }

    public void StopAllObjects()
    {
        InteractiveObjectsManager.Instance.StopAllObjects();
    }

    public void OnOpenGameImage()
    {
        VoxLib.mapManager.OpenGameImage();
    }

    public void SetNameMap(string nameMap)
    {
        _labelFile.Text = "Игра: " + nameMap;
    }

    private async void ApplySettingsFromConfig()
    {
        var model = _settingsModelProvider != null ? await _settingsModelProvider.GetAsync() : null;

        if (model == null)
        {
            GD.PrintErr($"{typeof(HUD).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
            return;
        }

        SliderMouseSence.Value = model.Sensitivity;
        optionButtonShadow.Selected = model.ShadowEnabled;
        SetShadow();
    }

    private bool TryGetSettingsModel(out EnvironmentSettingsModel model, bool errorIfNotExist=false)
    {
        model = null;

        if (!(_settingsModelProvider?.TryGet(out model) ?? false))
        {
            if (errorIfNotExist)
                GD.PrintErr($"{typeof(HUD).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
        }
        return model != null;
    }
}
