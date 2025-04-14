using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;



namespace Ursula.GameProjects.Model
{
    public partial class GameProjectLibraryManager : IGameProjectLibraryManager, IInjectable
    {
        public const string LibId = "CommonGameProjectLibrary";
        public const string MapName = "MapData.txt";

        public GameProjectAssetInfo currentProjectInfo { get; private set; }

        private ISingletonProvider<GameProjectAssetsUserSource> _userLibraryProvider;
        private ISingletonProvider<GameProjectAssetsEmbeddedSource> _embeddedLibraryProvider;

        private GameProjectAssetsUserSource _userLib;
        private GameProjectAssetsEmbeddedSource _embeddedLib;

        private Dictionary<string, CommonGameProjectLibraryItem> _commonAssetMap;
        private HashSet<string> _exclusions;


        public EventHandler GameProjectSetLoadProject_Event;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameProjectLibraryManager(
            ISingletonProvider<GameProjectAssetsUserSource> userLibraryProvider,
            ISingletonProvider<GameProjectAssetsEmbeddedSource> embeddedLibraryProvider)
        {
            _userLibraryProvider = userLibraryProvider;
            _embeddedLibraryProvider = embeddedLibraryProvider;
        }

        public bool IsDataLoaded { get; private set; } = false;

        public string Id => LibId;
        public int ItemCount => _commonAssetMap?.Count ?? 0;

        public IGameProjectAssetProvider UserCollection => _userLib;
        public IGameProjectAssetProvider EmbeddedCollection => _embeddedLib;

        public bool IsItemExcluded(string itemId)
        {
            if (!CheckLoaded())
                return false;
            return _exclusions.Contains(itemId);
        }

        public IReadOnlyCollection<GameProjectAssetInfo> GetAllInfo()
        {
            var mergedList = new List<GameProjectAssetInfo>();
            if (_userLib != null) mergedList.AddRange(_userLib.GetAllInfo());
            if (_embeddedLib != null) mergedList.AddRange(_embeddedLib.GetAllInfo());

            return mergedList;
        }

        public IReadOnlyCollection<IGameProjectAsset> GetAll()
        {
            if (!CheckLoaded())
                return null;

            var result = new List<IGameProjectAsset>();

            _userLib.GetAll();

            return result;
        }

        public bool ContainsItem(string itemId)
        {
            if (!CheckLoaded())
                return false;

            return _commonAssetMap.ContainsKey(itemId);
        }

        public GameProjectAssetInfo GetItemInfo(string itemId)
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

        public void RemoveItem(IGameProjectAsset asset)
        {
            if (asset == null)
                return;
            RemoveItem(asset.Info.Id);
        }

        public GameProjectAssetInfo SetItem(string name, GameProjectTemplate template, string libId)
        {
            if (!CheckLoaded())
                return null;
            if (string.IsNullOrEmpty(name) || template == null)
                return null;

            GameProjectAssetInfo info = null;
            if (libId == GameProjectAssetsUserSource.LibId)
                info = _userLib.SetItem(name, template, libId);
            else if (libId == GameProjectAssetsEmbeddedSource.LibId)
                info = _embeddedLib.SetItem(name, template, libId);

            var entry = new CommonGameProjectLibraryItem(name, _userLib.Id);
            _commonAssetMap[entry.Id] = entry;

            return info;
            //VoxLib.ShowMessage($"Модель {name} добавлена в библиотеку");
        }

        public bool TryGetItem(string itemId, out IGameProjectAsset asset)
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

        public async GDTask Load()
        {
            //if (IsDataLoaded)
            //{
            //    //TODO: Log an error or warning due to data already loaded
            //    return;
            //}

            _userLib = await _userLibraryProvider.GetAsync();
            _embeddedLib = await _embeddedLibraryProvider.GetAsync();

            if (_embeddedLib == null || _userLib == null)
            {
                GD.PrintErr("Not all game object libraries installed in DI!");
                return;
            }

            //if (!_userLib.IsDataLoaded)
                await _userLib.Load();
            //if (!_embeddedLib.IsDataLoaded)
                await _embeddedLib.Load();

            _commonAssetMap = LoadCommonAssetsInfo();

            IsDataLoaded = true;
        }

        private Dictionary<string, CommonGameProjectLibraryItem> LoadCommonAssetsInfo()
        {
            Dictionary<string, CommonGameProjectLibraryItem> commonList = new Dictionary<string, CommonGameProjectLibraryItem>();
            return commonList;
        }

        private bool TryGetItemProvider(string itemId, out IGameProjectAssetManager provider)
        {
            provider = null;

            if (!string.IsNullOrEmpty(itemId))
            {
                if (itemId.Contains(GameProjectAssetsUserSource.LibId))
                    provider = _userLib;
                else if (itemId.Contains(GameProjectAssetsEmbeddedSource.LibId))
                    provider = _embeddedLib;
            }

            if (provider == null)
            {
                return false;
            }
            return true;
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

        public bool SaveItem(string itemId, string libId)
        {
            IGameProjectAssetManager provider = null;

            if (itemId.Contains(GameProjectAssetsUserSource.LibId))
                provider = _userLib;
            else if (itemId.Contains(GameProjectAssetsEmbeddedSource.LibId))
                provider = _embeddedLib;

            if (provider == null)
            {
                return false;
            }

            provider.SaveItem(itemId, libId);
            return true;
        }

        public void SetCurrentProjectInfo(GameProjectAssetInfo info)
        {
            currentProjectInfo = info;
        }

        public void SetLoadProject(GameProjectAssetInfo info)
        {
            SetCurrentProjectInfo(info);
            GameObjectAssetsUserSource.ProjectPath = info.GetProjectPath();
            GameObjectAssetsEmbeddedSource.ProjectFolderPath = info.GetProjectPath();

            InvokeSetLoadProjectEvent();
        }

        private void InvokeSetLoadProjectEvent()
        {
            var handler = GameProjectSetLoadProject_Event;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
