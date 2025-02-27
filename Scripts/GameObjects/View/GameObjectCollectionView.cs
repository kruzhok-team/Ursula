using Godot;
using System.Collections.Generic;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    // Component to draw any game object library content
    public partial class GameObjectCollectionView : Node
    {
        public void Draw(IReadOnlyCollection<GameObjectAssetInfo> assets)
        {
            //List<IGameObjectAssetProvider> result = new List<IGameObjectAssetProvider>(assets);


        }
    }
}
