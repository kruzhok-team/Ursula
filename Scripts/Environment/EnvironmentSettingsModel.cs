using Godot;
using System;
using Ursula.Core.DI;
using Ursula.Core.Model;

namespace Ursula.Environment.Settings
{
    public class EnvironmentSettingsData : ConfigFileData
    {
        public EnvironmentSettingsData() : base(VoxLib.SETTINGPATH)
        {
        }

        public int ShadowEnabled => (int)(GetConfigFile()?.GetValue("Settings", "ShadowEnabled", 0) ?? 0);
        public float Sensitivity => (float)(GetConfigFile()?.GetValue("Settings", "MouseSence", 0.5f) ?? 0.5f);
        public string LastMapDirectory => (string)(GetConfigFile()?.GetValue("Settings", "LastMapDirectory", "") ?? "");
        public string LastDirectory => (string)(GetConfigFile()?.GetValue("Settings", "LastDirectory", "") ?? "");

        public EnvironmentSettingsData SetShadowEnabled(int value)
        {
            GetConfigFile()?.SetValue("Settings", "ShadowEnabled", value);
            return this;
        }

        public EnvironmentSettingsData SetSensitivity(float value)
        {
            GetConfigFile()?.SetValue("Settings", "MouseSence", value);
            return this;
        }

        public EnvironmentSettingsData SetLastMapDirectory(string value)
        {
            GetConfigFile()?.SetValue("Settings", "LastMapDirectory", value);
            return this;
        }

        public EnvironmentSettingsData SetLastDirectory(string value)
        {
            GetConfigFile()?.SetValue("Settings", "LastDirectory", value);
            return this;
        }
    }


    //TODO: Create separate model classes for the following code, organized by feature logic.
    public class EnvironmentSettingsModel
    {
        private EnvironmentSettingsData _environmentSettingsData;

        public EnvironmentSettingsModel()
        {
            _environmentSettingsData = new EnvironmentSettingsData();
        }

        public float _defaultSensitivity = 0.5f;
        public int ShadowEnabled => _environmentSettingsData.ShadowEnabled;
        public float Sensitivity => _environmentSettingsData.Sensitivity;
        public string LastMapDirectory => _environmentSettingsData.LastMapDirectory;
        public string LastDirectory => _environmentSettingsData.LastDirectory;

        public event EventHandler ShadowEnabledChangeEvent;
        public event EventHandler SensitivityChangeEvent;
        public event EventHandler LastMapDirectoryChangeEvent;
        public event EventHandler LastDirectoryChangeEvent;

        public EnvironmentSettingsModel SetShadowEnabled(int value)
        {
            if (_environmentSettingsData.ShadowEnabled == value) return this;
            _environmentSettingsData.SetShadowEnabled(value);
            InvokeShadowEnabledChangeEvent();
            return this;
        }

        public void Save()
        {
            _environmentSettingsData.Save();
        }

        public EnvironmentSettingsModel SetSensitivity(float value)
        {
            if (_environmentSettingsData.Sensitivity == value) return this;
            _environmentSettingsData.SetSensitivity(value);
            InvokeSensitivityChangeEvent();
            return this;
        }

        public EnvironmentSettingsModel SetLastMapDirectory(string value)
        {
            if (_environmentSettingsData.LastMapDirectory == value) return this;
            _environmentSettingsData.SetLastMapDirectory(value);
            InvokeLastMapDirectoryChangeEvent();
            return this;
        }

        public EnvironmentSettingsModel SetLastDirectory(string value)
        {
            if (_environmentSettingsData.LastDirectory == value) return this;
            _environmentSettingsData.SetLastDirectory(value);
            InvokeLastDirectoryChangeEvent();
            return this;
        }

        private void InvokeShadowEnabledChangeEvent()
        {
            var handler = ShadowEnabledChangeEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
        private void InvokeSensitivityChangeEvent()
        {
            var handler = SensitivityChangeEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
        private void InvokeLastMapDirectoryChangeEvent()
        {
            var handler = LastMapDirectoryChangeEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
        private void InvokeLastDirectoryChangeEvent()
        {
            var handler = LastDirectoryChangeEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
