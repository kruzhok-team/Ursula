using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using Ursula.Core.DI;
using Ursula.GameObjects;

namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public class GameObjectLibraryManager : IGameObjectLibraryManager
    {
        public const string JsonDataPath = "";

        [Inject]
        private ISingletonProvider<GameObjectAssetsUserSource> _userLibraryProvider;
        [Inject]
        private ISingletonProvider<GameObjectAssetsEmbeddedSource> _embeddedLibraryProvider;

        private GameObjectAssetJsonCollection _commonCollection;
        private HashSet<string> _exclusions;

        public GameObjectLibraryManager(string folderPath) 
        {
            _commonCollection = new(Id + ".Data", JsonDataPath);
        }

        public static string Id => "CommonGameObjectLibrary";
        public bool IsDataLoaded => _commonCollection?.IsDataLoaded ?? false;

        public int ItemCount => _commonCollection.ItemCount;

        public bool IsItemExcluded(string itemName)
        {
            if (!CheckLoaded())
                return false;
            return _exclusions.Contains(itemName);
        }

        public IReadOnlyCollection<IGameObjectAsset> GetAll()
        {
            return _commonCollection.GetAll();
        }

        public IReadOnlyCollection<IGameObjectAsset> GetFiltered(IEnumerable<string> excludeNames)
        {
            return _commonCollection.GetFiltered(excludeNames);
        }

        public bool ContainsItem(string itemName)
        {
            return _commonCollection.ContainsItem(itemName);
        }

        public void ExcludeItem(string itemName)
        {
            if (!_commonCollection.ContainsItem(itemName))
            {
                //TODO: Log an error in case where try to exclude not exist item
                return;
            }

            _commonCollection.RemoveItem(itemName);
            _exclusions.Add(itemName);
        }

        public void RemoveItem(string itemName)
        {
            _commonCollection.RemoveItem(itemName);
        }

        public async GDTask RemoveItem(IGameObjectAsset asset)
        {
            if (!CheckLoaded())
                return;

            if (asset == null)
                return;

            if (asset.Info.ProviderId == GameObjectAssetsEmbeddedSource.Id)
            {
                _exclusions.Add(asset.Info.Name);
                _commonCollection.RemoveItem(asset);
            }
            else if (asset.Info.ProviderId == GameObjectAssetsUserSource.Id)
            {
                var userLib = await _userLibraryProvider.GetAsync();
                _commonCollection.RemoveItem(asset);
            }
        }

        public bool RestoreItem(string itemName) => throw new System.NotImplementedException();
        public void SetItem(string name, GameObjectAssetSources sources) => throw new System.NotImplementedException();
        public bool TryGetItem(string name, out IGameObjectAsset asset) => throw new System.NotImplementedException();

        public async GDTask Load()
        {
            if (IsDataLoaded)
            {
                //TODO: Log an error or warning due to data already loaded
                return;
            }

            _commonCollection.Load();
            LoadExclusions();
            await SyncEmbeddedAssets();
        }

        private void LoadExclusions()
        { 
            //TODO: Implement exclussions list loading here
        }

        private async GDTask SyncEmbeddedAssets()
        {
            if (!CheckLoaded())
                return;

            var embeddedLib = await _embeddedLibraryProvider.GetAsync();

            if (embeddedLib == null)
            {
                GD.PrintErr("Not all game object liraries installed in DI!");
                return;
            }

            if (!embeddedLib.IsDataLoaded)
                embeddedLib.Load();
            if (!_commonCollection.IsDataLoaded)
                _commonCollection.Load();

            foreach (var asset in embeddedLib.GetAll())
            {
                if (!_commonCollection.ContainsItem(asset.Info.Name))
                {
                    _commonCollection.SetItem(asset.Info.Name, asset);
                }
            }
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
