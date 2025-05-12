using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameProjects.Model;
using Ursula.GameObjects.Model;
using Ursula.StartupMenu.Model;
using Ursula.EmbeddedGames.Model;
using Ursula.EmbeddedGames.Manager;
using static Godot.HttpRequest;

namespace Ursula.EmbeddedGames.View
{
    public partial class ControlEmbeddedGamesProjectView : ControlEmbeddedGamesProjectViewModel, IInjectable
    {
        [Export]
        public PackedScene importGameItemPrefab;

        [Export]
        public VBoxContainer container;

        [Export]
        public Button ButtonClose;

        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<ControlEmbeddedGamesProjectViewModel> _controlEmbeddedGamesProjectViewModelProvider;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<ControlEmbeddedGamesProjectManager> _controlEmbeddedGamesProjectManagerProvider;

        private GameProjectLibraryManager _commonLibrary { get; set; }
        private ControlEmbeddedGamesProjectViewModel _controlEmbeddedGamesProjectViewModel { get; set; }
        private StartupMenuModel _startupMenuModel { get; set; }
        private GameObjectLibraryManager _gameObjectLibraryManager { get; set; }
        private ControlEmbeddedGamesProjectManager _controlEmbeddedGamesProjectManager { get; set; }

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
            _ = OnShow();

            IReadOnlyCollection<GameProjectAssetInfo> assets = _commonLibrary.GetEmbeddedInfo();
            if (assets.Count == 0) _SetVisibleView(false);
        }

        private async GDTask SubscribeEvent()
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _controlEmbeddedGamesProjectViewModel = await _controlEmbeddedGamesProjectViewModelProvider.GetAsync();
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();
            _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
            _controlEmbeddedGamesProjectManager = await _controlEmbeddedGamesProjectManagerProvider.GetAsync();

            _controlEmbeddedGamesProjectViewModel.ViewVisible_EventHandler += ControlEmbeddedGamesProjectViewModel_ViewVisible_EventHandler;

            ButtonClose.ButtonDown += ButtonClose_ButtonDownEvent;
        }

        private async void ControlEmbeddedGamesProjectViewModel_ViewVisible_EventHandler(object sender, EventArgs e)
        {
            Visible = _controlEmbeddedGamesProjectViewModel.Visible;
            if (Visible == true) await OnShow();
        }

        public async GDTask OnShow()
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            Visible = true;

            await OnDraw(_commonLibrary.GetEmbeddedInfo());
        }

        private async GDTask OnDraw(IReadOnlyCollection<GameProjectAssetInfo> assets)
        {
            VoxLib.RemoveAllChildren(container);

            List<GameProjectAssetInfo> result = new List<GameProjectAssetInfo>(assets);

            for (int i = 0; i < result.Count; i++)
            {
                Node instance = importGameItemPrefab.Instantiate();
                ControlGameItemView item = instance as ControlGameItemView;

                if (item == null)
                    continue;

                item.clickItemEvent += ClickItem_PlayGameEvent;
                item.Generate(result[i]);

                container.AddChild(instance);
            }
        }

        private async void ClickItem_PlayGameEvent(string itemId)
        {
            GameProjectAssetInfo game = _commonLibrary?.GetItemInfo(itemId);

            if (game == null) return;

            _commonLibrary.SetLoadProject(game);
            await _gameObjectLibraryManager.Load(game.GetProjectPath());

            game?.PlayGame();

            _controlEmbeddedGamesProjectViewModel.SetVisibleView(false);
            _startupMenuModel.SetVisibleView(false);

            //_controlEmbeddedGamesProjectManager?.PlayGame(itemId);
        }

        private void ButtonClose_ButtonDownEvent()
        {
            _SetVisibleView(false);
        }

        private void _SetVisibleView(bool value)
        {
            _controlEmbeddedGamesProjectViewModel.SetVisibleView(value);
        }
    }
}
