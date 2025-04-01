using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using Ursula.Core.DI;
using Ursula.GameProjects.Model;

namespace Ursula.GameProjects.View
{
    public partial class GameProjectCollectionView : Control, IInjectable
    {
        [Export]
        private VBoxContainer VBoxContainerCollectionView;

        [Export]
        PackedScene GameProjectAssetInfoPrefab;

        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<GameProjectCollectionViewModel> _gameProjectCollectionViewModelProvider;
        

        private GameProjectLibraryManager _commonLibrary;
        private GameProjectCollectionViewModel _gameProjectCollectionViewModel;

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
            _gameProjectCollectionViewModel = await _gameProjectCollectionViewModelProvider.GetAsync();

            _gameProjectCollectionViewModel.ViewVisible_EventHandler += GameProjectCollectionViewModel_ViewVisible_EventHandler;
        }

        private void GameProjectCollectionViewModel_ViewVisible_EventHandler(object sender, EventArgs e)
        {
            Visible = _gameProjectCollectionViewModel.Visible;
        }

        public async GDTask Show()
        {
            await Draw(_commonLibrary.GetAllInfo());
        }

        private async GDTask Draw(IReadOnlyCollection<GameProjectAssetInfo> assets)
        {
            VoxLib.RemoveAllChildren(VBoxContainerCollectionView);

            List<GameProjectAssetInfo> result = new List<GameProjectAssetInfo>(assets);

            for (int i = 0; i < result.Count; i++)
            {
                Node instance = GameProjectAssetInfoPrefab.Instantiate();
                GameProjectLoadProjectInfoView item = instance as GameProjectLoadProjectInfoView;

                if (item == null)
                    continue;

                item.clickItemEvent += ClickItem_SelectEvent;
                item.Invalidate(result[i]);

                VBoxContainerCollectionView.AddChild(instance);
            }
        }

        private void ClickItem_SelectEvent(GameProjectAssetInfo asset)
        {
            _commonLibrary.LoadProject(asset);
        }
    }
}
