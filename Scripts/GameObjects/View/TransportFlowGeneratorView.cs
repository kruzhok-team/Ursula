using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement;
using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
    public partial class TransportFlowGeneratorView : Control, IInjectable
    {
        [Export]
        private GameObjectAssetInfoView RoadCrossPrefab;
        [Export]
        private GameObjectAssetInfoView RoadTPrefab;
        [Export]
        private GameObjectAssetInfoView RoadStraightPrefab;
        [Export]
        private GameObjectAssetInfoView RoadTurnPrefab;

        [Export]
        private GameObjectAssetInfoView TrafficLightGreenPrefab;
        [Export]
        private GameObjectAssetInfoView TrafficLightRedPrefab;

        [Export]
        private GameObjectAssetInfoView CarPrefab;

        [Export]
        private SliderShowValue SliderScale;
        [Export]
        private SliderShowValue SliderCarsCount;
        [Export]
        private SliderShowValue SliderTrafficLightsGreenTime;


        [Export]
        private Button ButtonGenerate;

        private float scale = 25;
        private int carsCount = 50;
        private float trafficLightsGreenTime = 5;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        private GameObjectCollectionModel _gameObjectCollectionModel;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;
        private GameObjectLibraryManager gameObjectLibraryManager;

        private bool firstTimeOpened = true;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            RoadCrossPrefab.clickItemEvent += OnRoadCrossPrefabClickItemEvent;
            RoadTPrefab.clickItemEvent += OnRoadTPrefabClickItemEvent;
            RoadStraightPrefab.clickItemEvent += OnRoadStraightPrefabClickItemEvent;
            RoadTurnPrefab.clickItemEvent += OnRoadTurnPrefabClickItemEvent;
            
            TrafficLightGreenPrefab.clickItemEvent += OnTrafficLightGreenPrefabClickItemEvent;
            TrafficLightRedPrefab.clickItemEvent += OnTrafficLightRedPrefabClickItemEvent;
            
            CarPrefab.clickItemEvent += OnCarPrefabClickItemEvent;

            SliderScale.ValueChanged += OnSliderScaleValueChanged;
            SliderCarsCount.ValueChanged += OnSliderCarsCountValueChanged;
            SliderTrafficLightsGreenTime.ValueChanged += OnSliderTrafficLightsGreenTimeValueChanged;

            SliderScale.Value = scale;
            SliderCarsCount.Value = carsCount;
            SliderTrafficLightsGreenTime.Value = trafficLightsGreenTime;

            ButtonGenerate.ButtonDown += OnButtonGenerateClick;

            this.VisibilityChanged += TransportFlowGeneratorView_VisibilityChanged;
        }

        private void TransportFlowGeneratorView_VisibilityChanged()
        {
            if (firstTimeOpened && Visible == true)
                TryLoadDefaultAssets();
            firstTimeOpened = false;
        }

        private void OnButtonGenerateClick()
        {
            GD.Print("Generate Transport Flow");
            GD.Print($"scale - {scale}, carsCount - {carsCount}, trafficLightsGreenTime - {trafficLightsGreenTime}");

            // Init
            NavGraphModelPlacer.Instance.Init(
                RoadCrossPrefab.GameObjectAssetInfo, 
                RoadTPrefab.GameObjectAssetInfo, 
                RoadStraightPrefab.GameObjectAssetInfo, 
                RoadTurnPrefab.GameObjectAssetInfo, 
                TrafficLightGreenPrefab.GameObjectAssetInfo, 
                TrafficLightRedPrefab.GameObjectAssetInfo, 
                CarPrefab.GameObjectAssetInfo
                );
            NavGraphManager.Instance.Init(scale, carsCount, trafficLightsGreenTime);
            
            // Generate
            TerrainManager terrainManager = VoxLib.terrainManager;
            float height = terrainManager.GetTerrainHeight(terrainManager.size / 2, terrainManager.size / 2);
           _ = NavGraphManager.Instance.Generate(terrainManager.countBlock, height);
        }

        private void OnButtonClearClick()
        {
            GD.Print("Clear Transport Flow");
            // Логика очистки транспортного потока
        }

        private void OnSliderTrafficLightsGreenTimeValueChanged(double value)
        {
            trafficLightsGreenTime = Convert.ToSingle(SliderTrafficLightsGreenTime.Value);
        }

        private void OnSliderCarsCountValueChanged(double value)
        {
            carsCount = Convert.ToInt32(SliderCarsCount.Value);
        }

        private void OnSliderScaleValueChanged(double value)
        {
            scale = Convert.ToSingle(SliderScale.Value);
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            gameObjectLibraryManager = await _commonLibraryProvider.GetAsync();
        }

        private void TryLoadDefaultAssets()
        {
            TryLoad(RoadCrossPrefab, $"{GameObjectAssetsEmbeddedSource.LibId}.road_cross");
            TryLoad(RoadTPrefab, $"{GameObjectAssetsEmbeddedSource.LibId}.road_t");
            TryLoad(RoadStraightPrefab, $"{GameObjectAssetsEmbeddedSource.LibId}.road_straight");
            TryLoad(RoadTurnPrefab, $"{GameObjectAssetsEmbeddedSource.LibId}.road_turn");
            TryLoad(TrafficLightGreenPrefab, $"{GameObjectAssetsEmbeddedSource.LibId}.traffic_light_green");
            TryLoad(TrafficLightRedPrefab, $"{GameObjectAssetsEmbeddedSource.LibId}.traffic_light_red");
            TryLoad(CarPrefab, $"{GameObjectAssetsEmbeddedSource.LibId}.Cow");
        }

        private void TryLoad(GameObjectAssetInfoView assetInfoView, string id)
        {
            if (gameObjectLibraryManager.TryGetItem(id, out IGameObjectAsset asset))
            {
                assetInfoView.Invalidate(asset.Info);
            }
        }

        private void OnCarPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            CarPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnTrafficLightRedPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            TrafficLightRedPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnTrafficLightGreenPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            TrafficLightGreenPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadTurnPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadTurnPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadStraightPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadStraightPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadTPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadTPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadCrossPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadCrossPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        public void OnDependenciesInjected()
        {
        }
    }
}
