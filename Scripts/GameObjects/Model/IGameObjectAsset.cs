namespace Ursula.GameObjects.Model
{
    public interface IGameObjectAsset // Provide read only data here
    {
        GameObjectAssetInfo Info { get; }
        object PreviewImage { get; }
        object Texture { get; }
        object Model3d { get; }
    }
}
