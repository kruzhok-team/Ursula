
using Godot;
using System.IO;
using Ursula.Core.DI;

namespace Ursula.GameProjects.Model
{
    public partial class GameProjectAssetsUserSource : GameProjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "UserGameProjectAssets";
        public const string CollectionPath = "user://Project/Games/";
        public const string FolderPath = CollectionPath;

        public GameProjectAssetsUserSource() : base(LibId, FolderPath)
        {
            CheckExistDirectory();
        }

        public void OnDependenciesInjected()
        {
        }

        private void CheckExistDirectory()
        {
            //if (!Directory.Exists(ProjectSettings.GlobalizePath(FolderPath)))
            //{
            //    Directory.CreateDirectory(ProjectSettings.GlobalizePath(FolderPath));
            //}
        }
    }
}
