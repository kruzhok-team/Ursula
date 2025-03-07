using System;
using System.Text.Json.Serialization;

namespace Ursula.GameObjects.Model
{
    [Serializable]
    public class GameObjectAssetInfo
    {
        [JsonConstructor]
        public GameObjectAssetInfo(string name, string providerId, GameObjectTemplate template)
        {
            Name = name;
            ProviderId = providerId;
            Template = template;
        }

        public string Name { get; }
        public string ProviderId { get; }
        public GameObjectTemplate Template { get; }
        public string Id => ProviderId + "." + Name;
    }
}
