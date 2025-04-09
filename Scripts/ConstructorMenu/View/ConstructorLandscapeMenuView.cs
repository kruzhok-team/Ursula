using Fractural.Tasks;
using Godot;
using System;
using Ursula.ConstructorMenu.Model;
using Ursula.Core.DI;
using Ursula.StartupMenu.Model;
using Ursula.Terrain.Model;
using Ursula.Water.Model;

namespace Ursula.ConstructorMenu.View
{
    public partial class ConstructorLandscapeMenuView : ConstructorLandscapeMenuViewModel
    {
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
        private Button ButtonCreateLandscape;

        [Inject]
        private ISingletonProvider<ConstructorLandscapeMenuViewModel> _constructorLandscapeMenuViewModelProvider;

        [Inject]
        private ISingletonProvider<TerrainModel> _terrainModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateGameViewModel> _startupMenuCreateGameViewModelProvider;


        private ConstructorLandscapeMenuViewModel _constructorLandscapeMenuViewModel { get; set; }
        private TerrainModel _terrainModel { get; set; }
        private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            TabBarReplaceTexture.ClearTabs();
            for (int i = 0; i < VoxLib.mapAssets.terrainTexReplace.Length; i++)
            {
                Texture2D originalTexture = (Texture2D)VoxLib.mapAssets.terrainTexReplace[i];

                var image = originalTexture.GetImage();
                var newSize = new Vector2(38, 38);

                if (image.IsCompressed())
                {
                    image.Decompress();
                }

                image.Resize((int)newSize.X, (int)newSize.Y);

                var resizedTexture = ImageTexture.CreateFromImage(image);

                TabBarReplaceTexture.AddTab();
                TabBarReplaceTexture.SetTabIcon(i, resizedTexture);
            }
        }

        private async GDTask SubscribeEvent()
        {
            _terrainModel = await _terrainModelProvider.GetAsync();
            _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();

            ButtonCreateLandscape.ButtonDown += ButtonCreateLandscape_ButtonDownEvent;
        }

        private void ButtonCreateLandscape_ButtonDownEvent()
        {
            _startupMenuCreateGameViewModel.SetPowerValue((float)HSliderPower.Value);
            _startupMenuCreateGameViewModel.SetScaleValue((float)HSliderScale.Value);
            _startupMenuCreateGameViewModel.SetPlatoSizeValue((int)HSliderPlatoSize.Value);
            _startupMenuCreateGameViewModel.SetPlatoPlatoOffsetX((int)HSliderPlatoOffsetX.Value);
            _startupMenuCreateGameViewModel.SetPlatoPlatoOffsetZ((int)HSliderPlatoOffsetZ.Value);
            _startupMenuCreateGameViewModel.SetReplaceTextureID(TabBarReplaceTexture.CurrentTab);
            _startupMenuCreateGameViewModel.SetTypeSkyID(TypeSkyOption.Selected);
            _startupMenuCreateGameViewModel.SetFullDayLength((float)HSliderFullDayLength.Value);

            _terrainModel.SetPower(_startupMenuCreateGameViewModel._CreateGameSourceData.PowerValue);
            _terrainModel.SetScale(_startupMenuCreateGameViewModel._CreateGameSourceData.ScaleValue);
            _terrainModel.SetReplaceTexID(_startupMenuCreateGameViewModel._CreateGameSourceData.ReplaceTextureID);
            _terrainModel.SetPlatoSize(_startupMenuCreateGameViewModel._CreateGameSourceData.PlatoSize);
            _terrainModel.SetPlatoOffsetX(_startupMenuCreateGameViewModel._CreateGameSourceData.PlatoPlatoOffsetX);
            _terrainModel.SetPlatoOffsetZ(_startupMenuCreateGameViewModel._CreateGameSourceData.PlatoPlatoOffsetZ);
            _terrainModel.SetTypeSkyID(_startupMenuCreateGameViewModel._CreateGameSourceData.TypeSkyID);
            _terrainModel.SetFullDayLength(_startupMenuCreateGameViewModel._CreateGameSourceData.FullDayLength);

            _terrainModel.StartGenerateTerrain(true);
        }
    }
}
