using Fractural.Tasks;
using System.Collections.Generic;

namespace Ursula.GameProjects.Model
{
    public interface IGameProjectAssetProvider // Provide read only data here
    {
        string Id { get; }
        int ItemCount { get; }

        GDTask Load();
        IReadOnlyCollection<IGameProjectAsset> GetAll();
        bool ContainsItem(string name);
        bool TryGetItem(string name, out IGameProjectAsset asset);
    }
}
