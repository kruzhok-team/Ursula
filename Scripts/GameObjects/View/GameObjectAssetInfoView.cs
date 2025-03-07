using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using System.Threading.Tasks;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAssetInfoView : Control
    {
        [Export]
        private Label LabelNameAsset;

        [Export]
        private Button ButtonClickAsset;

        [Export]
        TextureRect PreviewImageRect;

        public Action<GameObjectAssetInfo> clickItemEvent = null;

        GameObjectAssetInfo _gameObjectAssetInfo;

        //
        public override void _Ready()
        {
            ButtonClickAsset.ButtonDown += OnItemClickEvent;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonClickAsset.ButtonDown -= OnItemClickEvent;
        }

        public async void Invalidate(GameObjectAssetInfo assetInfo)
        {
            PreviewImageRect.Visible = false;

            _gameObjectAssetInfo = assetInfo;
            LabelNameAsset.Text = assetInfo.Name;

            if (assetInfo.ProviderId == GameObjectAssetsUserSource.LibId)
            {

                string previewImageFilePath = assetInfo.Template.PreviewImageFilePath;

                // :TODO fix build paths
#if TOOLS              
                previewImageFilePath = $"{GameObjectAssetsUserSource.CollectionPath}{assetInfo.Template.Folder}/{previewImageFilePath}";
#else
                previewImageFilePath = $"{VoxLib.mapManager.GetCurrentProjectFolderPath()}{assetInfo.Template.Folder}/{previewImageFilePath}";
#endif

                await LoadPreviewImage(previewImageFilePath);
            }
            else if (assetInfo.ProviderId == GameObjectAssetsEmbeddedSource.LibId)
            {
                int idEmbeddedAsset = -1;
                int.TryParse(assetInfo.Template.PreviewImageFilePath, out idEmbeddedAsset);
                if (idEmbeddedAsset >= 0 && idEmbeddedAsset < VoxLib.mapAssets.inventarItemTex.Length)
                {
                    PreviewImageRect.Texture = (Texture2D)VoxLib.mapAssets.inventarItemTex[idEmbeddedAsset];
                }
            }

            PreviewImageRect.Visible = PreviewImageRect.Texture != null;
        }

        async GDTask LoadPreviewImage(string path)
        {
            PreviewImageRect.Texture = await _LoadPreviewImage(path);
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

        private void OnItemClickEvent()
        {
            string id = _gameObjectAssetInfo != null ? _gameObjectAssetInfo.Id : null;
            clickItemEvent?.Invoke(_gameObjectAssetInfo);
        }
    }
}
