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
        public static string JsonDataPath;

        public GameProjectAssetsEmbeddedSource() : base(LibId, JsonDataPath)
        {
            string path = Path.GetDirectoryName(OS.GetExecutablePath());
            JsonDataPath = ProjectSettings.GlobalizePath($"{path}{CollectionPath}");
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