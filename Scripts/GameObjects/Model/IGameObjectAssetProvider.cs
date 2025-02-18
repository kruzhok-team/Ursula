using System.Collections.Generic;

namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public interface IGameObjectAssetProvider // Provide read only data here
    {
        string Id { get; }
        int ItemCount { get; }
        IReadOnlyCollection<IGameObjectAsset> GetAll();
        IReadOnlyCollection<IGameObjectAsset> GetFiltered(IEnumerable<string> excludeNames);
        bool ContainsItem(string name);
        bool TryGetItem(string name, out IGameObjectAsset asset);
    }
}
