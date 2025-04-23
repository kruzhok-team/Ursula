using Godot;
using System;

namespace Ursula.EmbeddedGames.Model
{
    public partial class CommonGameProjectLibraryItem : IEquatable<IGameProjectAsset>, IEquatable<GameProjectAssetInfo>, IEquatable<CommonGameProjectLibraryItem>
    {
        public CommonGameProjectLibraryItem(string name, string providerId)
        {
            Name = name;
            ProviderId = providerId;
        }

        public string Name { get; }
        public string ProviderId { get; }
        public string Id => ProviderId + "." + Name;

        public bool Equals(IGameProjectAsset other) => other != null && other.Info?.Id == Id;
        public bool Equals(GameProjectAssetInfo other) => other != null && other.Id == Id;
        public bool Equals(CommonGameProjectLibraryItem other) => other != null && other.Id == Id;
    }
}
