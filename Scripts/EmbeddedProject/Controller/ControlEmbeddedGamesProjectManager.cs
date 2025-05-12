using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameProjects.Model;
using Ursula.GameObjects.Model;
using Ursula.StartupMenu.Model;
using Ursula.EmbeddedGames.Model;

namespace Ursula.EmbeddedGames.Manager
{
    public partial class ControlEmbeddedGamesProjectManager : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<ControlEmbeddedGamesProjectViewModel> _controlEmbeddedGamesProjectViewModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        private GameProjectLibraryManager _commonLibrary { get; set; }
        private ControlEmbeddedGamesProjectViewModel _controlEmbeddedGamesProjectViewModel { get; set; }
        private StartupMenuModel _startupMenuModel { get; set; }
        private GameObjectLibraryManager _gameObjectLibraryManager { get; set; }

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
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _controlEmbeddedGamesProjectViewModel = await _controlEmbeddedGamesProjectViewModelProvider.GetAsync();
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();
            _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
        }
    }
}
