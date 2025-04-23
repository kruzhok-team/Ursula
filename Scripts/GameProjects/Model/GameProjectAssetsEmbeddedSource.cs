using Godot;
using System;
using System.IO;
using Ursula.Core.DI;

namespace Ursula.GameProjects.Model
{
    public partial class GameProjectAssetsEmbeddedSource : GameProjectAssetJsonCollection, IInjectable
    {
        public const string LibId = "EmbeddedGameProjectAssets";
        public const string CollectionPath = "/Games/";
        public static string FolderPath;

        public GameProjectAssetsEmbeddedSource() : base(LibId, FolderPath)
        {
            CheckExistDirectory();
        }

        public void OnDependenciesInjected()
        {
        }

        private void CheckExistDirectory()
        {
//#if !TOOLS
//            if (!Directory.Exists(ProjectSettings.GlobalizePath(FolderPath)))
//            {
//                Directory.CreateDirectory(ProjectSettings.GlobalizePath(FolderPath));
//            }
//#endif
        }
    }
}