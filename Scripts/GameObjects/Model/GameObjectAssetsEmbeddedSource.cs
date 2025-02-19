using System.Collections.Generic;

namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsEmbeddedSource : GameObjectAssetJsonCollection
    {
        public const string LibId = "EmbeddedGameObjectAssets";
        public const string JsonDataPath = "";

        private List<string> _excludedObjectNames;

        public GameObjectAssetsEmbeddedSource() : base(LibId, JsonDataPath)
        {
        }
    }
}
