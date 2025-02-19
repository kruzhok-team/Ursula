using Fractural.Tasks;

namespace Ursula.GameObjects.Model
{
    // Common asset library interface
    public interface IGameObjectLibraryManager : IGameObjectAssetManager 
    {
        bool IsDataLoaded { get; }

        IGameObjectAssetProvider UserCollection { get; }
        IGameObjectAssetProvider EmbeddedCollection { get; }

        bool IsItemExcluded(string itemName);
    }
}
