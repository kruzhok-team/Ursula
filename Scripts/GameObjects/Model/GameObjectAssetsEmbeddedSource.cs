using System.Collections.Generic;
using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsEmbeddedSource : GameObjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "EmbeddedGameObjectAssets";
        public const string CollectionPath = "user://Project/Models/EmbeddedCollection/";
        public const string JsonDataPath = CollectionPath + "EmbeddedSource.json";

        private List<string> _excludedObjectNames;

        public GameObjectAssetsEmbeddedSource() : base(LibId, JsonDataPath)
        {

        }

        public void OnDependenciesInjected()
        {
        }
    }
}
