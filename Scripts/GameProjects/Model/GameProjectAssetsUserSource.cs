
using Ursula.Core.DI;

namespace Ursula.GameProjects.Model
{
    public partial class GameProjectAssetsUserSource : GameProjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "UserGameProjectAssets";
        public const string CollectionPath = "user://Project/Games/";
        public const string JsonDataPath = CollectionPath;

        public GameProjectAssetsUserSource() : base(LibId, JsonDataPath)
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
