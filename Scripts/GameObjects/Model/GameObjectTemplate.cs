using Godot;
using System;

namespace Ursula.GameObjects.Model
{
    [Serializable]
    public partial class GameObjectTemplate
    {
        public readonly GameObjectAssetSources AssetSources;
        public readonly string GameObjectGroup;
        public readonly int GameObjectClass;
        public readonly string GameObjectSample;

        public GameObjectTemplate(string gameObjectGroup, int gameObjectClass, string gameObjectSample, GameObjectAssetSources gameObjectAssetSources)
        {
            AssetSources = gameObjectAssetSources;
            GameObjectGroup = gameObjectGroup;
            GameObjectClass = gameObjectClass;
            GameObjectSample = gameObjectSample;
        }
    }
}
