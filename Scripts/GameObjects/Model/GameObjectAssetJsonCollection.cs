using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Ursula.Core.DI;
using static Godot.RenderingDevice;

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

        public IReadOnlyCollection<GameObjectAssetInfo> GetAllInfo()
        {
            return _infoMap.Values;
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

        public void SetItem(string name, GameObjectAssetSources sources, string libId)
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

            //object model3D = ModelLoader.LoadModelByPath(assetInfo.Sources.Model3dFilePath);
            //GameObjectAsset asset = new GameObjectAsset(assetInfo, null, model3D);


            PackedScene prefab = null;

            if (assetInfo.ProviderId == GameObjectAssetsEmbeddedSource.LibId)
            {
                int numItem = -1;
                int.TryParse(assetInfo.Sources.Model3dFilePath, out numItem);
                prefab = numItem >= 0 ? VoxLib.mapAssets.gameItemsGO[numItem] : null;

            }
            else if (assetInfo.ProviderId == GameObjectAssetsUserSource.LibId)
            {
                prefab = VoxLib.mapAssets.customItemPrefab;
                //object model3D = ModelLoader.LoadModelByPath(assetInfo.Sources.Model3dFilePath);               
            }

            if (prefab == null) return null;

            Node instance = prefab.Instantiate();
            Node3D node = instance as Node3D;

            if (assetInfo.ProviderId == GameObjectAssetsUserSource.LibId)
            {
                var customItem = instance.GetNodeOrNull("CustomItemScript");
                if (customItem == null) customItem = instance.GetParent().FindChild("CustomItemScript", true, true);
                var ci = customItem as CustomItem;
                if (ci != null)
                {
                    ci.objPath = assetInfo.Sources.Model3dFilePath;
                    ci.InitModel();
                }
            }

            GameObjectAsset asset = new GameObjectAsset(assetInfo, null, node);

            return asset;

        }
    }
}
