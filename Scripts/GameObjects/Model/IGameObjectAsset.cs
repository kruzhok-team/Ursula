namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public interface IGameObjectAsset // Provide read only data here
    {
        GameObjectAssetInfo Info { get; }
        object Texture { get; }
        object Model3d { get; }
    }
}
