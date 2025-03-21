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

        [Export]
        TextureRect LoadObjectImageRect;

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
            if (assetInfo != null)
            {
                _gameObjectAssetInfo = assetInfo;
                LabelNameAsset.Text = assetInfo.Name;

                PreviewImageRect.Texture = await assetInfo.GetPreviewImage();

                PreviewImageRect.Visible = PreviewImageRect.Texture != null;

                LoadObjectImageRect.Visible = false;
            }
            else
            {
                LabelNameAsset.Visible = false;
                LoadObjectImageRect.Visible = true;
            }
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
