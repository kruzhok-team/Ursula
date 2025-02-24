using System.Collections.Generic;
using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsEmbeddedSource : GameObjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "EmbeddedGameObjectAssets";
        public const string JsonDataPath = "";

        private List<string> _excludedObjectNames;

        public GameObjectAssetsEmbeddedSource() : base(LibId, JsonDataPath)
        {
        }

        public void OnDependenciesInjected()
        {
        }
    }
}
