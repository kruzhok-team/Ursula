using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.EmbeddedGames.Model;
using Ursula.MapManagers.Model;
using Ursula.MapManagers.Setters;
using Ursula.Terrain.Model;
using Ursula.Water.Model;
using VoxLibExample;
using static Godot.TileSet;

namespace Ursula.StartupMenu.Model
{
    public partial class StartupMenuCreateGameManager : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _gameProjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateGameViewModel> _startupMenuCreateGameViewModelProvider;

        [Inject]
        private ISingletonProvider<MapManager> _mapManagerProvider;

        [Inject]
        private ISingletonProvider<MapManagerModel> _mapManagerModelProvider;

        [Inject]
        private ISingletonProvider<TerrainModel> _terrainModelProvider;

        [Inject]
        private ISingletonProvider<WaterModel> _waterModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<TerrainManager> _terrainManagerProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectCreateItemsModel> _gameObjectCreateItemsModelProvider;

        private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }
        private GameProjectLibraryManager _gameProjectLibraryManager { get; set; }
        private MapManager _mapManager { get; set; }
        private MapManagerModel _mapManagerModel { get; set; }
        private TerrainModel _terrainModel { get; set; }
        private WaterModel _waterModel { get; set; }
        private GameObjectLibraryManager _commonLibrary { get; set; }
        private TerrainManager _terrainManager { get; set; }
        private GameObjectCollectionModel _gameObjectCollectionModel { get; set; }
        private GameObjectCreateItemsModel _gameObjectCreateItemsModel;

        void IInjectable.OnDependenciesInjected()
        {

        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        public async GDTask GeneratePlants()
        {
            _mapManagerModel.RemoveAllGameItems();
            await GenerateTrees();
            await GenerateGrass();
        }

        private async GDTask SubscribeEvent()
        {
            _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();
            _gameProjectLibraryManager = await _gameProjectLibraryManagerProvider.GetAsync();
            _mapManager = await _mapManagerProvider.GetAsync();
            _mapManagerModel = await _mapManagerModelProvider.GetAsync();
            _terrainModel = await _terrainModelProvider.GetAsync();
            _waterModel = await _waterModelProvider.GetAsync();
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _terrainManager = await _terrainManagerProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _gameObjectCreateItemsModel = await _gameObjectCreateItemsModelProvider.GetAsync();

            _startupMenuCreateGameViewModel.StartGenerateGame_EventHandler += StartupMenuCreateGameViewModel_StartGenerateGame_EventHandler;
            _startupMenuCreateGameViewModel.StartGeneratePlants_EventHandler += StartupMenuCreateGameViewModel_StartGeneratePlants_EventHandler;
        }

        private async void StartupMenuCreateGameViewModel_StartGenerateGame_EventHandler(object sender, EventArgs e)
        {
            GameProjectAssetInfo gameInfo = _gameProjectLibraryManager.currentProjectInfo;

            GameProjectTemplate template = new GameProjectTemplate
                (
                gameInfo.Template.Folder,
                _startupMenuCreateGameViewModel._CreateGameSourceData.GameNameAlias,
                Path.GetFileName(_startupMenuCreateGameViewModel._CreateGameSourceData.GameImagePath)
                );

            GameProjectAssetInfo gameInfoNew = new GameProjectAssetInfo(gameInfo.Name, gameInfo.ProviderId, template);

            _gameProjectLibraryManager.RemoveItem(gameInfoNew.Id);
            _gameProjectLibraryManager.SetItem(gameInfoNew.Name, template, gameInfoNew.ProviderId);
            _gameProjectLibraryManager.SaveItem(gameInfoNew.Id, gameInfoNew.ProviderId);

            await GDTask.DelayFrame(1);

            string fileName = Path.GetFileName(_startupMenuCreateGameViewModel._CreateGameSourceData.GameImagePath);
            string destPath = $"{_gameProjectLibraryManager.currentProjectInfo.GetProjectPath()}/{fileName}";
            CopyFile(_startupMenuCreateGameViewModel._CreateGameSourceData.GameImagePath, destPath);

            _terrainModel.SetScale(_startupMenuCreateGameViewModel._CreateGameSourceData.PowerValue);
            _terrainModel.SetPower(_startupMenuCreateGameViewModel._CreateGameSourceData.PowerValue);
            _terrainModel.SetReplaceTexID(_startupMenuCreateGameViewModel._CreateGameSourceData.ReplaceTextureID);
            _terrainModel.SetPlatoSize(_startupMenuCreateGameViewModel._CreateGameSourceData.PlatoSize);
            _terrainModel.SetPlatoOffsetX(_startupMenuCreateGameViewModel._CreateGameSourceData.PlatoPlatoOffsetX);
            _terrainModel.SetPlatoOffsetZ(_startupMenuCreateGameViewModel._CreateGameSourceData.PlatoPlatoOffsetZ);
            _terrainModel.StartGenerateTerrain(true);

            _waterModel.SetStaticWater(_startupMenuCreateGameViewModel._CreateGameSourceData.IsStaticWater);
            _waterModel.SetWaterOffset(_startupMenuCreateGameViewModel._CreateGameSourceData.WaterOffset);
            _waterModel.SetTypeWaterID(_startupMenuCreateGameViewModel._CreateGameSourceData.TypeWaterID + 1);
            _waterModel.StartGenerateWater();

            float treesDensity = _startupMenuCreateGameViewModel._CreateGameSourceData.TreesDensity;

            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            IReadOnlyCollection<GameObjectAssetInfo> assets = _commonLibrary.GetInfoOnGroup(gameObjectGroups[0], _startupMenuCreateGameViewModel._CreateGameSourceData.TreeProviderID);

            await GeneratePlants();

            _gameProjectLibraryManager.SetLoadProject(gameInfoNew);
            gameInfoNew.SaveMap();

        }

        private void StartupMenuCreateGameViewModel_StartGeneratePlants_EventHandler(object sender, EventArgs e)
        {
            _= GeneratePlants();
        }

        private void CopyFile(string path, string destPath)
        {
            try
            {
                File.Copy(path, destPath, true);
            }
            catch
            {

            }
        }

        private async GDTask GenerateTrees()
        {
            float treesDensity = _startupMenuCreateGameViewModel._CreateGameSourceData.TreesDensity;

            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            IReadOnlyCollection<GameObjectAssetInfo> assets = _commonLibrary.GetInfoOnGroup(gameObjectGroups[0], _startupMenuCreateGameViewModel._CreateGameSourceData.TreeProviderID);
            List<GameObjectAssetInfo> trees = new List<GameObjectAssetInfo>(assets);

            int _size = _terrainModel._TerrainData.Size;

            int amountTrees = Mathf.Min((int)(_size * _size * treesDensity / 64)
                , _size * _size / 64);

            if (amountTrees > 0)
            {
                VoxLib.ShowMessage("Генерация деревьев...");
            }

            for (int i = 0; i < amountTrees; i++)
            {
                int x = Mathf.RoundToInt(GD.Randi() % (_size - 1));
                int z = Mathf.RoundToInt(GD.Randi() % (_size - 1));
                int y = Mathf.RoundToInt(_terrainModel._TerrainData.MapHeight[x, z] + _terrainManager.positionOffset.Y);
                int id = x + z * 256 + y * 256 * 256;

                float positionY = _terrainModel._TerrainData.MapHeight[x, z] + _terrainManager.positionOffset.Y;

                if (positionY < _waterModel._WaterData.WaterLevel) continue;

                int rnd = (int)(GD.Randi() % (trees.Count - 1));

                GameObjectAssetInfo assetInfo = trees[rnd];

                _gameObjectCollectionModel.SetGameObjectAssetSelected(assetInfo);
                Vector3 pos = new Vector3(x, positionY, z);
                _gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, 1f, 0);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            //_gameObjectCollectionModel.SetGameObjectAssetSelected(null);
            //_mapManager.GenerateTrees(trees, amountTrees);
        }

        private async GDTask GenerateGrass()
        {
            float grassDensity = _startupMenuCreateGameViewModel._CreateGameSourceData.TreesDensity;

            string[] gameObjectGroups = MapAssets.GameObjectGroups.Split(',');
            IReadOnlyCollection<GameObjectAssetInfo> assets = _commonLibrary.GetInfoOnGroup(gameObjectGroups[1], _startupMenuCreateGameViewModel._CreateGameSourceData.TreeProviderID);
            List<GameObjectAssetInfo> grass = new List<GameObjectAssetInfo>(assets);

            int _size = _terrainModel._TerrainData.Size;

            int amountGrass = Mathf.Min((int)(_size * _size * grassDensity / 64)
                , _size * _size / 64);

            if (amountGrass > 0)
            {
                VoxLib.ShowMessage("Генерация травы...");
            }

            for (int i = 0; i < amountGrass; i++)
            {
                int x = Mathf.RoundToInt(GD.Randi() % (_size - 1));
                int z = Mathf.RoundToInt(GD.Randi() % (_size - 1));
                int y = Mathf.RoundToInt(_terrainModel._TerrainData.MapHeight[x, z] + _terrainManager.positionOffset.Y);
                int id = x + z * 256 + y * 256 * 256;

                float positionY = _terrainModel._TerrainData.MapHeight[x, z] + _terrainManager.positionOffset.Y;

                if (positionY < _waterModel._WaterData.WaterLevel) continue;

                int rnd = (int)(GD.Randi() % (grass.Count - 1));

                GameObjectAssetInfo assetInfo = grass[rnd];

                _gameObjectCollectionModel.SetGameObjectAssetSelected(assetInfo);
                Vector3 pos = new Vector3(x, positionY, z);
                _gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, 1f, 0);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            //_gameObjectCollectionModel.SetGameObjectAssetSelected(null);
        }
    }
}
