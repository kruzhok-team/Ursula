using System;

namespace Ursula.GameObjects.Model
{
    public class CommonGameObjectLibraryItem : IEquatable<IGameObjectAsset>, IEquatable<GameObjectAssetInfo>, IEquatable<CommonGameObjectLibraryItem>
    {
        public CommonGameObjectLibraryItem(string name, string providerId)
        {
            Name = name;
            ProviderId = providerId;
        }

        public string Name { get; }
        public string ProviderId { get; }
        public string Id => ProviderId + "." + Name;

        public bool Equals(IGameObjectAsset other) => other != null && other.Info?.Id == Id;
        public bool Equals(GameObjectAssetInfo other) => other != null && other.Id == Id;
        public bool Equals(CommonGameObjectLibraryItem other) => other != null && other.Id == Id;
    }
}
