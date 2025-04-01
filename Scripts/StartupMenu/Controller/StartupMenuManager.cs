using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.GameProjects.Model;
using Ursula.StartupMenu.Model;

namespace Ursula.StartupMenu.View
{
    public partial class StartupMenuManager : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateNewProjectViewModel> _startupMenuCreateNewProjectViewModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateGameViewModel> _startupMenuCreateGameViewModelProvider;

        [Inject]
        private ISingletonProvider<GameProjectCollectionViewModel> _gameProjectCollectionViewModelProvider;

        private StartupMenuModel _startupMenuModel { get; set; }
        private StartupMenuCreateNewProjectViewModel _startupMenuCreateNewProjectViewModel { get; set; }
        private StartupMenuCreateGameViewModel _startupMenuCreateGameViewModel { get; set; }
        private GameProjectCollectionViewModel _gameProjectCollectionViewModel { get; set; }

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
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();
            _startupMenuCreateNewProjectViewModel = await _startupMenuCreateNewProjectViewModelProvider.GetAsync();
            _startupMenuCreateGameViewModel = await _startupMenuCreateGameViewModelProvider.GetAsync();
            _gameProjectCollectionViewModel = await _gameProjectCollectionViewModelProvider.GetAsync();

            _startupMenuModel.ButtonCreateGame_EventHandler += StartupMenuModel_ButtonCreateGame_EventHandler;
            _startupMenuModel.ButtonLoadGame_EventHandler += StartupMenuModel_ButtonLoadGame_EventHandler;

            _startupMenuCreateNewProjectViewModel.StartCreatingProject_EventHandler += StartupMenuCreateNewProjectView_ButtonStartCreatingProject_EventHandler;

            _startupMenuModel.SetStartupMenuVisible(true);
        }



        private void StartupMenuModel_ButtonCreateGame_EventHandler(object sender, EventArgs e)
        {
            _startupMenuCreateNewProjectViewModel.SetVisibleView(true);
            _startupMenuCreateGameViewModel.SetVisibleView(false);
            _gameProjectCollectionViewModel.SetVisibleView(false);
        }

        private void StartupMenuModel_ButtonLoadGame_EventHandler(object sender, EventArgs e)
        {
            _startupMenuCreateNewProjectViewModel.SetVisibleView(false);
            _startupMenuCreateGameViewModel.SetVisibleView(false);
            _gameProjectCollectionViewModel.SetVisibleView(true);
        }

        private void StartupMenuCreateNewProjectView_ButtonStartCreatingProject_EventHandler(object sender, EventArgs e)
        {
            _startupMenuCreateNewProjectViewModel.SetVisibleView(false);
            _startupMenuCreateGameViewModel.SetVisibleView(true);
            _gameProjectCollectionViewModel.SetVisibleView(false);
        }
    }
}
