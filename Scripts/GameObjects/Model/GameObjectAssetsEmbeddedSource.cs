using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Ursula.Core.DI;
using Ursula.MapManagers.Model;
using VoxLibExample;

namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsEmbeddedSource : GameObjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "EmbeddedGameObjectAssets";
        public const string CollectionPath = "user://Project/EmbeddedCollection/";
        public const string JsonDataPath = CollectionPath + "EmbeddedSource.json";

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;

        private GameObjectLibraryManager _commonLibrary;

        private List<string> _excludedObjectNames;

        public GameObjectAssetsEmbeddedSource() : base(LibId, JsonDataPath)
        {

        }

        public void OnDependenciesInjected()
        {
        }


    }
}
