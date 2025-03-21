using Fractural.Tasks;
using Godot;
using System.Collections.Generic;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using VoxLibExample;

namespace Ursula.GameObjects.View
{
    // Component to draw any game object library content
    public partial class GameObjectCollectionView : Node, IInjectable
    {
        [Export] 
        private GridContainer GridContainerCollectionView;

        [Export]
        PackedScene GameObjectAssetInfoPrefab;

        [Export]
        TabBar TabBarGameObjectGroup;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddGameObjectAssetProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        private GameObjectCollectionModel _gameObjectCollectionModel;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            SetTabBar();
        }

        public void Draw(IReadOnlyCollection<GameObjectAssetInfo> assets)
        {
            VoxLib.RemoveAllChildren(GridContainerCollectionView);

            Node nodeAdd = GameObjectAssetInfoPrefab.Instantiate();
            GameObjectAssetInfoView itemAdd = nodeAdd as GameObjectAssetInfoView;

            itemAdd.clickItemEvent += ClickItem_AddAssetEventHandler;
            itemAdd.Invalidate(null);

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

        async void ClickItem_SelectEventHandler(GameObjectAssetInfo asset)
        {
            ControlPopupMenu.instance._HideAllMenu();
            var model = _gameObjectCollectionModelProvider != null ? await _gameObjectCollectionModelProvider.GetAsync() : null;
            model?.SetGameObjectAssetSelected(asset);
        }

        async void ClickItem_AddAssetEventHandler(GameObjectAssetInfo asset)
        {
            ControlPopupMenu.instance._HideAllMenu();
            var model = _gameObjectAddGameObjectAssetProvider != null ? await _gameObjectAddGameObjectAssetProvider.GetAsync() : null;
            model?.SetGameObjectAddGameObjectAssetVisible(true);
        }

        private void SetTabBar()
        {
            TabBarGameObjectGroup.ClearTabs();
            TabBarGameObjectGroup.AddTab("Все объекты");
            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            for (int i = 0; i < gameObjectGroups.Length; i++)
            {
                TabBarGameObjectGroup.AddTab(gameObjectGroups[i]);
            }

            TabBarGameObjectGroup.TabClicked += TabBarGameObjectGroup_TabClickedEvent;
        }

        async void TabBarGameObjectGroup_TabClickedEvent(long tab)
        {
            var model = _gameObjectCollectionModelProvider != null ? await _gameObjectCollectionModelProvider.GetAsync() : null;
            string nameGroup = (int)tab == 0 ? "" : TabBarGameObjectGroup.GetTabTitle((int)tab);
            model?.DrawCollection(nameGroup);
        }
    }
}
