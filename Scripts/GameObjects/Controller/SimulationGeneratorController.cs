using Fractural.Tasks;
using Godot;
using System;
using bearloga.addons.Ursula.Modules.LogicInjector;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Setters;
using Ursula.Terrain.Model;
using Ursula.Water.Model;

namespace ursula.addons.Ursula.Scripts.GameObjects.Controller
{
    public partial class SimulationGeneratorController : Node3D, IInjectable
    {
        [Inject]
        private ISingletonProvider<GameObjectCreateItemsModel> _GameObjectCreateItemsModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        [Inject]
        private ISingletonProvider<TerrainModel> _terrainModelProvider;

        [Inject]
        private ISingletonProvider<TerrainManager> _terrainManagerProvider;

        [Inject]
        private ISingletonProvider<WaterModel> _waterModelProvider;

        [Inject]
        private ISingletonProvider<SimulationGeneratorController> _simulationGeneratorControllerProvider;

        private GameObjectCreateItemsModel _gameObjectCreateItemsModel;
        private GameObjectCollectionModel _gameObjectCollectionModel;
        private MapManagerItemSetter _mapManagerItemSetter;
        private TerrainModel _terrainModel;
        private TerrainManager _terrainManager;
        private WaterModel _waterModel;


        public float Coefficient { get; private set; }

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            //_ = SubscribeEvent();
        }

        public void Init(
            GameObjectCreateItemsModel gameObjectCreateItemsModel,
            GameObjectCollectionModel gameObjectCollectionModel,
            MapManagerItemSetter mapManagerItemSetter,
            TerrainModel terrainModel,
            TerrainManager terrainManager,
            WaterModel waterModel)
        {
            _gameObjectCreateItemsModel = gameObjectCreateItemsModel;
            _gameObjectCollectionModel = gameObjectCollectionModel;
            _mapManagerItemSetter = mapManagerItemSetter;
            _terrainModel = terrainModel;
            _terrainManager = terrainManager;
            _waterModel = waterModel;

            Coefficient = 0f;
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectCreateItemsModel = await _GameObjectCreateItemsModelProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _terrainModel = await _terrainModelProvider.GetAsync();
            _terrainManager = await _terrainManagerProvider.GetAsync();
            _waterModel = await _waterModelProvider.GetAsync();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public async void GenerateSimulationItems(GameObjectAssetInfo asset1, GameObjectAssetInfo asset2, int entitiesCount, float percent, float coefficient, Injector injector = null)
        {
            if (_gameObjectCreateItemsModel == null
                || _gameObjectCollectionModel == null
                || _terrainModel?._TerrainData?.MapHeight == null
                || _terrainManager == null
                || _waterModel == null)
            {
                GD.PrintErr($"{nameof(SimulationGeneratorController)} dependencies are not initialized.");
                return;
            }

            entitiesCount = Mathf.Max(0, entitiesCount);
            percent = Mathf.Clamp(percent, 0f, 100f);
            Coefficient = coefficient;

            int firstAssetCount = Mathf.RoundToInt(entitiesCount * (percent * 0.01f));
            firstAssetCount = Mathf.Clamp(firstAssetCount, 0, entitiesCount);
            int secondAssetCount = entitiesCount - firstAssetCount;

            /*await*/ SpawnAssetAsync(asset1, firstAssetCount, injector);
            /*await*/ SpawnAssetAsync(asset2, secondAssetCount, injector);
        }

        private void SpawnAssetAsync(GameObjectAssetInfo assetInfo, int count, Injector injector = null)
        {
            if (assetInfo == null || count <= 0)
                return;

            int terrainSize = _terrainModel._TerrainData.Size;
            const int maxPositionAttempts = 32;

            for (int i = 0; i < count; i++)
            {
                if (!TryGetSpawnPosition(terrainSize, maxPositionAttempts, out Vector3 position))
                    break;

                _gameObjectCollectionModel.SetGameObjectAssetSelected(assetInfo);
                _gameObjectCreateItemsModel.SetGameObjectCreateItem(position, 1f, 0, false);
                
                Vector3 positionNode = _gameObjectCreateItemsModel.PositionNode;
                float scaleNode = _gameObjectCreateItemsModel.ScaleNode;
                byte rotationNode = _gameObjectCreateItemsModel.RotationNode;

                int _x = Mathf.RoundToInt(positionNode.X);
                int _y = Mathf.RoundToInt(positionNode.Y);
                int _z = Mathf.RoundToInt(positionNode.Z);

                int id = _x + _y * 256 + _z * 256 * 256;

                Node item = _mapManagerItemSetter.CreateGameItem(assetInfo, rotationNode, scaleNode, positionNode.X, positionNode.Y, positionNode.Z, 0, id, false, injector: injector);
            }
        }

        private bool TryGetSpawnPosition(int terrainSize, int maxAttempts, out Vector3 position)
        {
            position = Vector3.Zero;
            if (terrainSize <= 0)
                return false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int x = Mathf.RoundToInt(GD.Randi() % Math.Max(1, terrainSize - 1));
                int z = Mathf.RoundToInt(GD.Randi() % Math.Max(1, terrainSize - 1));
                float height = _terrainModel._TerrainData.MapHeight[x, z] + _terrainManager.positionOffset.Y;

                if (_waterModel != null && height < _waterModel._WaterData.WaterLevel)
                    continue;

                position = new Vector3(x, height, z);
                return true;
            }

            return false;
        }
    }
}
