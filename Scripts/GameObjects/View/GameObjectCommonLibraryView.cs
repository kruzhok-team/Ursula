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

        private GameObjectLibraryManager _commonLibrary;
        private HUDViewModel _hudModel;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();

            //_ = Load();
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
            _hudModel.ButtonShowLibraryEvent += HUDViewModel_ButtonShowLibraryEventHandler;
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
            _collectionView?.Draw(commonLib.GetAll());
        }



        private void HUDViewModel_ButtonShowLibraryEventHandler(object sender, EventArgs e)
        {
            OnShow();
        }
    }
}
