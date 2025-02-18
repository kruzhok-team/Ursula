using System;
using System.Collections.Generic;
using UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model;

namespace Ursula.GameObjects
{

    // Json asset collection partial implementation
    public class GameObjectAssetJsonCollection : IGameObjectAssetManager
    {
        private string _jsonFilePath;
        private Dictionary<string, IGameObjectAsset> _cachedAssets;
        private Dictionary<string, GameObjectAssetSources> _sources;

        public GameObjectAssetJsonCollection(string id, string jsonFilePath)
        {
            Id = id;
            _jsonFilePath = jsonFilePath;
            _cachedAssets = new ();
            _sources = new ();
        }

        public string Id { get; private set; } = string.Empty;
        public bool IsDataLoaded { get; private set; } = false;

        public int ItemCount
        {
            get
            {
                if (!CheckLoaded())
                    return 0;
                return _sources.Count;
            }
        }

        public IReadOnlyCollection<IGameObjectAsset> GetAll()
        {
            if (!CheckLoaded())
                return null;

            foreach (var sourceName in _sources.Keys)
            {
                TryGetItem(sourceName, out _);
            }

            return _cachedAssets.Values;
        }

        public bool ContainsItem(string name)
        {
            if (!CheckLoaded())
                return false;

            if (string.IsNullOrEmpty(name))
                return false;

            return _cachedAssets.ContainsKey(name);
        }

        public bool TryGetItem(string name, out IGameObjectAsset asset)
        {
            if (!CheckLoaded())
            {
                asset = null;
                return false;
            }

            if (_cachedAssets.ContainsKey(name))
            {
                asset = _cachedAssets[name];
                return true;
            }
            return TryBuildAsset(name, out asset);
        }

        public void SetItem(string name, GameObjectAssetSources sources)
        {
            if (!CheckLoaded())
                return;

            if (string.IsNullOrEmpty(name) || sources == null)
            {
                return;
            }

            _sources.Add(name, sources);
        }

        public void RemoveItem(IGameObjectAsset asset)
        {
            if (!CheckLoaded())
                return;

            if (asset == null)
                return;

            RemoveItem(asset.Info.Name);
        }

        public void RemoveItem(string name)
        {
            if (!CheckLoaded())
                return;

            if (string.IsNullOrEmpty(name))
                return;

            if (_cachedAssets.ContainsKey(name))
                _cachedAssets.Remove(name);
            _sources.Remove(name);
        }

        public void Load()
        {
            if (IsDataLoaded)
            {
                //TODO: Log a data already loaded warning here
                return;
            }

            IsDataLoaded = true;    
            // TODO: Implement _sources deserialization from a json file by _jsonFilePath
            throw new NotImplementedException();
        }

        public void Save()
        {
            if (!CheckLoaded())
                return;

            // TODO: Implement _sources serialization to a json file by _jsonFilePath
            throw new NotImplementedException();
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
            if (!_sources.TryGetValue(name, out var assetSources))
            { 
                //TODO: Some error or warning log
                asset = null;
                return false;
            }

            asset = BuildAssetImplementation(name, assetSources);
            return true;
        }

        private IGameObjectAsset BuildAssetImplementation(string name, GameObjectAssetSources sources) 
        {
            //TODO: asset building implementation
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<IGameObjectAsset> GetFiltered(IEnumerable<string> excludeNames)
        {
            if (!CheckLoaded())
                return null;
            return null; //TODO: Implement data filtering by asset names here
        }
    }
}
