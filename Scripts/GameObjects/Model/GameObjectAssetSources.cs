using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ursula.GameObjects.Model
{
    [Serializable]
    public class GameObjectAssetSources
    {
        public readonly string TextureFilePath;
        public readonly string Model3dFilePath;
        public readonly List<string> Audios;
        public readonly List<string> Animations;

        [JsonConstructor]
        public GameObjectAssetSources(string textureFilePath, string model3dFilePath, List<string> audios, List<string> animations)
        {
            TextureFilePath = textureFilePath;
            Model3dFilePath = model3dFilePath;
            Audios = audios;
            Animations = animations;
        }
        // ...
    }
}
