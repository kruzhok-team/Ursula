using Godot;
using System;
using System.Text.Json.Serialization;

namespace Ursula.GameObjects.Model
{
    [Serializable]
    public partial class GameObjectTemplate
    {
        public readonly string Folder;
        public readonly string GameObjectGroup;
        public readonly int GameObjectClass;
        public readonly string GameObjectSample;
        public readonly GameObjectAssetSources Sources;
        public readonly string GraphXmlPath;
        public readonly string PreviewImageFilePath;

        [JsonConstructor]
        public GameObjectTemplate(string folder, string gameObjectGroup, int gameObjectClass, string gameObjectSample, GameObjectAssetSources sources, string graphXmlPath, string previewImageFilePath)
        {
            Folder = folder;
            GameObjectGroup = gameObjectGroup;
            GameObjectClass = gameObjectClass;
            GameObjectSample = gameObjectSample;
            Sources = sources;
            GraphXmlPath = graphXmlPath;
            PreviewImageFilePath = previewImageFilePath;
        }
    }
}
