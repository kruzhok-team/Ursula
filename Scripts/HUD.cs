﻿using Fractural.Tasks;
using Godot;
using Ursula.Core.DI;
using Ursula.Environment.Settings;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using System.Threading.Tasks;
using System;
using Ursula.EmbeddedGames.Model;


public partial class HUD : Control, IInjectable
{
    [Export]
    public Label _labelFile;

    [Export]
    public Control _cross;

    [Export]
    public Label _labelCoordinates;

    [Export]
    public CheckButton[] CheckButtonAlgoritm;

    [Export]
    public Button ButtonOpenGameProjectView;

    [Inject]
    private ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider;

    [Inject]
    private ISingletonProvider<HUDViewModel> _hudModelProvider;

    [Inject]
    private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddUserSourceViewProvider;

    [Inject]
    private ISingletonProvider<GameObjectCurrentInfoModel> _GameObjectCurrentInfoModelProvider;

    [Inject]
    private ISingletonProvider<ControlEmbeddedGamesProjectViewModel> _controlEmbeddedGamesProjectViewModelProvider;

    private GameObjectCurrentInfoModel gameObjectCurrentInfoModel;

    private Vector3 coord;

    void IInjectable.OnDependenciesInjected()
    {
    }

    public override void _Ready()
    {
        base._Ready();

        VoxLib.hud = this;

        _ = SubscribeEvent();
    }

    private async GDTask SubscribeEvent()
    {
        gameObjectCurrentInfoModel = await _GameObjectCurrentInfoModelProvider.GetAsync();

        ButtonOpenGameProjectView.ButtonDown += ButtonOpenGameProjectView_ButtonDownEvent;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public void SetCoordinate(Vector3 coord)
    {
        if (this.coord != coord)
        {
            this.coord = coord;
            _labelCoordinates.Text = $"Координаты: x={(int)coord.X} y={(int)coord.Z} z={(int)coord.Y}";
        }
    }

    public void _SetInfo(string info)
    {
        if (_labelCoordinates.Text != info)
            _labelCoordinates.Text = info;
    }

    public void SetInfo(string info)
    {
        _SetInfo(info);
    }

    public async void ShowCommonLibraryButton_DownEventHandler()
    {
        ControlPopupMenu.instance._HideAllMenu();
        var viewModel = _hudModelProvider != null ? await _hudModelProvider.GetAsync() : null;
        viewModel?.SetGameObjectLibraryVisible(true);
    }

    public async void ShowAddUserSourceUiButton_DownEventHandler()
    {
        ControlPopupMenu.instance._HideAllMenu();
        var model = _gameObjectAddUserSourceViewProvider != null ? await _gameObjectAddUserSourceViewProvider.GetAsync() : null;
        model?.SetGameObjectAddGameObjectAssetVisible(true);
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
            info = $"Имя={name}, Координаты: x={(int)itemPropS.x} y={(int)itemPropS.z} z={(int)itemPropS.y}, Шаблон={itemPropS.GameObjectSample}";
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

    public void OnOpenGameVideo()
    {
        VoxLib.mapManager.OpenGameVideo();
    }

    public void SetNameMap(string nameMap)
    {
        _labelFile.Text = "Игра: " + nameMap;
    }

    public void OnOpenPanelLoadObject()
    {
        ControlPopupMenu.instance._HideAllMenu();
        ObjectsCatalog.instance.OnOpenPanelLoadObject();
    }

    private bool TryGetSettingsModel(out EnvironmentSettingsModel model, bool errorIfNotExist = false)
    {
        model = null;

        if (!(_settingsModelProvider?.TryGet(out model) ?? false))
        {
            if (errorIfNotExist)
                GD.PrintErr($"{typeof(HUD).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
        }
        return model != null;
    }

    private async void ButtonOpenGameProjectView_ButtonDownEvent()
    {
        ControlEmbeddedGamesProjectViewModel _controlEmbeddedGamesProjectViewModel = await _controlEmbeddedGamesProjectViewModelProvider.GetAsync();
        _controlEmbeddedGamesProjectViewModel.SetVisibleView(true);
    }
}
