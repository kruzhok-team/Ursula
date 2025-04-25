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
        public string _jsonFilePath;
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

        public void SetPathJson(string path)
        {
            _jsonFilePath = path;
        }

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

        public GameObjectAssetInfo GetItemInfo(string id)
        {
            return _infoMap[id];
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

            return _infoMap.ContainsKey(itemId);
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

        public void SetItem(string name, GameObjectTemplate template, string libId)
        {
            if (!CheckLoaded())
                return;

            if (string.IsNullOrEmpty(name) || template == null)
            {
                return;
            }

            var info = new GameObjectAssetInfo(name, Id, template);
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

        public async GDTask Load(string _jsonFilePath)
        {
            this._jsonFilePath = _jsonFilePath;

            IsDataLoaded = true;

            if (string.IsNullOrEmpty(_jsonFilePath) || !File.Exists(ProjectSettings.GlobalizePath(_jsonFilePath)))
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
            if (string.IsNullOrEmpty(_jsonFilePath))
                return;

            string pathDir = Path.GetDirectoryName(_jsonFilePath);
            if (!Directory.Exists(pathDir)) Directory.CreateDirectory(pathDir);


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
            string model3dFilePath = assetInfo.Template.Sources.Model3dFilePath;

            PackedScene prefab = null;

            if (assetInfo.ProviderId == GameObjectAssetsEmbeddedSource.LibId)
            {
                int numItem = -1;
                int.TryParse(model3dFilePath, out numItem);
                prefab = numItem >= 0 ? VoxLib.mapAssets.gameItemsGO[numItem] : null;

            }
            else if (assetInfo.ProviderId == GameObjectAssetsUserSource.LibId)
            {
                model3dFilePath = $"{assetInfo.GetAssetPath()}/{model3dFilePath}";

                if (assetInfo.Template.GameObjectClass == 1)
                    prefab = VoxLib.mapAssets.customObjectPrefab;
                else if (assetInfo.Template.GameObjectClass == 2)
                    prefab = VoxLib.mapAssets.customPlatformPrefab;
                else
                    prefab = VoxLib.mapAssets.customItemPrefab;   
                
            }

            if (prefab == null) return null;

            Node instance = prefab.Instantiate();
            Node3D node = instance as Node3D;

            if (assetInfo.ProviderId == GameObjectAssetsUserSource.LibId)
            {
                if (assetInfo.Template.GameObjectClass == 1)
                {
                    var custom = instance.GetNodeOrNull("CustomObjectScript");
                    if (custom == null) custom = instance.GetParent().FindChild("CustomObjectScript", true, true);
                    var co = custom as CustomObject;
                    if (co != null)
                    {
                        co.objPath = model3dFilePath;
                        co.InitModel();
                    }
                }
                else
                {
                    var customItem = instance.GetNodeOrNull("CustomItemScript");
                    if (customItem == null) customItem = instance.GetParent().FindChild("CustomItemScript", true, true);
                    var ci = customItem as CustomItem;
                    if (ci != null)
                    {
                        ci.objPath = model3dFilePath;
                        ci.InitModel();
                    }
                }
            }

            GameObjectAsset asset = new GameObjectAsset(assetInfo, null, node);

            return asset;

        }
    }
}
