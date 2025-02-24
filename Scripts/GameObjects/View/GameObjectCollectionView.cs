using Godot;
using System.Collections.Generic;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    // Component to draw any game object library content
    public partial class GameObjectCollectionView : Node
    {
        public void Draw(IReadOnlyCollection<IGameObjectAsset> assets) //(IGameObjectAssetProvider assets)
        {
            List<IGameObjectAsset> result = new List<IGameObjectAsset>(assets);

        }
    }
}
