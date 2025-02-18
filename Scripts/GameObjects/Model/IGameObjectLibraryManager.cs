using Fractural.Tasks;

namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    // Common asset library interface
    public interface IGameObjectLibraryManager : IGameObjectAssetManager 
    {
        bool IsDataLoaded { get; }

        IGameObjectAssetProvider UserCollection { get; }
        IGameObjectAssetProvider EmbeddedCollection { get; }

        bool IsItemExcluded(string itemName);
        void SetItem(IGameObjectAsset asset);
        bool RestoreItem(string itemName);
        GDTask Load();
        void Save();
    }
}
