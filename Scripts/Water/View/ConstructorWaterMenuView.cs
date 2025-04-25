using Core.UI.Constructor;
using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.StartupMenu.Model;
using Ursula.Water.Model;

public partial class ConstructorWaterMenuView : ConstructorWaterMenuViewModel, IInjectable
{
    [Export]
    private HSlider HSliderWaterLevel;

    [Export]
    private OptionButton TypeWaterOption;

    [Export]
    private CheckBox CheckButtonWaterStatic;

    [Export]
    private Button ButtonCreateWater;

    [Inject]
    private ISingletonProvider<WaterModel> _waterModelProvider;

    [Inject]
    private ISingletonProvider<StartupMenuCreateGameViewModel> _startupMenuCreateGameViewModelProvider;

    private WaterModel _waterModel { get; set; }
    private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }

    void IInjectable.OnDependenciesInjected()
    {

    }

    public override void _Ready()
    {
        base._Ready();
        _ = SubscribeEvent();

        Control control = this as Control;
        if (control != null) control.VisibilityChanged += Control_VisibilityChangedEvent;
    }

    private async GDTask SubscribeEvent()
    {
        _waterModel = await _waterModelProvider.GetAsync();
        _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();

        ButtonCreateWater.ButtonDown += ButtonCreateWater_ButtonDownEvent;
    }

    private void Control_VisibilityChangedEvent()
    {
        SetWaterData();
    }

    private void SetWaterData()
    {
        float offset = 1 - _waterModel._WaterData.WaterOffset;
        HSliderWaterLevel.Value = MapRange(offset, 0, 1, 0, 100);
        CheckButtonWaterStatic.ButtonPressed = _waterModel._WaterData.IsStaticWater;
        TypeWaterOption.Selected = _waterModel._WaterData.TypeWaterID;
    }

    private void ButtonCreateWater_ButtonDownEvent()
    {
        _startupMenuCreateGameViewModel.SetWaterOffset((float)HSliderWaterLevel.Value);
        _startupMenuCreateGameViewModel.SetStaticWater(!CheckButtonWaterStatic.ButtonPressed);
        _startupMenuCreateGameViewModel.SetTypeWaterID(TypeWaterOption.Selected);

        _waterModel.SetStaticWater(_startupMenuCreateGameViewModel._CreateGameSourceData.IsStaticWater);
        _waterModel.SetWaterOffset(_startupMenuCreateGameViewModel._CreateGameSourceData.WaterOffset);
        _waterModel.SetTypeWaterID(_startupMenuCreateGameViewModel._CreateGameSourceData.TypeWaterID + 1);
        _waterModel.StartGenerateWater();
    }

    private float MapRange(float value, float min1, float max1, float min2, float max2)
    {
        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }
}
