using Fractural.Tasks;
using System.Collections.Generic;

namespace Ursula.GameObjects.Model
{
    public interface IGameObjectAssetProvider // Provide read only data here
    {
        string Id { get; }
        int ItemCount { get; }

        GDTask Load();
        IReadOnlyCollection<IGameObjectAsset> GetAll();
        bool ContainsItem(string name);
        bool TryGetItem(string name, out IGameObjectAsset asset);
    }
}
