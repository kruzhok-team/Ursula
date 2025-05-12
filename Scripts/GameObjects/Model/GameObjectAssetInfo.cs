using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public GameObjectTemplate Template { get; set; }
        public string Id => ProviderId + "." + Name;

        private Texture2D previewImage;

        public string GetAssetPath()
        {
            if (ProviderId == GameObjectAssetsUserSource.LibId)
                return $"{GameObjectAssetsUserSource.ProjectFolderPath}{GameObjectAssetsUserSource.CollectionPath}{Name}";
            else if (ProviderId == GameObjectAssetsEmbeddedSource.LibId)
                return $"{GameObjectAssetsUserSource.ProjectFolderPath}{GameObjectAssetsEmbeddedSource.CollectionPath}{Name}";
            return null;
        }

        public string GetGraphXmlPath()
        {
            return $"{GetAssetPath()}/{Template.GraphXmlPath}";
        }

        public async GDTask<Texture2D> GetPreviewImage()
        {
            if (previewImage != null) return previewImage;

            string path = Template.PreviewImageFilePath;
            string collectionPath = "";

//#if TOOLS
            path = $"{GetAssetPath()}/{Template.PreviewImageFilePath}";
            //#else
            //            path = $"{VoxLib.mapManager.GetCurrentProjectFolderPath()}{Template.Folder}/{Template.PreviewImageFilePath}";
            //#endif

            if (ProviderId == GameObjectAssetsUserSource.LibId)
                previewImage = await _LoadPreviewImage(path);
            else
            {
                int idEmbeddedAsset = -1;
                int.TryParse(Template.PreviewImageFilePath, out idEmbeddedAsset);
                if (idEmbeddedAsset >= 0 && idEmbeddedAsset < VoxLib.mapAssets.inventarItemTex.Length)
                {
                    previewImage = (Texture2D)VoxLib.mapAssets.inventarItemTex[idEmbeddedAsset];
                }
            }

            return previewImage;
        }

        private async GDTask<Texture2D> _LoadPreviewImage(string path)
        {
            Texture2D tex;
            Image img = new Image();

            if (!File.Exists(path)) return null;

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
