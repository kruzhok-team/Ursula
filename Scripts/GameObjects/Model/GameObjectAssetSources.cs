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
        public readonly string ModelType;
        public readonly List<string> Audios;
        public readonly List<string> Animations;

        public GameObjectAssetSources(string previewImageFilePath, string textureFilePath, string model3dFilePath, string modelType, List<string> audios, List<string> animations)
        {
            PreviewImageFilePath = previewImageFilePath;
            TextureFilePath = textureFilePath;
            Model3dFilePath = model3dFilePath;
            ModelType = modelType;
            Audios = audios;
            Animations = animations;
        }
        // ...
    }
}
