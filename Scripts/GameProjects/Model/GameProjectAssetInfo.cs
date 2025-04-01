using Fractural.Tasks;
using Godot;
using System;
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

        private Texture2D previewImage;

        public async GDTask<Texture2D> GetPreviewImage()
        {
            if (previewImage != null) return previewImage;

            string path = Template.PreviewImageFilePath;

            path = $"{GameProjectAssetsUserSource.CollectionPath}{Template.Folder}/{Template.PreviewImageFilePath}";

            previewImage = await _LoadPreviewImage(path);

            return previewImage;
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
    }
}
