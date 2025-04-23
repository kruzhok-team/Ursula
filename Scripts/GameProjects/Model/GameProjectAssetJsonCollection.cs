using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Talent.Graphs;
using Ursula.GameObjects.Model;

namespace Ursula.EmbeddedGames.Model
{
    public partial class GameProjectAssetJsonCollection : IGameProjectAssetManager
    {
        public const string ProjectJson = "project.json";

        private string _folderPath;

        private Dictionary<string, IGameProjectAsset> _cachedAssetMap;
        private Dictionary<string, GameProjectAssetInfo> _infoMap;

        public GameProjectAssetJsonCollection(string id, string folderPath)
        {
            Id = id;
            _folderPath = folderPath;
            if (id == GameProjectAssetsEmbeddedSource.LibId)
            {
                string path = Path.GetDirectoryName(OS.GetExecutablePath());
                path = ProjectSettings.GlobalizePath($"{path}{GameProjectAssetsEmbeddedSource.CollectionPath}");
                _folderPath = path;
            }
            _cachedAssetMap = new();
            _infoMap = new();
        }

        public string Id { get; private set; } = string.Empty;
        public bool IsDataLoaded { get; private set; } = false;

        public int ItemCount
        {
            get
            {
                if (!CheckLoaded())
                    return 0;
                return _infoMap.Count;
            }
        }

        public IReadOnlyCollection<GameProjectAssetInfo> GetAllInfo()
        {
            return _infoMap.Values;
        }

        public GameProjectAssetInfo GetItemInfo(string id)
        {
            return _infoMap[id];
        }

        public IReadOnlyCollection<IGameProjectAsset> GetAll()
        {
            if (!CheckLoaded())
                return null;

            foreach (var sourceName in _infoMap.Keys)
            {
                TryGetItem(sourceName, out _);
            }

            return _cachedAssetMap.Values;
        }

        public bool ContainsItem(string itemId)
        {
            if (!CheckLoaded())
                return false;

            if (string.IsNullOrEmpty(itemId))
                return false;

            return _cachedAssetMap.ContainsKey(itemId);
        }

        public bool TryGetItem(string itemId, out IGameProjectAsset asset)
        {
            if (!CheckLoaded())
            {
                asset = null;
                return false;
            }

            if (_cachedAssetMap.ContainsKey(itemId))
            {
                asset = _cachedAssetMap[itemId];
                return true;
            }
            return TryBuildAsset(itemId, out asset);
        }

        public GameProjectAssetInfo SetItem(string name, GameProjectTemplate template, string libId)
        {
            if (!CheckLoaded())
                return null;

            if (string.IsNullOrEmpty(name) || template == null)
            {
                return null;
            }

            var info = new GameProjectAssetInfo(name, Id, template);
            return _infoMap[info.Id] = info;
        }

        public void RemoveItem(IGameProjectAsset asset)
        {
            if (!CheckLoaded())
                return;

            if (asset == null)
                return;

            RemoveItem(asset.Info.Id);
        }

        public void RemoveItem(string itemId)
        {
            if (!CheckLoaded())
                return;

            if (string.IsNullOrEmpty(itemId))
                return;

            if (_cachedAssetMap.ContainsKey(itemId))
                _cachedAssetMap.Remove(itemId);
            _infoMap.Remove(itemId);
        }

        public async GDTask Load()
        {
            //if (IsDataLoaded)
            //{
            //    //TODO: Log a data already loaded warning here
            //    GD.PrintErr($"Error load _jsonFilePath={_folderPath}");
            //    return;
            //}

            IsDataLoaded = true;

            _infoMap = new Dictionary<string, GameProjectAssetInfo>();

            if (!Directory.Exists(ProjectSettings.GlobalizePath(_folderPath)))
            {
                GD.Print($"Folder not found {_folderPath}.");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            List<string> folders = GetFoldersInDirectory(ProjectSettings.GlobalizePath(_folderPath));

            for (int i = 0; i < folders.Count; i++)
            {
                string folder = folders[i];
                string path = $"{folder}/{ProjectJson}";

                if (!File.Exists(path)) continue;

                string json = File.ReadAllText(path);
                GameProjectAssetInfo projectInfo = JsonSerializer.Deserialize<GameProjectAssetInfo>(json, options);

                projectInfo.Name = Path.GetFileName(folder);
                projectInfo.ProviderId = Id;

                string id = $"{Id}.{projectInfo.Name}";

                if (!_infoMap.ContainsKey(id)) _infoMap.Add(id, projectInfo);
            }


        }

        public bool SaveItem(string itemId, string libId)
        {
            if (!CheckLoaded())
                return false;

            if (string.IsNullOrEmpty(itemId)) return false;

            GameProjectAssetInfo info = GetItemInfo(itemId);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(info, options);

            string jsonFilePath = $"{_folderPath}{info.Template.Folder}/{ProjectJson}";
            File.WriteAllText(ProjectSettings.GlobalizePath(jsonFilePath), json);

            return true;
        }

        private bool TryBuildAsset(string name, out IGameProjectAsset asset)
        {
            if (!_infoMap.TryGetValue(name, out var assetInfo))
            {
                //TODO: Some error or warning log
                asset = null;
                return false;
            }

            asset = BuildAssetImplementation(assetInfo);
            return true;
        }

        private IGameProjectAsset BuildAssetImplementation(GameProjectAssetInfo assetInfo)
        {
            GameProjectAsset asset = new GameProjectAsset(assetInfo);

            return asset;
        }


        private bool CheckLoaded()
        {
            if (!IsDataLoaded)
            {
                //TODO: Log a not loaded error or throw exception here
                return false;
            }
            return true;
        }

        private List<string> GetFoldersInDirectory(string directoryPath)
        {
            List<string> folders = new List<string>();

            // Создаем объект DirectoryInfo
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

            // Получаем все поддиректории
            DirectoryInfo[] subDirectories = dirInfo.GetDirectories();

            foreach (var subDir in subDirectories)
            {
                folders.Add(subDir.FullName);
            }

            return folders;
        }
    }
}
