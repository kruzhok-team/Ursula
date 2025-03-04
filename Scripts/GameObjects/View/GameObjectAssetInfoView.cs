using Godot;
using System;
using System.IO;
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

        public void Invalidate(GameObjectAssetInfo asset)
        {
            _gameObjectAssetInfo = asset;
            LabelNameAsset.Text = asset.Name;

            PreviewImageRect.Visible = !string.IsNullOrEmpty(asset.Sources.PreviewImageFilePath);

            if (asset.ProviderId == GameObjectAssetsEmbeddedSource.LibId)
            {
                int idEmbeddedAsset = -1;
                int.TryParse(asset.Sources.PreviewImageFilePath, out idEmbeddedAsset);
                if (idEmbeddedAsset >= 0 && idEmbeddedAsset < VoxLib.mapAssets.inventarItemTex.Length)
                {
                    PreviewImageRect.Texture = (Texture2D)VoxLib.mapAssets.inventarItemTex[idEmbeddedAsset];
                }
            }
        }


        //
        private void OnItemClickEvent()
        {
            string id = _gameObjectAssetInfo != null ? _gameObjectAssetInfo.Id : null;
            clickItemEvent?.Invoke(_gameObjectAssetInfo);
        }
    }
}
