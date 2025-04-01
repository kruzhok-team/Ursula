using Godot;
using System;

namespace Ursula.GameProjects.Model
{
    public partial class GameProjectAsset : IGameProjectAsset
    {
        public GameProjectAsset(GameProjectAssetInfo info)
        {
            Info = info;
        }

        public GameProjectAssetInfo Info { get; private set; }
        public object PreviewImage => Info.GetPreviewImage();

    }
}
