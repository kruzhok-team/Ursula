using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.GameProjects.Model;
using Ursula.StartupMenu.Model;
using VoxLibExample;


namespace Ursula.StartupMenu.View
{
    public partial class StartupMenuCreateGameView : StartupMenuCreateGameViewModel
    {
        [Export]
        private TextEdit TextEditGameName;

        [Export]
        private TextEdit TextEditGameNameAlias;

        [Export]
        private Button ButtonOpenGameImagePath;

        [Export]
        private TextureRect TextureRectPreviewImage;

        [Export]
        private HSlider HSliderPower;

        [Export]
        private HSlider HSliderScale;

        [Export]
        public HSlider HSliderPlatoSize;

        [Export]
        public HSlider HSliderPlatoOffsetX;

        [Export]
        public HSlider HSliderPlatoOffsetZ;

        [Export]
        private Button ButtonCreatingGame;

        [Export]
        private TabBar TabBarReplaceTexture;

        [Export]
        private OptionButton TypeSkyOption;

        [Export]
        private PackedScene StartupItemInfoViewPrefab;

        [Export]
        private GridContainer GridContainerSkysView;

        [Export]
        private HSlider HSliderFullDayLength;

        [Export]
        private OptionButton TypeWaterOption;

        [Export]
        private VoxDrawTypes TypeWater = VoxDrawTypes.water;

        [Export]
        private HSlider HSliderWaterLevel;

        [Export]
        private CheckBox CheckButtonWaterStatic;

        [Export]
        private TabBar TabBarTrees;

        [Export]
        private GridContainer GridContainerTrees;

        [Export]
        private TabBar TabBarGrass;

        [Export]
        private GridContainer GridContainerGrass;

        [Export]
        private PackedScene GameObjectAssetInfoPrefab;

        [Export]
        private HSlider HSliderTreesDensity;

        [Export]
        private HSlider HSliderGrassDensity;




        [Inject]
        private ISingletonProvider<StartupMenuCreateNewProjectViewModel> _startupMenuCreateNewProjectViewModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateGameViewModel> _startupMenuCreateGameViewModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<GameObjectAddGameObjectAssetModel> _gameObjectAddGameObjectAssetProvider;

        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _gameProjectLibraryManagerProvider;

        private StartupMenuCreateNewProjectViewModel _startupMenuCreateNewProjectViewModel { get; set; }
        private StartupMenuModel _startupMenuModel { get; set; }
        private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }
        private GameObjectLibraryManager _commonLibrary { get; set; }
        private GameObjectAddGameObjectAssetModel _gameObjectAddGameObjectAsset { get; set; }
        private GameProjectLibraryManager _gameProjectLibraryManager { get; set; }


        private FileDialogTool dialogTool;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);

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

            _startupMenuCreateGameViewModel.SetTreeProviderID(GameObjectAssetsEmbeddedSource.LibId);
            _startupMenuCreateGameViewModel.SetGrassProviderID(GameObjectAssetsEmbeddedSource.LibId);
            Redraw();
        }

        public override void _ExitTree()
        {
            Dispose();
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        private void ShowView()
        {
            Visible = _startupMenuCreateGameViewModel.Visible;

            if (Visible == true)
            {
                TextEditGameName.Text = _startupMenuCreateNewProjectViewModel.GameName;
                _startupMenuCreateGameViewModel.SetGameName(_startupMenuCreateNewProjectViewModel.GameName);
                _startupMenuCreateGameViewModel.SetCreateGameViewCreateFolderGame();

                GameProjectTemplate projectTemplate = new GameProjectTemplate
                (
                    _startupMenuCreateNewProjectViewModel.GameName,
                    null,
                    null
                );


                string id = $"{GameProjectAssetsUserSource.LibId}.{_startupMenuCreateNewProjectViewModel.GameName}";
                bool isExist = _gameProjectLibraryManager.ContainsItem(id);

                if (!isExist)
                {
                    var info = _gameProjectLibraryManager.SetItem(_startupMenuCreateNewProjectViewModel.GameName, projectTemplate, GameProjectAssetsUserSource.LibId);
                    _gameProjectLibraryManager.SaveItem(info.Id, GameProjectAssetsUserSource.LibId);
                    _gameProjectLibraryManager.SetCurrentProjectInfo(info);
                }
            }

        }

        private async GDTask SubscribeEvent()
        {
            _startupMenuCreateNewProjectViewModel = await _startupMenuCreateNewProjectViewModelProvider.GetAsync();
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();
            _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _gameObjectAddGameObjectAsset = await _gameObjectAddGameObjectAssetProvider.GetAsync();
            _gameProjectLibraryManager = await _gameProjectLibraryManagerProvider.GetAsync();

            _startupMenuCreateGameViewModel.ViewVisible_EventHandler += (sender, args) => { ShowView(); };

            ButtonCreatingGame.ButtonDown += ButtonCreatingGame_ButtonDownEvent;
            ButtonOpenGameImagePath.ButtonDown += ButtonOpenGameImagePath_ButtonDownEvent;
            TabBarTrees.TabClicked += TabBarTrees_TabClickedEvent;
            TabBarGrass.TabClicked += TabBarGrass_TabClickedEvent;
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
            _startupMenuCreateGameViewModel.SetPlatoSizeValue((int)HSliderPlatoSize.Value);
            _startupMenuCreateGameViewModel.SetPlatoPlatoOffsetX((int)HSliderPlatoOffsetX.Value);
            _startupMenuCreateGameViewModel.SetPlatoPlatoOffsetZ((int)HSliderPlatoOffsetZ.Value);
            _startupMenuCreateGameViewModel.SetReplaceTextureID(TabBarReplaceTexture.CurrentTab);
            _startupMenuCreateGameViewModel.SetTypeSkyID(TypeSkyOption.Selected);
            _startupMenuCreateGameViewModel.SetFullDayLength((float)HSliderFullDayLength.Value);
            _startupMenuCreateGameViewModel.SetTypeWaterID(TypeWaterOption.Selected);
            _startupMenuCreateGameViewModel.SetWaterOffset((float)HSliderWaterLevel.Value);
            _startupMenuCreateGameViewModel.SetStaticWater(CheckButtonWaterStatic.ButtonPressed);
            _startupMenuCreateGameViewModel.SetTreesDensity((float)HSliderTreesDensity.Value);
            _startupMenuCreateGameViewModel.SetGrassDensity((float)HSliderGrassDensity.Value);

            _startupMenuCreateGameViewModel.StartGenerateGame();
        }

        private void Redraw()
        {
            DrawTreesCollection(_startupMenuCreateGameViewModel._CreateGameSourceData.TreeProviderID);
            DrawGrassCollection(_startupMenuCreateGameViewModel._CreateGameSourceData.GrassProviderID);
        }

        private void ButtonStartCreate_ButtonDownEvent(object sender, EventArgs e)
        {
            _startupMenuCreateGameViewModel.SetVisibleView(true);
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

            _startupMenuCreateGameViewModel.SetTreeProviderID(providerId);

            Redraw();
        }

        private void DrawTreesCollection(string providerId)
        {
            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');

            IReadOnlyCollection<GameObjectAssetInfo> assets = _commonLibrary.GetInfoOnGroup(gameObjectGroups[0], providerId);

            VoxLib.RemoveAllChildren(GridContainerTrees);

            if (providerId != GameObjectAssetsEmbeddedSource.LibId)
            {
                Node nodeAdd = GameObjectAssetInfoPrefab.Instantiate();
                GameObjectAssetInfoView itemAdd = nodeAdd as GameObjectAssetInfoView;

                itemAdd.clickItemEvent += ClickItem_TreeAddAssetEventHandler;
                itemAdd.Invalidate(null);

                GridContainerTrees.AddChild(nodeAdd);
            }

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

            if (providerId != GameObjectAssetsEmbeddedSource.LibId)
            {
                Node nodeAdd = GameObjectAssetInfoPrefab.Instantiate();
                GameObjectAssetInfoView itemAdd = nodeAdd as GameObjectAssetInfoView;

                itemAdd.clickItemEvent += ClickItem_GrassAddAssetEventHandler;
                itemAdd.Invalidate(null);

                GridContainerGrass.AddChild(nodeAdd);
            }

            List<GameObjectAssetInfo> result = new List<GameObjectAssetInfo>(assets);

            for (int i = 0; i < result.Count; i++)
            {
                Node instance = GameObjectAssetInfoPrefab.Instantiate();
                GameObjectAssetInfoView item = instance as GameObjectAssetInfoView;

                if (item == null)
                    continue;

                //item.clickItemEvent += ClickItem_SelectEventHandler;
                item.Invalidate(result[i]);

                GridContainerGrass.AddChild(instance);
            }
        }

        private void ClickItem_TreeAddAssetEventHandler(GameObjectAssetInfo info)
        {
            _gameObjectAddGameObjectAsset.GameGameObjectAddGameObjectAssetVisible_EventHandler += GameObjectAddGameObjectAsset_GameGameObjectAddGameObjectAssetVisible_EventHandler;

            _gameObjectAddGameObjectAsset.SetGameObjectGroup("Деревья");
            _gameObjectAddGameObjectAsset.SetGameObjectAddGameObjectAssetVisible(true);
        }

        private void ClickItem_GrassAddAssetEventHandler(GameObjectAssetInfo info)
        {
            _gameObjectAddGameObjectAsset.GameGameObjectAddGameObjectAssetVisible_EventHandler += GameObjectAddGameObjectAsset_GameGameObjectAddGameObjectAssetVisible_EventHandler;

            _gameObjectAddGameObjectAsset.SetGameObjectGroup("Трава");
            _gameObjectAddGameObjectAsset.SetGameObjectAddGameObjectAssetVisible(true);
        }

        private void GameObjectAddGameObjectAsset_GameGameObjectAddGameObjectAssetVisible_EventHandler(object sender, EventArgs e)
        {
            _gameObjectAddGameObjectAsset.GameGameObjectAddGameObjectAssetVisible_EventHandler -= GameObjectAddGameObjectAsset_GameGameObjectAddGameObjectAssetVisible_EventHandler;
            Redraw();
        }
    }
}
