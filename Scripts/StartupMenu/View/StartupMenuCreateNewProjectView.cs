using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.StartupMenu.Model;

namespace Ursula.StartupMenu.View
{
    public partial class StartupMenuCreateNewProjectView : StartupMenuCreateNewProjectViewModel
    {
        [Export]
        private TextEdit TextEditProjectName;

        [Export]
        private Button ButtonStartCreatingProject;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuCreateNewProjectViewModel> _startupMenuCreateNewProjectViewModelProvider;

        private StartupMenuModel _startupMenuModel;
        private StartupMenuCreateNewProjectViewModel _startupMenuCreateNewProjectViewModel;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();
            _startupMenuCreateNewProjectViewModel = await _startupMenuCreateNewProjectViewModelProvider.GetAsync();

            _startupMenuModel.ButtonCreateGame_EventHandler += (sender, args) => { ShowView(true); };

            ButtonStartCreatingProject.ButtonDown += ButtonStartCreatingProject_ButtonDownEvent;
        }



        private void ShowView(bool value)
        {
            Visible = value;
            _startupMenuCreateNewProjectViewModel.SetVisibleCreateNewProjectView(value);
        }

        private void ButtonStartCreatingProject_ButtonDownEvent()
        {
            if (TextEditProjectName.Text.Length <= 0) return;

            ShowView(false);
            _startupMenuCreateNewProjectViewModel.StartCreatingProject();

        }
    }
}
