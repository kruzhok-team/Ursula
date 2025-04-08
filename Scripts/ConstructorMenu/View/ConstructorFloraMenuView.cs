using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using Ursula.ConstructorMenu.Model;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.StartupMenu.Model;
using VoxLibExample;

namespace Ursula.ConstructorMenu.View
{
    public partial class ConstructorFloraMenuView : ConstructorFloraMenuViewModel
    {
        [Export]
        private Button ButtonPlants;

        [Export]
        private Button ButtonTrees;

        [Export]
        private Button ButtonGrass;

        [Export]
        private TabBar TabBarTrees;

        [Export]
        private Label LabelTreeDensity;

        [Export]
        private Panel PanelTreeDensity;

        [Export]
        private GridContainer GridContainerTrees;

        [Export]
        private TabBar TabBarGrass;

        [Export]
        private Label LabelGrassDensity;

        [Export]
        private Panel PanelGrassDensity;

        [Export]
        private GridContainer GridContainerGrass;

        [Export]
        private PackedScene GameObjectAssetInfoPrefab;

        [Export]
        private HSlider HSliderTreesDensity;

        [Export]
        private HSlider HSliderGrassDensity;

        [Export]
        private Button ButtonGenerateFlora;


        [Inject]
        private ISingletonProvider<ConstructorFloraMenuViewModel> _constructorFloraMenuViewModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateGameViewModel> _startupMenuCreateGameViewModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;


        private ConstructorFloraMenuViewModel _constructorFloraMenuViewModel { get; set; }
        private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }
        private GameObjectLibraryManager _commonLibrary { get; set; }

        private bool isTreesView = true;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            RedrawVisible();

            _startupMenuCreateGameViewModel.SetTreeProviderID(GameObjectAssetsEmbeddedSource.LibId);
            _startupMenuCreateGameViewModel.SetGrassProviderID(GameObjectAssetsEmbeddedSource.LibId);
        }

        private async GDTask SubscribeEvent()
        {
            _constructorFloraMenuViewModel = await _constructorFloraMenuViewModelProvider.GetAsync();
            _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();
            _commonLibrary = await _commonLibraryProvider.GetAsync();

            ButtonPlants.ButtonDown += Redraw;

            ButtonGenerateFlora.ButtonDown += ButtonGenerateFlora_ButtonDownEvent;

            ButtonTrees.ButtonDown += ButtonTrees_ButtonDownEvent;
            ButtonGrass.ButtonDown += ButtonGrass_ButtonDownEvent;

            TabBarTrees.TabClicked += TabBarTrees_TabClickedEvent;
            TabBarGrass.TabClicked += TabBarGrass_TabClickedEvent;
        }

        private void ButtonTrees_ButtonDownEvent()
        {
            isTreesView = true;
            RedrawVisible();
        }

        private void ButtonGrass_ButtonDownEvent()
        {
            isTreesView = false;
            RedrawVisible();
        }

        private void RedrawVisible()
        {
            TabBarTrees.Visible = isTreesView;
            GridContainerTrees.Visible = isTreesView;
            LabelTreeDensity.Visible = isTreesView;
            PanelTreeDensity.Visible = isTreesView;

            TabBarGrass.Visible = !isTreesView;
            GridContainerGrass.Visible = !isTreesView;
            LabelGrassDensity.Visible = !isTreesView;
            PanelGrassDensity.Visible = !isTreesView;
        }

        private void Redraw()
        {
            DrawTreesCollection(_startupMenuCreateGameViewModel._CreateGameSourceData.TreeProviderID);
            DrawGrassCollection(_startupMenuCreateGameViewModel._CreateGameSourceData.GrassProviderID);
            RedrawVisible();
        }

        private void ButtonGenerateFlora_ButtonDownEvent()
        {
            _startupMenuCreateGameViewModel.SetTreesDensity((float)HSliderTreesDensity.Value);
            _startupMenuCreateGameViewModel.SetGrassDensity((float)HSliderGrassDensity.Value);

            _startupMenuCreateGameViewModel.StartGeneratePlants();
        }

        private void TabBarTrees_TabClickedEvent(long tab)
        {
            string typeGroup = TabBarTrees.GetTabTitle((int)tab);

            string providerId = null;
            if ((int)tab == 0)
                providerId = GameObjectAssetsEmbeddedSource.LibId;
            else if ((int)tab == 1)
                providerId = GameObjectAssetsUserSource.LibId;

            _startupMenuCreateGameViewModel.SetTreeProviderID(providerId);

            Redraw();
        }

        private void DrawTreesCollection(string providerId)
        {
            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');

            IReadOnlyCollection<GameObjectAssetInfo> assets = _commonLibrary.GetInfoOnGroup(gameObjectGroups[0], providerId);

            VoxLib.RemoveAllChildren(GridContainerTrees);

            List<GameObjectAssetInfo> trees = new List<GameObjectAssetInfo>(assets);

            for (int i = 0; i < trees.Count; i++)
            {
                Node instance = GameObjectAssetInfoPrefab.Instantiate();
                GameObjectAssetInfoView item = instance as GameObjectAssetInfoView;

                if (item == null)
                    continue;

                item.Invalidate(trees[i]);

                GridContainerTrees.AddChild(instance);

                Control control = instance as Control;
                control.Size = new Vector2(70, 90);
            }
        }

        private void TabBarGrass_TabClickedEvent(long tab)
        {
            string typeGroup = TabBarGrass.GetTabTitle((int)tab);

            string providerId = null;
            if ((int)tab == 0)
                providerId = GameObjectAssetsEmbeddedSource.LibId;
            else if ((int)tab == 1)
                providerId = GameObjectAssetsUserSource.LibId;

            _startupMenuCreateGameViewModel.SetGrassProviderID(providerId);

            Redraw();
        }

        private void DrawGrassCollection(string providerId)
        {
            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');

            IReadOnlyCollection<GameObjectAssetInfo> assets = _commonLibrary.GetInfoOnGroup(gameObjectGroups[1], providerId);

            VoxLib.RemoveAllChildren(GridContainerGrass);

            List<GameObjectAssetInfo> result = new List<GameObjectAssetInfo>(assets);

            for (int i = 0; i < result.Count; i++)
            {
                Node instance = GameObjectAssetInfoPrefab.Instantiate();
                GameObjectAssetInfoView item = instance as GameObjectAssetInfoView;

                if (item == null)
                    continue;

                item.Invalidate(result[i]);

                GridContainerGrass.AddChild(instance);
            }
        }


    }
}