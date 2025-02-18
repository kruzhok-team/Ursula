using System.Collections.Generic;
using Ursula.GameObjects;

namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public class GameObjectAssetsEmbeddedSource : GameObjectAssetJsonCollection
    {
        public const string Id = "EmbeddedGameObjectAssets";
        public const string JsonDataPath = "";

        private List<string> _excludedObjectNames;

        public GameObjectAssetsEmbeddedSource() : base(Id, JsonDataPath)
        {
        }
    }
}
