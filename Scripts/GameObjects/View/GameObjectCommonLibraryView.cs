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

            _ = Load(null);
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

        public async GDTask Load(string _projectPath)
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            await _commonLibrary.Load(_projectPath);
        }

        private async GDTask SubscribeEvent()
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            _commonLibrary.GameObjectLibraryLoadLibraryEvent += CommonLibrary_GameObjectLibraryLoadLibrary_EventHandler;

            _hudModel = await _hudModelProvider.GetAsync();
            _hudModel.GameObjectLibraryVisibleEvent += HUDViewModel_ShowLibrary_EventHandler;

            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            _gameObjectCollectionModel.GameObjectCollectionVisibleChangeEvent += GameObjectCollectionModel_GameObjectCollectionVisibleChange_EventHandler;

            _gameObjectCollectionModel.GameObjectDrawCollectionEvent += GameObjectCollectionModel_GameObjectDrawCollection_EventHandler;
        }

        private void CommonLibrary_GameObjectLibraryLoadLibrary_EventHandler(object sender, EventArgs e)
        {
            //_ = Load();
        }

        private async GDTask DrawCommonCollection()
        {
            var commonLib = await _commonLibraryProvider.GetAsync();
            _collectionView?.Draw(commonLib.GetInfoOnGroup(_gameObjectCollectionModel.NameGameObjectGroup));
        }

        private void HUDViewModel_ShowLibrary_EventHandler(object sender, EventArgs e)
        {
            OnShow();
        }

        private void GameObjectCollectionModel_GameObjectCollectionVisibleChange_EventHandler(object sender, EventArgs e)
        {
            bool isVisible = _gameObjectCollectionModel.IsCollectionVisible;
            if (isVisible)
            {
                OnShow();
            }
        }

        async void GameObjectCollectionModel_GameObjectDrawCollection_EventHandler(object sender, EventArgs e)
        {
            await DrawCommonCollection();
        }


    }
}
