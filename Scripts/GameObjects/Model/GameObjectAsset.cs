namespace Ursula.GameObjects.Model
{
    public class GameObjectAsset : IGameObjectAsset
    {
        public GameObjectAsset(GameObjectAssetInfo info, object texture, object model3d)
        {
            Info = info;
            Texture = texture;
            Model3d = model3d;
        }

        public GameObjectAssetInfo Info { get; private set; }
        public object Model3d { get; private set; } // TODO: Replace on a real data type
        public object Texture { get; private set; } // TODO: Replace on a real data type
        public object PreviewImage => Info.GetPreviewImage();

    }
}
