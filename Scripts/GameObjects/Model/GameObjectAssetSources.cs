using System;
using System.Collections.Generic;

namespace Ursula.GameObjects.Model
{
    [Serializable]
    public class GameObjectAssetSources
    {
        public readonly string PreviewImageFilePath;
        public readonly string TextureFilePath;
        public readonly string Model3dFilePath;
        public readonly string GameObjectGroup;
        public readonly int GameObjectClass;
        public readonly string GameObjectSample;
        public readonly List<string> Audios;
        public readonly List<string> Animations;
        public readonly string GraphXmlPath;

        public GameObjectAssetSources(string previewImageFilePath, string textureFilePath, string model3dFilePath, string gameObjectGroup, int gameObjectClass, string gameObjectSample, List<string> audios, List<string> animations, string graphXmlPath)
        {
            PreviewImageFilePath = previewImageFilePath;
            TextureFilePath = textureFilePath;
            Model3dFilePath = model3dFilePath;
            GameObjectGroup = gameObjectGroup;
            GameObjectClass = gameObjectClass;
            GameObjectSample = gameObjectSample;
            Audios = audios;
            Animations = animations;
            GraphXmlPath = graphXmlPath;
        }
        // ...
    }
}
