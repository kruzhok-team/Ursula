using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsUserSource : GameObjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "UserGameObjectAssets";
        public const string NameSource = "UserSource.json";
        public static string AssetPath = ProjectFolderPath + CollectionPath;
        public static string ProjectFolderPath;
        public const string CollectionPath = "/UserCollection/";
        public const string JsonDataPath = CollectionPath + NameSource;

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
