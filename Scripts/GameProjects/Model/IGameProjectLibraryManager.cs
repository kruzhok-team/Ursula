using Godot;
using System;

namespace Ursula.GameProjects.Model
{
    public interface IGameProjectLibraryManager : IGameProjectAssetManager
    {
        bool IsDataLoaded { get; }

        IGameProjectAssetProvider UserCollection { get; }
        IGameProjectAssetProvider EmbeddedCollection { get; }

        bool IsItemExcluded(string itemName);
    }
}
