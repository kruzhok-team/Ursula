namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public class GameObjectAssetSources
    {
        public readonly string PreviewImageFilePath;
        public readonly string TextureFilePath;
        public readonly string Model3dFilePath;

        public GameObjectAssetSources(string previewImageFilePath, string textureFilePath, string model3dFilePath)
        {
            PreviewImageFilePath = previewImageFilePath;
            TextureFilePath = textureFilePath;
            Model3dFilePath = model3dFilePath;
        }
        // ...
    }
}
