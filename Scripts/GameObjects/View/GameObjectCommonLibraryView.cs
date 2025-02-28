using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.Environment.Settings;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    public partial class GameObjectCommonLibraryView : Node, IInjectable
    {
        [Export]
        private GameObjectCollectionView _collectionView;
        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;
        [Inject]
        private ISingletonProvider<HUDViewModel> _hudModelProvider;
        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;      

        private GameObjectLibraryManager _commonLibrary;
        private HUDViewModel _hudModel;
        private GameObjectCollectionModel _gameObjectCollectionModel;

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
            _hudModel = await _hudModelProvider.GetAsync();
            _hudModel.GameObjectLibraryVisible_EventHandler += HUDViewModel_ShowLibraryEventHandler;

            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _gameObjectCollectionModel.GameObjectCollectionVisibleChangeEvent += _gameObjectCollectionModel_GameObjectCollectionVisibleChangeEventHandler;
        }

        public async void OnShow()
        {
            await Show();
        }

        public async GDTask Show()
        {
            await DrawCommonCollection();
        }

        private async GDTask DrawCommonCollection()
        {
            var commonLib = await _commonLibraryProvider.GetAsync();
            _collectionView?.Draw(commonLib.GetAllInfo());
        }

        private void HUDViewModel_ShowLibraryEventHandler(object sender, EventArgs e)
        {
            OnShow();
        }

        private void _gameObjectCollectionModel_GameObjectCollectionVisibleChangeEventHandler(object sender, EventArgs e)
        {
            bool isVisible = _gameObjectCollectionModel.IsCollectionVisible;
            if (isVisible)
            {
                OnShow();
            }
        }
    }
}
