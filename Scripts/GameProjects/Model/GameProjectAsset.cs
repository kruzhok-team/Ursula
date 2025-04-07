using Godot;
using System;
using System.Xml.Linq;

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
        public string FolderPath => Info.GetFolderPath();
        public string MapPath => Info.GetMapPath();
        public bool LoadMap => Info.LoadMap();
        public bool SaveMap => Info.SaveMap();
    }
}
