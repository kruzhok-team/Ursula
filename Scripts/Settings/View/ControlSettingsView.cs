using Core.UI.Constructor;
using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.Environment.Settings;
using Ursula.Settings.Model;

namespace Ursula.Settings.View
{
    public partial class ControlSettingsView : ControlSettingsViewModel
    {
        [Export]
        public Slider SliderMouseSence;

        [Export]
        public OptionButton OptionButtonShadow;

        [Export]
        public Button ButtonClose;

        [Inject]
        private ISingletonProvider<ControlSettingsViewModel> _controlSettingsViewModelProvider;

        [Inject]
        private ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider;

        private ControlSettingsViewModel _controlSettingsViewModel;

        public override void _Ready()
        {
            base._Ready();

            ApplySettingsFromConfig();

            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _controlSettingsViewModel = await _controlSettingsViewModelProvider.GetAsync();
            _controlSettingsViewModel.ShowView_EventHandler += (sender, args) => { ShowView(); };

            ButtonClose.ButtonDown += ButtonClose_ButtonDownEvent;
        }

        public void SetSensitivity(float sence)
        {
            if (TryGetSettingsModel(out var settingsModel))
                settingsModel.SetSensitivity(sence / 100).Save();
        }

        public void SetShadowEnabled(int value)
        {
            if (TryGetSettingsModel(out var settingsModel))
                settingsModel.SetShadowEnabled(value).Save();
        }

        public void SetShowView(bool value)
        {
            _controlSettingsViewModel?.SetShowVisibleView(value);
        }

        private async void ShowView()
        {
            Visible = _controlSettingsViewModel.Visible;
        }

        private async void ApplySettingsFromConfig()
        {
            var model = _settingsModelProvider != null ? await _settingsModelProvider.GetAsync() : null;

            if (model == null)
            {
                GD.PrintErr($"{typeof(HUD).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
                return;
            }

            SliderMouseSence.Value = model.Sensitivity * 100;
            OptionButtonShadow.Selected = model.ShadowEnabled;
        }

        private bool TryGetSettingsModel(out EnvironmentSettingsModel model, bool errorIfNotExist = false)
        {
            model = null;

            if (!(_settingsModelProvider?.TryGet(out model) ?? false))
            {
                if (errorIfNotExist)
                    GD.PrintErr($"{typeof(ControlSettingsView).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
            }
            return model != null;
        }

        private void ButtonClose_ButtonDownEvent()
        {
            SetShowView(false);
        }
    }
}
