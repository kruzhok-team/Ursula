namespace UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model
{
    public class GameObjectAssetInfo
    {
        public GameObjectAssetInfo(string name, string providerId, GameObjectAssetSources sources)
        {
            Name = name;
            ProviderId = providerId;
            Sources = sources;
        }

        public string Name { get; }
        public string ProviderId { get; }
        public GameObjectAssetSources Sources { get; }
        public string Id => ProviderId + "." + Name;
    }
}
