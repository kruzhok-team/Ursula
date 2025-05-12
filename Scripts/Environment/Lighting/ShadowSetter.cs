using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.Environment.Settings;

namespace Ursula.Environment.Lighting
{
    public partial class ShadowSetter : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider;

        private EnvironmentSettingsModel _model;

        void IInjectable.OnDependenciesInjected()
        {

        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            if (_model != null) 
                SetShadow();
        }

        private async GDTask SubscribeEvent()
        {
            _model = await _settingsModelProvider.GetAsync();
            _model.ShadowEnabledChangeEvent += EnvironmentSettings_ShadowEnabledChangeEventHandler;
        }

        private void SetShadow()
        {
            var scene = GetTree().CurrentScene;
            var directionalLight = FindDirectionalLight(scene);
            directionalLight.ShadowEnabled = _model.ShadowEnabled > 0;
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

        private void EnvironmentSettings_ShadowEnabledChangeEventHandler(object sender, EventArgs e)
        {
            SetShadow();
        }
    }
}

