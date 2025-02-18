namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public interface IGameObjectAssetManager : IGameObjectAssetProvider // Provide full data control here
    {
        void SetItem(string name, GameObjectAssetSources sources);
        void RemoveItem(string name);
        void RemoveItem(IGameObjectAsset asset);

    }
}
