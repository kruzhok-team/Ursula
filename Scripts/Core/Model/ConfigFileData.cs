using Godot;

namespace Ursula.Core.Model
{
    public class ConfigFileData
    {
        private string _filePath;
        private ConfigFile _cachedConfigFile; // Don't use it directly!! Use GetConfigFile instead!

        public Error LastFileError { get; private set; } = Godot.Error.Ok;

        public ConfigFileData(string filePath)
        {
            _filePath = filePath;
        }

        public void Save()
        {
            if (LastFileError != Godot.Error.Ok)
            {
                GD.PrintErr($"{GetType().Name} Can't update the settings file '{_filePath}' due to a previously existing error: \r\n {LastFileError.ToString()}");
                return;
            }

            GetConfigFile().Save(_filePath);
        }

        protected ConfigFile GetConfigFile()
        {
            if (_cachedConfigFile == null)
            {
                _cachedConfigFile = new ConfigFile();
                LastFileError = _cachedConfigFile.Load(_filePath);
                string path = ProjectSettings.GlobalizePath(_filePath);

                if (LastFileError != Godot.Error.Ok)
                {
                    GD.Print($"{GetType().Name} Settings file '{_filePath}' loading error: \r\n{LastFileError.ToString()}");
                    _cachedConfigFile.Save(path);
                }
                else
                {
                    //_cachedConfigFile.Save(ProjectSettings.GlobalizePath(_filePath));
                }
            }
            return _cachedConfigFile;
        }
    }
}
