using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{

    public class GameObjectLibraryManager : IGameObjectLibraryManager, IInjectable
    {
        public const string LibId = "CommonGameObjectLibrary";
        public const string JsonDataPath = "";

        [Inject]
        private ISingletonProvider<GameObjectAssetsUserSource> _userLibraryProvider;
        [Inject]
        private ISingletonProvider<GameObjectAssetsEmbeddedSource> _embeddedLibraryProvider;

        private GameObjectAssetsUserSource _userLib;
        private GameObjectAssetsEmbeddedSource _embeddedLib;
        private Dictionary<string, CommonGameObjectLibraryItem> _commonAssetMap;
        private HashSet<string> _exclusions;

        void IInjectable.OnDependenciesInjected()
        {
        }

        //public GameObjectLibraryManager(string folderPath)
        //{
        //}

        public bool IsDataLoaded { get; private set; } = false;

        public string Id => LibId;
        public int ItemCount => _commonAssetMap?.Count ?? 0;

        public IGameObjectAssetProvider UserCollection => _userLib;
        public IGameObjectAssetProvider EmbeddedCollection => _embeddedLib;

        public bool IsItemExcluded(string itemId)
        {
            if (!CheckLoaded())
                return false;
            return _exclusions.Contains(itemId);
        }

        public IReadOnlyCollection<IGameObjectAsset> GetAll()
        {
            if (!CheckLoaded())
                return null;

            var result = new List<IGameObjectAsset>();

            _userLib.GetAll();

            return result;
        }

        public bool ContainsItem(string itemId)
        {
            if (!CheckLoaded())
                return false;

            return _commonAssetMap.ContainsKey(itemId);
        }

        public void RemoveItem(string itemId)
        {
            if (!CheckLoaded())
                return;
            if (string.IsNullOrEmpty(itemId))
                return;
            if (!_commonAssetMap.ContainsKey(itemId))
                return;

            if (!TryGetItemProvider(itemId, out var provider))
                return;
            if (provider == _userLib)
                _userLib.RemoveItem(itemId);
            else if (provider == _embeddedLib)
                _exclusions.Remove(itemId);

            _commonAssetMap.Remove(itemId);
        }

        public void RemoveItem(IGameObjectAsset asset)
        {
            if (asset == null)
                return;
            RemoveItem(asset.Info.Id);
        }

        public void SetItem(string name, GameObjectAssetSources sources)
        {
            if (!CheckLoaded())
                return;
            if (string.IsNullOrEmpty(name) || sources == null)
                return;
            _userLib.SetItem(name, sources);

            var entry = new CommonGameObjectLibraryItem(name, _userLib.Id);
            _commonAssetMap[entry.Id] = entry;
        }

        public bool TryGetItem(string itemId, out IGameObjectAsset asset)
        {
            asset = null;

            if (!CheckLoaded())
                return false;
            if (!_commonAssetMap.ContainsKey(itemId))
                return false;

            if (!TryGetItemProvider(itemId, out var provider))
                return false;            
            return provider.TryGetItem(itemId, out asset);
        }

        public async GDTask Load()
        {
            if (IsDataLoaded)
            {
                //TODO: Log an error or warning due to data already loaded
                return;
            }

            _userLib = await _userLibraryProvider.GetAsync();
            _embeddedLib = await _embeddedLibraryProvider.GetAsync();

            if (_embeddedLib == null || _userLib == null)
            {
                GD.PrintErr("Not all game object liraries installed in DI!");
                return;
            }

            if (!_userLib.IsDataLoaded)
                await _userLib.Load();
            if (!_embeddedLib.IsDataLoaded)
                await _embeddedLib.Load();

            _commonAssetMap = LoadCommonAssetsInfo();
            _exclusions = LoadExclusions();
            IsDataLoaded = true;

            SyncEmbeddedAssets();
        }

        public async GDTask Save()
        {
            if (!CheckLoaded())
                return;

            await SaveCommonAssetsInfo();
            await SaveExclusions();
        }

        private bool TryGetItemProvider(string itemId, out IGameObjectAssetManager provider)
        {
            provider = null;

            if (!string.IsNullOrEmpty(itemId))
            {
                if (itemId.Contains(GameObjectAssetsUserSource.LibId))
                    provider = _userLib;
                else if (itemId.Contains(GameObjectAssetsEmbeddedSource.LibId))
                    provider = _embeddedLib;
            }

            if (provider == null)
            {
                //TODO: Log en error here
                return false;
            }
            return true;
        }

        private Dictionary<string, CommonGameObjectLibraryItem> LoadCommonAssetsInfo()
        {
            //var commonList = LoadCommonAssetList(JsonDataPath);
            //return commonList != null ? commonList.ToDictionary(i => i.Id, j => j) : null;

            throw new NotImplementedException();
        }

        private async GDTask SaveCommonAssetsInfo()
        {
            throw new NotImplementedException();
        }

        private HashSet<string> LoadExclusions()
        {
            //TODO: Implement exclussions list loading here
            throw new NotImplementedException();
        }

        private async GDTask SaveExclusions()
        {
            throw new NotImplementedException();
        }

        private async GDTask SyncEmbeddedAssets()
        {
            if (!CheckLoaded())
                return;

            var countBefore = _commonAssetMap.Count;

            foreach (var asset in _embeddedLib.GetAll())
            {
                if (!_commonAssetMap.ContainsKey(asset.Info.Id))
                {
                    _commonAssetMap[asset.Info.Id] = asset.Info.AsCommonItem();
                }
            }

            if (countBefore != _commonAssetMap.Count)
                await SaveCommonAssetsInfo();
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
    }
}
