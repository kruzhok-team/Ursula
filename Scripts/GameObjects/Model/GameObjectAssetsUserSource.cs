using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsUserSource : GameObjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "UserGameObjectAssets";
        public const string JsonDataPath = "user://Project/Models/UserCatalog/UserSource.json";

        public GameObjectAssetsUserSource() : base(LibId, JsonDataPath)
        {

        }

        public void OnDependenciesInjected()
        {
        }
    }
}
