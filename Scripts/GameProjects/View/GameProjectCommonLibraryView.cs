using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.EmbeddedGames.Model;
using Ursula.StartupMenu.Model;

namespace Ursula.EmbeddedGames.View
{
    public partial class GameProjectCommonLibraryView : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        private GameProjectLibraryManager _commonLibrary { get; set; }
        private GameObjectLibraryManager _gameObjectLibraryManager { get; set; }
        private StartupMenuModel _startupMenuModel { get; set; }

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();

            _ = Load();
            _ = SubscribeEvent();
        }

        private async GDTask Load()
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            await _commonLibrary.Load();
        }

        private async GDTask SubscribeEvent()
        {
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();

            _commonLibrary.GameProjectSetLoadProject_Event += GameProjectSetLoadProject_EventHandler;
            _startupMenuModel.LoadLibrary_Event += StartupMenuModel_LoadLibrary_EventHandler;
        }

        private async void GameProjectSetLoadProject_EventHandler(object sender, EventArgs e)
        {
            _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
            await _gameObjectLibraryManager.Load(_commonLibrary.currentProjectInfo.GetProjectPath());
            await _commonLibrary.currentProjectInfo.LoadMap();

            _startupMenuModel?.SetVisibleView(false);
        }

        private void StartupMenuModel_LoadLibrary_EventHandler(object sender, EventArgs e)
        {
            _ = Load();
        }
    }
}
