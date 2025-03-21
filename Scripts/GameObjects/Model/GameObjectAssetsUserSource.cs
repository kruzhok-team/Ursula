using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsUserSource : GameObjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "UserGameObjectAssets";
        public const string CollectionPath = "user://Project/UserCollection/";
        public const string JsonDataPath = CollectionPath + "UserSource.json";

        public GameObjectAssetsUserSource() : base(LibId, JsonDataPath)
        {
            CheckExistDirectory();
        }

        public void OnDependenciesInjected()
        {
        }

        private void CheckExistDirectory()
        {

        }
    }
}
