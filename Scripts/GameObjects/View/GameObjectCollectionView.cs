using Godot;
using System.Collections.Generic;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    // Component to draw any game object library content
    public partial class GameObjectCollectionView : Node, IInjectable
    {
        [Export] 
        private GridContainer GridContainerCollectionView;

        [Export]
        PackedScene GameObjectAssetInfoPrefab;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddGameObjectAssetProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public void Draw(IReadOnlyCollection<GameObjectAssetInfo> assets)
        {
            VoxLib.RemoveAllChildren(GridContainerCollectionView);

            Node nodeAdd = GameObjectAssetInfoPrefab.Instantiate();
            GameObjectAssetInfoView itemAdd = nodeAdd as GameObjectAssetInfoView;

            itemAdd.clickItemEvent += ClickItem_AddAssetEventHandler;

            GridContainerCollectionView.AddChild(nodeAdd);

            List<GameObjectAssetInfo> result = new List<GameObjectAssetInfo>(assets);

            for (int i = 0; i < result.Count; i++)
            {
                Node instance = GameObjectAssetInfoPrefab.Instantiate();
                GameObjectAssetInfoView item = instance as GameObjectAssetInfoView;

                if (item == null)
                    continue;

                item.clickItemEvent += ClickItem_SelectEventHandler;
                item.Invalidate(result[i]);

                GridContainerCollectionView.AddChild(instance);
            }
        }

        async void ClickItem_SelectEventHandler(string itemId)
        {
            ControlPopupMenu.instance._HideAllMenu();
            var model = _gameObjectCollectionModelProvider != null ? await _gameObjectCollectionModelProvider.GetAsync() : null;
            model?.SetGameObjectAssetSelected(itemId);
        }

        async void ClickItem_AddAssetEventHandler(string itemId)
        {
            ControlPopupMenu.instance._HideAllMenu();
            var model = _gameObjectAddGameObjectAssetProvider != null ? await _gameObjectAddGameObjectAssetProvider.GetAsync() : null;
            model?.SetGameObjectAddGameObjectAssetVisible(true);
        }

    }
}
