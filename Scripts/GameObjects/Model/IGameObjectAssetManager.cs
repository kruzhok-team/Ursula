using Fractural.Tasks;

namespace Ursula.GameObjects.Model
{
    public interface IGameObjectAssetManager : IGameObjectAssetProvider // Provide full data control here
    {
        void SetItem(string name, GameObjectTemplate template, string libId);
        void RemoveItem(string name);
        void RemoveItem(IGameObjectAsset asset);
        GDTask Save();
    }
}
