using Godot;
using System;
using Ursula.GameProjects.Model;

namespace Ursula.GameProjects.View
{
    public partial class GameProjectLoadProjectInfoView : Node
    {
        [Export]
        private Label LabelNameAsset;

        [Export]
        private Label LabelSizeAsset;

        [Export]
        TextureRect PreviewImageRect;

        [Export]
        private Button ButtonClickAsset;


        public Action<GameProjectAssetInfo> clickItemEvent = null;

        GameProjectAssetInfo _gameProjectAssetInfo;
       
        public override void _Ready()
        {
            ButtonClickAsset.ButtonDown += OnItemClickEvent;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonClickAsset.ButtonDown -= OnItemClickEvent;
        }

        public async void Generate(GameProjectAssetInfo assetInfo)
        {
            PreviewImageRect.Visible = false;
            if (assetInfo != null)
            {
                _gameProjectAssetInfo = assetInfo;
                LabelNameAsset.Text = assetInfo.Name;

                PreviewImageRect.Texture = await assetInfo.GetPreviewImage();

                PreviewImageRect.Visible = PreviewImageRect.Texture != null;
            }
            else
            {
                LabelNameAsset.Visible = false;
            }

            long sizeInBytes = (long)(assetInfo.ProjectSize / (1024.0 * 1024));

            LabelSizeAsset.Text = $"{sizeInBytes:F2} mb";
        }

        private void OnItemClickEvent()
        {
            clickItemEvent?.Invoke(_gameProjectAssetInfo);
        }
    }
}
