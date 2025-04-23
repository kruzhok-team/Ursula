using Fractural.Tasks;
using Godot;
using System;
using Ursula.ConstructorMenu.Model;
using Ursula.Core.DI;
using Ursula.EmbeddedGames.Model;
using Ursula.Settings.Model;
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

        [Export]
        Button ButtonSettings;

        [Export]
        Button ButtonGames;

        [Inject]
        private ISingletonProvider<ConstructorMenuFileModel> _constructorMenuFileModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<ControlSettingsViewModel> _controlSettingsViewModelProvider;

        [Inject]
        private ISingletonProvider<ControlEmbeddedGamesProjectViewModel> _controlEmbeddedGamesProjectViewModelProvider;

        private ConstructorMenuFileModel _constructorMenuFileModel { get; set; }
        private StartupMenuModel _startupMenuModel { get; set; }
        private GameProjectLibraryManager _commonLibrary { get; set; }
        private ControlSettingsViewModel _controlSettingsViewModel { get; set; }
        private ControlEmbeddedGamesProjectViewModel _controlEmbeddedGamesProjectViewModel { get; set; }

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
            _controlSettingsViewModel = await _controlSettingsViewModelProvider.GetAsync();
            _controlEmbeddedGamesProjectViewModel = await _controlEmbeddedGamesProjectViewModelProvider.GetAsync();

            ButtonCreateNewProject.ButtonDown += ButtonCreateNewProject_ButtonDownEvent;
            ButtonLoadProject.ButtonDown += ButtonLoadProject_ButtonDownEvent;
            ButtonSaveProject.ButtonDown += ButtonSaveProject_ButtonDownEvent;
            ButtonExportProject.ButtonDown += ButtonExportProject_ButtonDownEvent;
            ButtonAboutProgram.ButtonDown += ButtonAboutProgram_ButtonDownEvent;
            ButtonSupport.ButtonDown += ButtonSupport_ButtonDownEvent;
            ButtonSettings.ButtonDown += ButtonSettings_ButtonDownEvent;
            ButtonGames.ButtonDown += ButtonGames_ButtonDownEvent;
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
            _commonLibrary?.currentProjectInfo?.SaveMap();
        }

        private void ButtonExportProject_ButtonDownEvent()
        {
            _commonLibrary?.currentProjectInfo?.ExportProject();
        }

        private void ButtonAboutProgram_ButtonDownEvent()
        {
            throw new NotImplementedException();
        }

        private void ButtonGames_ButtonDownEvent()
        {
            _controlEmbeddedGamesProjectViewModel.SetVisibleView(true);
        }

        private void ButtonSupport_ButtonDownEvent()
        {
            OpenEmailClient(
            to: "platform@kruzhok.org",
            subject: "Тема письма",
            body: "Это текст письма."
        );
        }

        private void ButtonSettings_ButtonDownEvent()
        {
            _controlSettingsViewModel?.SetShowVisibleView(true);
        }

        private void OpenEmailClient(string to, string subject, string body)
        {
            // Создаем mailto URI
            string mailtoUri = $"mailto:{to}?subject={System.Uri.EscapeDataString(subject)}&body={System.Uri.EscapeDataString(body)}";

            // Открываем почтовый клиент
            OS.ShellOpen(mailtoUri);
        }








    }
}
