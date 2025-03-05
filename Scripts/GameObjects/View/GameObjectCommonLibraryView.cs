using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
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

        public async void OnShow()
        {
            await Show();
        }

        public async GDTask Show()
        {
            await DrawCommonCollection();
        }

        private async GDTask Load()
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            await _commonLibrary.Load();
        }

        private async GDTask SubscribeEvent()
        {
            _hudModel = await _hudModelProvider.GetAsync();
            _hudModel.GameObjectLibraryVisibleEvent += HUDViewModel_ShowLibrary_EventHandler;

            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _gameObjectCollectionModel.GameObjectCollectionVisibleChangeEvent += _gameObjectCollectionModel_GameObjectCollectionVisibleChange_EventHandler;

            _gameObjectCollectionModel.GameObjectDrawCollectionEvent += _gameObjectCollectionModel_GameObjectDrawCollection_EventHandler;
        }

        private async GDTask DrawCommonCollection()
        {
            var commonLib = await _commonLibraryProvider.GetAsync();
            _collectionView?.Draw(commonLib.GetInfo(_gameObjectCollectionModel.NameGameObjectGroup));
        }

        private void HUDViewModel_ShowLibrary_EventHandler(object sender, EventArgs e)
        {
            OnShow();
        }

        private void _gameObjectCollectionModel_GameObjectCollectionVisibleChange_EventHandler(object sender, EventArgs e)
        {
            bool isVisible = _gameObjectCollectionModel.IsCollectionVisible;
            if (isVisible)
            {
                OnShow();
            }
        }

        async void _gameObjectCollectionModel_GameObjectDrawCollection_EventHandler(object sender, EventArgs e)
        {
            await DrawCommonCollection();
        }
    }
}
