using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{

    // Json asset collection partial implementation
    public class GameObjectAssetJsonCollection : IGameObjectAssetManager
    {
        private string _jsonFilePath;
        private Dictionary<string, IGameObjectAsset> _cachedAssetMap;
        private Dictionary<string, GameObjectAssetInfo> _infoMap;

        public GameObjectAssetJsonCollection(string id, string jsonFilePath)
        {
            Id = id;
            _jsonFilePath = jsonFilePath;
            _cachedAssetMap = new ();
            _infoMap = new ();
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

        public IReadOnlyCollection<IGameObjectAsset> GetAll()
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

        public bool TryGetItem(string itemId, out IGameObjectAsset asset)
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

        public void SetItem(string name, GameObjectAssetSources sources)
        {
            if (!CheckLoaded())
                return;

            if (string.IsNullOrEmpty(name) || sources == null)
            {
                return;
            }

            var info = new GameObjectAssetInfo(name, Id, sources);
            _infoMap[info.Id] = info;
        }

        public void RemoveItem(IGameObjectAsset asset)
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
            if (IsDataLoaded)
            {
                //TODO: Log a data already loaded warning here
                return;
            }

            IsDataLoaded = true;
            // TODO: Implement _sources deserialization from a json file by _jsonFilePath
            //throw new NotImplementedException();

            if (!File.Exists(ProjectSettings.GlobalizePath(_jsonFilePath)))
            {
                GD.Print($"Object file not found {_jsonFilePath}. Creating a new list.");
                _infoMap = new Dictionary<string, GameObjectAssetInfo>();
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            string json = File.ReadAllText(ProjectSettings.GlobalizePath(_jsonFilePath));
            _infoMap = JsonSerializer.Deserialize<Dictionary<string, GameObjectAssetInfo>>(json, options);
        }

        public async GDTask Save()
        {
            if (!CheckLoaded())
                return;

            // TODO: Implement sources serialization to a json file by _jsonFilePath
            //throw new NotImplementedException();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(_infoMap, options);
            File.WriteAllText(ProjectSettings.GlobalizePath(_jsonFilePath), json);
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

        private bool TryBuildAsset(string name, out IGameObjectAsset asset)
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

        private IGameObjectAsset BuildAssetImplementation(GameObjectAssetInfo assetInfo) 
        {
            //TODO: asset building implementation
            throw new NotImplementedException();

        }
    }
}
