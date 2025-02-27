using Godot;
using System;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectAssetInfoView : Control
    {
        [Export]
        private Label LabelNameAsset;

        [Export]
        Button ButtonClickAsset;

        [Export]
        TextureRect PreviewImageRect;

        GameObjectAssetInfo _gameObjectAssetInfo;


    }
}
