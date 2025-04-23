using Fractural.Tasks;

namespace Ursula.GameProjects.Model
{
    public interface IGameProjectAssetManager : IGameProjectAssetProvider // Provide full data control here
    {
        GameProjectAssetInfo SetItem(string name, GameProjectTemplate template, string libId);
        bool SaveItem(string itemId, string libId);
        void RemoveItem(string name);
        void RemoveItem(IGameProjectAsset asset);
    }
}
