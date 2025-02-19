using Godot;
using Ursula.Core.DI;
using Ursula.Core.Model;

namespace Ursula.Environment.Settings
{
    //TODO: Create separate model classes for the following code, organized by feature logic.
    public class EnvironmentSettingsModel : ConfigFileModel
    {
        public EnvironmentSettingsModel() : base(VoxLib.SETTINGPATH)
        {
        }

        public int ShadowEnabled => (int)(GetConfigFile()?.GetValue("Settings", "ShadowEnabled", 0) ?? 0);
        public float Sensitivity => (float)(GetConfigFile()?.GetValue("Settings", "MouseSence", 0) ?? 0);
        public string LastMapDirectory => (string)(GetConfigFile()?.GetValue("Settings", "LastMapDirectory", "") ?? "");
        public string LastDirectory => (string)(GetConfigFile()?.GetValue("Settings", "LastDirectory", "") ?? "");

        public EnvironmentSettingsModel SetShadowEnabled(int value)
        {
            GetConfigFile()?.SetValue("Settings", "ShadowEnabled", value);
            return this;
        }

        public EnvironmentSettingsModel SetSensitivity(float value)
        {
            GetConfigFile()?.SetValue("Settings", "MouseSence", value);
            return this;
        }

        public EnvironmentSettingsModel SetLastMapDirectory(string value)
        {
            GetConfigFile()?.SetValue("Settings", "LastMapDirectory", value);
            return this;
        }

        public EnvironmentSettingsModel SetLastDirectory(string value)
        {
            GetConfigFile()?.SetValue("Settings", "LastDirectory", value);
            return this;
        }

        public static bool TryGetSettingsModel(ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider, out EnvironmentSettingsModel model, bool errorIfNotExist = false)
        {
            model = null;

            if (!(_settingsModelProvider?.TryGet(out model) ?? false))
            {
                if (errorIfNotExist)
                    GD.PrintErr($"{typeof(MapManager).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
            }
            return model != null;
        }
    }
}
