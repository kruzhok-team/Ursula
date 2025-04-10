using Fractural.Tasks;
using Godot;
using System;
using Ursula.ConstructorMenu.Model;
using Ursula.Core.DI;
using Ursula.GameProjects.Model;
using Ursula.StartupMenu.Model;


namespace Ursula.ConstructorMenu.View
{
    public partial class ConstructorMenuFileView : ConstructorMenuFileModel
    {
        [Export]
        Button ButtonCreateNewProject;

        [Export]
        Button ButtonLoadProject;

        [Export]
        Button ButtonSaveProject;

        [Export]
        Button ButtonExportProject;

        [Export]
        Button ButtonAboutProgram;

        [Export]
        Button ButtonSupport;

        [Inject]
        private ISingletonProvider<ConstructorMenuFileModel> _constructorMenuFileModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;


        private ConstructorMenuFileModel _constructorMenuFileModel { get; set; }
        private StartupMenuModel _startupMenuModel { get; set; }
        private GameProjectLibraryManager _commonLibrary { get; set; }


        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _constructorMenuFileModel = await _constructorMenuFileModelProvider.GetAsync();
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();
            _commonLibrary = await _commonLibraryProvider.GetAsync();

            ButtonCreateNewProject.ButtonDown += ButtonCreateNewProject_ButtonDownEvent;
            ButtonLoadProject.ButtonDown += ButtonLoadProject_ButtonDownEvent;
            ButtonSaveProject.ButtonDown += ButtonSaveProject_ButtonDownEvent;
            ButtonExportProject.ButtonDown += ButtonExportProject_ButtonDownEvent;
            ButtonAboutProgram.ButtonDown += ButtonAboutProgram_ButtonDownEvent;
            ButtonSupport.ButtonDown += ButtonSupport_ButtonDownEvent;
        }

        private void ButtonCreateNewProject_ButtonDownEvent()
        {
            _startupMenuModel?.SetVisibleView(true);
            _startupMenuModel?.SetCreateGame();
        }

        private void ButtonLoadProject_ButtonDownEvent()
        {
            _startupMenuModel?.SetVisibleView(true);
            _startupMenuModel?.SetLoadGame();
        }

        private void ButtonSaveProject_ButtonDownEvent()
        {
            _commonLibrary.currentProjectInfo?.SaveMap();
        }

        private void ButtonExportProject_ButtonDownEvent()
        {
            _commonLibrary.currentProjectInfo?.ExportProject();
        }

        private void ButtonAboutProgram_ButtonDownEvent()
        {
            throw new NotImplementedException();
        }

        private void ButtonSupport_ButtonDownEvent()
        {
            throw new NotImplementedException();
        }










    }
}
