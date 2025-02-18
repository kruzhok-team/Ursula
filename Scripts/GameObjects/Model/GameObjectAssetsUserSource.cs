using Ursula.GameObjects;

namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public class GameObjectAssetsUserSource : GameObjectAssetJsonCollection
    {
        public const string Id = "UserGameObjectAssets";
        public const string JsonDataPath = "";

        public GameObjectAssetsUserSource() : base(Id, JsonDataPath)
        {
        }
    }
}
