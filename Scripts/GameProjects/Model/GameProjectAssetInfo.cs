using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ursula.GameObjects.Model;

namespace Ursula.GameProjects.Model
{
    [Serializable]
    public partial class GameProjectAssetInfo
    {
        [JsonConstructor]
        public GameProjectAssetInfo(string name, string providerId, GameProjectTemplate template)
        {
            Name = name;
            ProviderId = providerId;
            Template = template;
        }

        public string Name { get; }
        public string ProviderId { get; }
        public GameProjectTemplate Template { get; set; }
        public string Id => ProviderId + "." + Name;

        public long ProjectSize
        {
            get
            {
                return GetDirectorySize(GetFolderPath());
            }
        }

        private Texture2D previewImage;

        public async GDTask<Texture2D> GetPreviewImage()
        {
            if (previewImage != null) return previewImage;

            string path = Template.PreviewImageFilePath;

            path = $"{GameProjectAssetsUserSource.CollectionPath}{Template.Folder}/{Template.PreviewImageFilePath}";

            previewImage = await _LoadPreviewImage(path);

            return previewImage;
        }

        public string GetFolderPath()
        {
            string path = ProviderId == GameProjectAssetsUserSource.LibId
                ? $"{GameProjectAssetsUserSource.FolderPath}{Name}"
                : $"{GameProjectAssetsEmbeddedSource.FolderPath}{Name}";
            return ProjectSettings.GlobalizePath(path);
        }

        public string GetMapPath()
        {
            return $"{GetFolderPath()}/mapData.txt";
        }

        public bool LoadMap()
        {
            string pathMap = GetMapPath();

            VoxLib.mapManager.LoadMapFromFile(pathMap);

            return true;
        }

        private async GDTask<Texture2D> _LoadPreviewImage(string path)
        {
            Texture2D tex;
            Image img = new Image();

            var err = await Task.Run(() => img.Load(path));

            if (err != Error.Ok)
            {
                GD.Print("Failed to load image from path: " + path);
            }
            else
            {
                tex = ImageTexture.CreateFromImage(img);
                return tex;
            }
            return null;
        }

        private long GetDirectorySize(string directoryPath)
        {
            long totalSize = 0;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

                foreach (var file in dirInfo.GetFiles())
                {
                    totalSize += file.Length;
                }

                foreach (var subDir in dirInfo.GetDirectories())
                {
                    totalSize += GetDirectorySize(subDir.FullName);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Ошибка при вычислении размера директории: {ex.Message}");
            }

            return totalSize;
        }
    }
}
