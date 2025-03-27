using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.StartupMenu.Model;
using VoxLibExample;


namespace Ursula.StartupMenu.View
{
    public partial class StartupMenuCreateGameView : StartupMenuCreateGameViewModel
    {
        [Export]
        TextEdit TextEditGameName;

        [Export]
        TextEdit TextEditGameNameAlias;

        [Export]
        Button ButtonOpenGameImagePath;

        [Export]
        TextureRect TextureRectPreviewImage;

        [Export]
        HSlider HSliderPower;

        [Export]
        HSlider HSliderScale;

        [Export]
        Button ButtonCreatingGame;

        [Export]
        TabBar TabBarReplaceTexture;

        [Export]
        public OptionButton TypeSkyOption;

        [Export]
        PackedScene StartupItemInfoViewPrefab;

        [Export]
        private GridContainer GridContainerSkysView;

        [Export]
        HSlider HSliderFullDayLength;

        [Export]
        public OptionButton TypeWaterOption;
        [Export]
        public VoxDrawTypes TypeWater = VoxDrawTypes.water;
        [Export]
        HSlider HSliderWaterLevel;
        [Export]
        public CheckBox CheckButtonWaterStatic;

        [Export]
        TabBar TabBarTrees;

        [Export]
        private GridContainer GridContainerTrees;

        [Export]
        PackedScene GameObjectAssetInfoPrefab;



        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateGameViewModel> _startupMenuCreateGameViewModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddGameObjectAssetProvider;

        private StartupMenuModel _startupMenuModel { get; set; }
        private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }
        private GameObjectLibraryManager _commonLibrary { get; set; }
        private GameObjectAddGameObjectAssetModel _gameObjectAddGameObjectAsset { get; set; }

        private FileDialogTool dialogTool;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

            //var theme = new Theme();
            //var styleBox = new StyleBoxEmpty();
            //styleBox.ContentMarginLeft = 10;
            //styleBox.ContentMarginRight = 10;
            //styleBox.ContentMarginTop = 5;
            //styleBox.ContentMarginBottom = 5;

            //theme.SetStylebox("tab_fg", "TabBar", styleBox);
            //theme.SetStylebox("tab_bg", "TabBar", styleBox);

            //TabBarReplaceTexture.Theme = theme;

            TabBarReplaceTexture.ClearTabs();
            for (int i = 0; i < VoxLib.mapAssets.terrainTexReplace.Length; i++)
            {
                Texture2D originalTexture = (Texture2D)VoxLib.mapAssets.terrainTexReplace[i];

                var image = originalTexture.GetImage();
                var newSize = new Vector2(80, 80);

                if (image.IsCompressed())
                {
                    image.Decompress();
                }

                image.Resize((int)newSize.X, (int)newSize.Y);

                var resizedTexture = ImageTexture.CreateFromImage(image);

                TabBarReplaceTexture.AddTab();
                TabBarReplaceTexture.SetTabIcon(i, resizedTexture);
            }

            TabBarTrees_TabClickedEvent(0);
        }

        public override void _ExitTree()
        {
            Dispose();
        }

        public new void Dispose()
        {
            base.Dispose();
            //ButtonCreate?.Dispose();
            //ButtonLoad?.Dispose();
        }


        private async GDTask SubscribeEvent()
        {
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();
            _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _gameObjectAddGameObjectAsset = await _gameObjectAddGameObjectAssetProvider.GetAsync();

            _startupMenuModel.ButtonCreateGame_EventHandler += ButtonStartCreate_ButtonDownEvent;

            ButtonCreatingGame.ButtonDown += ButtonCreatingGame_ButtonDownEvent;
            ButtonOpenGameImagePath.ButtonDown += ButtonOpenGameImagePath_ButtonDownEvent;
            TabBarTrees.TabClicked += TabBarTrees_TabClickedEvent;
        }

        public void DrawItemsSky(int id)
        {
            VoxLib.RemoveAllChildren(GridContainerSkysView);

            //List<GameObjectAssetInfo> result = new List<GameObjectAssetInfo>(assets);

            //for (int i = 0; i < result.Count; i++)
            //{
            //    Node instance = StartupItemInfoViewPrefab.Instantiate();
            //    StartupItemInfoView item = instance as StartupItemInfoView;

            //    if (item == null)
            //        continue;

            //    item.clickItemEvent += ClickItem_SelectItemSkyEventHandler;
            //    item.Invalidate(result[i]);

            //    GridContainerSkysView.AddChild(instance);
            //}
        }

        private void ButtonCreatingGame_ButtonDownEvent()
        {
            _startupMenuCreateGameViewModel.SetGameName(TextEditGameName.Text);
            _startupMenuCreateGameViewModel.SetGameNameAlias(TextEditGameNameAlias.Text);
            _startupMenuCreateGameViewModel.SetPowerValue((float)HSliderPower.Value);
            _startupMenuCreateGameViewModel.SetScaleValue((float)HSliderScale.Value);
            _startupMenuCreateGameViewModel.SetReplaceTextureID(TabBarReplaceTexture.CurrentTab);
            _startupMenuCreateGameViewModel.SetTypeSkyID(TypeSkyOption.Selected);
            _startupMenuCreateGameViewModel.SetFullDayLength((float)HSliderFullDayLength.Value);
            _startupMenuCreateGameViewModel.SetTypeWaterID(TypeWaterOption.Selected);
            _startupMenuCreateGameViewModel.SetWaterOffset((float)HSliderWaterLevel.Value);
            _startupMenuCreateGameViewModel.SetStaticWater(CheckButtonWaterStatic.ButtonPressed);


            //_startupMenuCreateGameViewManager.CreatingGame();
        }

        private void ButtonStartCreate_ButtonDownEvent(object sender, EventArgs e)
        {
            _startupMenuCreateGameViewModel.SetCreateGameViewVisible(true);
        }

        private void ButtonOpenGameImagePath_ButtonDownEvent()
        {
            dialogTool.Open(new string[] { "*.jpg ; Файл jpg", "*.png ; Файл png" }, async (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Image img = new Image();
                    var err = img.Load(path);

                    if (err != Error.Ok)
                    {
                        GD.Print("Failed to load image from path: " + path);
                    }
                    else
                    {
                        TextureRectPreviewImage.Texture = ImageTexture.CreateFromImage(img);
                        _startupMenuCreateGameViewModel.SetGameImagePath(path);
                    }
                }
                else
                    GD.PrintErr($"Ошибка  открытия файла {path}");
            }
, FileDialog.AccessEnum.Filesystem);
        }


        private void ClickItem_SelectItemSkyEventHandler(int obj)
        {
            
        }

        private void TabBarTrees_TabClickedEvent(long tab)
        {
            string typeGroup = TabBarTrees.GetTabTitle((int)tab);

            string providerId = null;
            if ((int)tab == 0)
                providerId = GameObjectAssetsEmbeddedSource.LibId;
            else if ((int)tab == 1)
                providerId = GameObjectAssetsUserSource.LibId;

            DrawTreesCollection(providerId);
        }

        private void DrawTreesCollection(string providerId)
        {
            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');

            IReadOnlyCollection<GameObjectAssetInfo> assets = _commonLibrary.GetInfoOnGroup(gameObjectGroups[0], providerId);

            VoxLib.RemoveAllChildren(GridContainerTrees);

            Node nodeAdd = GameObjectAssetInfoPrefab.Instantiate();
            GameObjectAssetInfoView itemAdd = nodeAdd as GameObjectAssetInfoView;

            itemAdd.clickItemEvent += ClickItem_AddAssetEventHandler;
            itemAdd.Invalidate(null);

            GridContainerTrees.AddChild(nodeAdd);

            List<GameObjectAssetInfo> result = new List<GameObjectAssetInfo>(assets);

            for (int i = 0; i < result.Count; i++)
            {
                Node instance = GameObjectAssetInfoPrefab.Instantiate();
                GameObjectAssetInfoView item = instance as GameObjectAssetInfoView;

                if (item == null)
                    continue;

                //item.clickItemEvent += ClickItem_SelectEventHandler;
                item.Invalidate(result[i]);

                GridContainerTrees.AddChild(instance);
            }
        }

        private void ClickItem_AddAssetEventHandler(GameObjectAssetInfo info)
        {
            _startupMenuModel.SetStartupMenuMouseFilter(false);
            _gameObjectAddGameObjectAsset.GameGameObjectAddGameObjectAssetVisible_EventHandler += _gameObjectAddGameObjectAsset_GameGameObjectAddGameObjectAssetVisible_EventHandler;
            _gameObjectAddGameObjectAsset?.SetGameObjectAddGameObjectAssetVisible(true);
        }

        private void _gameObjectAddGameObjectAsset_GameGameObjectAddGameObjectAssetVisible_EventHandler(object sender, EventArgs e)
        {
            _gameObjectAddGameObjectAsset.GameGameObjectAddGameObjectAssetVisible_EventHandler -= _gameObjectAddGameObjectAsset_GameGameObjectAddGameObjectAssetVisible_EventHandler;
            _startupMenuModel.SetStartupMenuMouseFilter(true);
        }
    }
}
