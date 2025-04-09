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
    }

    private async GDTask SubscribeEvent()
    {
        _waterModel = await _waterModelProvider.GetAsync();
        _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();

        ButtonCreateWater.ButtonDown += ButtonCreateWater_ButtonDownEvent;
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
}
