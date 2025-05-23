﻿using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Ursula.Core.DI;
using Ursula.MapManagers.Model;
using VoxLibExample;

namespace Ursula.GameObjects.Model
{

    public class GameObjectLibraryManager : IGameObjectLibraryManager, IInjectable
    {
        public const string LibId = "CommonGameObjectLibrary";

        public event EventHandler GameObjectLibraryLoadLibraryEvent;

        private ISingletonProvider<GameObjectAssetsUserSource> _userLibraryProvider;
        private ISingletonProvider<GameObjectAssetsEmbeddedSource> _embeddedLibraryProvider;

        private GameObjectAssetsUserSource _userLib;
        private GameObjectAssetsEmbeddedSource _embeddedLib;
        private Dictionary<string, CommonGameObjectLibraryItem> _commonAssetMap;
        private HashSet<string> _exclusions;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectLibraryManager(
            ISingletonProvider<GameObjectAssetsUserSource> userLibraryProvider, 
            ISingletonProvider<GameObjectAssetsEmbeddedSource> embeddedLibraryProvider)
        {
            _userLibraryProvider = userLibraryProvider;
            _embeddedLibraryProvider = embeddedLibraryProvider;
        }

        public bool IsDataLoaded { get; private set; } = false;

        public string Id => LibId;
        public int ItemCount => _commonAssetMap?.Count ?? 0;

        public IGameObjectAssetProvider UserCollection => _userLib;
        public IGameObjectAssetProvider EmbeddedCollection => _embeddedLib;

        public GameObjectLibraryManager LoadLibrary()
        {
            InvokeGameObjectLibraryLoadLibraryEvent();
            return this;
        }

        public bool IsItemExcluded(string itemId)
        {
            if (!CheckLoaded())
                return false;
            return _exclusions.Contains(itemId);
        }

        public IReadOnlyCollection<GameObjectAssetInfo> GetAllInfo()
        {
            var mergedList = new List<GameObjectAssetInfo>(_userLib.GetAllInfo());
            mergedList.AddRange(_embeddedLib.GetAllInfo());

            return mergedList;
        }

        public IReadOnlyCollection<GameObjectAssetInfo> GetInfoOnGroup(string nameGroup, string LibId = null)
        {
            var mergedList = new List<GameObjectAssetInfo>();
            if ((LibId == null || LibId == GameObjectAssetsUserSource.LibId) && _userLib != null) mergedList.AddRange(_userLib.GetAllInfo());
            if ((LibId == null || LibId == GameObjectAssetsEmbeddedSource.LibId && _embeddedLib != null)) mergedList.AddRange(_embeddedLib.GetAllInfo());

            var collectionList = new List<GameObjectAssetInfo>();

            if (string.IsNullOrEmpty(nameGroup))
            {
                collectionList = mergedList;
            }
            else
            {
                for (int i = 0; i < mergedList.Count; i++)
                {
                    if (mergedList[i].Template.GameObjectGroup == nameGroup) collectionList.Add(mergedList[i]);
                }
            }

            return collectionList;
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

            if (!TryGetItemProvider(itemId, out var provider))
                return false;

            if (provider == _userLib)
                return _userLib.ContainsItem(itemId);
            else if (provider == _embeddedLib)
                return _embeddedLib.ContainsItem(itemId);

            return _commonAssetMap.ContainsKey(itemId);
        }

        public GameObjectAssetInfo GetItemInfo(string itemId)
        {
            //if (!ContainsItem(itemId))
            //    return null;

            if (!TryGetItemProvider(itemId, out var provider))
                return null;

            if (provider == _userLib)
                return _userLib.GetItemInfo(itemId);
            else if (provider == _embeddedLib)
                return _embeddedLib.GetItemInfo(itemId);

            return null;
        }

        public void RemoveItem(string itemId)
        {
            if (!CheckLoaded())
                return;
            if (string.IsNullOrEmpty(itemId))
                return;
            //if (!_commonAssetMap.ContainsKey(itemId))
            //    return;

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

        public void SetItem(string name, GameObjectTemplate template, string libId)
        {
            if (string.IsNullOrEmpty(name) || template == null)
                return;

            if (libId == GameObjectAssetsUserSource.LibId)
                _userLib.SetItem(name, template, libId);
            else if (libId == GameObjectAssetsEmbeddedSource.LibId)
                _embeddedLib.SetItem(name, template, libId);

            var entry = new CommonGameObjectLibraryItem(name, _userLib.Id);
            _commonAssetMap[entry.Id] = entry;

            //VoxLib.ShowMessage($"Модель {name} добавлена в библиотеку");
        }

        public void EditItem(IGameObjectAsset asset)
        {

        }

        public bool TryGetItem(string itemId, out IGameObjectAsset asset)
        {
            asset = null;

            if (!CheckLoaded())
                return false;
            //if (!_commonAssetMap.ContainsKey(itemId))
            //    return false;

            if (!TryGetItemProvider(itemId, out var provider))
                return false;            
            return provider.TryGetItem(itemId, out asset);
        }

        public async GDTask Load(string _projectPath)
        {
            _userLib = await _userLibraryProvider.GetAsync();
            _embeddedLib = await _embeddedLibraryProvider.GetAsync();

            if (string.IsNullOrEmpty(_projectPath))
            {
                await _userLib.Load(null);
                await _embeddedLib.Load(null);
                return;
            }

            if (_embeddedLib == null || _userLib == null)
            {
                GD.PrintErr("Not all game object libraries installed in DI!");
                return;
            }

            string pathUserJson = $"{_projectPath}/{GameObjectAssetsUserSource.JsonDataPath}";
            string pathEmbeddedJson = $"{_projectPath}/{GameObjectAssetsEmbeddedSource.JsonDataPath}";

            _commonAssetMap = LoadCommonAssetsInfo();

            await CheckEmbeddedAssets(pathEmbeddedJson);

            await GDTask.Delay(100);

            await _userLib.Load(pathUserJson);

            await _embeddedLib.Load(pathEmbeddedJson);

            _exclusions = LoadExclusions();
            IsDataLoaded = true;

            _ = SyncEmbeddedAssets();


        }

        public async GDTask Save()
        {
            if (!CheckLoaded())
                return;

            await SaveCommonAssetsInfo();
            await SaveExclusions();
        }

        public string GetAssetPath(string itemId)
        {
            GameObjectAssetInfo assetInfo = GetItemInfo(itemId);
            if (assetInfo == null) return null;
            return assetInfo.GetAssetPath();
        }

        public string GetGraphXmlPath(string itemId)
        {
            GameObjectAssetInfo assetInfo = GetItemInfo(itemId);
            if (assetInfo == null) return null;
            return assetInfo.GetGraphXmlPath();
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

            //throw new NotImplementedException();
            // _userLib.GetAllInfo();
            //return new Dictionary<string, CommonGameObjectLibraryItem>();

            Dictionary<string, CommonGameObjectLibraryItem> commonList = new Dictionary<string, CommonGameObjectLibraryItem>();
            return commonList;
        }

        private async GDTask SaveCommonAssetsInfo()
        {
            await _embeddedLib.Save();
            await _userLib.Save();
        }

        private HashSet<string> LoadExclusions()
        {
            //TODO: Implement exclussions list loading here
            //throw new NotImplementedException();
            return new HashSet<string>();
        }

        private async GDTask SaveExclusions()
        {

            //throw new NotImplementedException();

        }

        private async GDTask CheckEmbeddedAssets(string path)
        {
            _embeddedLib._jsonFilePath = path;

            bool isExist = File.Exists(path);
            bool isCreateNew = false;

            var mapAssets = ResourceLoader.Load<MapAssets>(MapManagerData.MapManagerAssetPath);

            if (isExist)
            {
                await _embeddedLib.Load(path);
                if (_embeddedLib.ItemCount != mapAssets.gameItemsGO.Length) isCreateNew = true;
            }

            if (!isExist || isCreateNew)
            {
                for (int i = 0; i < mapAssets.gameItemsGO.Length; i++)
                {
                    string modelName = Path.GetFileNameWithoutExtension( mapAssets.gameItemsGO[i].ResourcePath );
                    string gameObjectGroup = i < mapAssets.inventarGameObjectGroups.Length ? mapAssets.inventarGameObjectGroups[i] : "";
                    string gameObjectSample = gameObjectGroup;

                    GameObjectAssetSources sources = new GameObjectAssetSources
                        (
                            null,
                            i.ToString(),
                            null,
                            null
                        );

                    GameObjectTemplate template = new GameObjectTemplate
                        (
                            i.ToString(),
                            gameObjectGroup,
                            0,
                            gameObjectSample,
                            sources,
                            "",
                            i.ToString()
                        );

                    SetItem(modelName, template, GameObjectAssetsEmbeddedSource.LibId);
                }

                await _embeddedLib.Save();
            }
        }

        private async GDTask SyncEmbeddedAssets()
        {
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

        private void InvokeGameObjectLibraryLoadLibraryEvent()
        {
            var handler = GameObjectLibraryLoadLibraryEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
