using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameProjects.Model;
using Ursula.MapManagers.Model;

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

        private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }
        private GameProjectLibraryManager _gameProjectLibraryManager { get; set; }
        private MapManager _mapManager { get; set; }
        private MapManagerModel _mapManagerModel { get; set; }


        void IInjectable.OnDependenciesInjected()
        {

        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();
            _gameProjectLibraryManager = await _gameProjectLibraryManagerProvider.GetAsync();
            _mapManager = await _mapManagerProvider.GetAsync();
            _mapManagerModel = await _mapManagerModelProvider.GetAsync();

            _startupMenuCreateGameViewModel.StartGenerateGame_EventHandler += StartupMenuCreateGameViewModel_StartGenerateGame_EventHandler;
        }

        private void StartupMenuCreateGameViewModel_StartGenerateGame_EventHandler(object sender, EventArgs e)
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

            string fileName = Path.GetFileName(_startupMenuCreateGameViewModel._CreateGameSourceData.GameImagePath);
            string destPath = $"{_gameProjectLibraryManager.currentProjectInfo.GetFolderPath()}/{fileName}";
            CopyFile(_startupMenuCreateGameViewModel._CreateGameSourceData.GameImagePath, destPath);


            // :TODO fix new generation
            VoxLib.createTerrain.scale = _startupMenuCreateGameViewModel._CreateGameSourceData.ScaleValue;
            VoxLib.createTerrain.power = _startupMenuCreateGameViewModel._CreateGameSourceData.PowerValue;

            VoxLib.mapManager.StartCoroutineCreateTerrain(true);
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
    }
}
