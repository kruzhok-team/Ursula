using Fractural.Tasks;
using Godot;
using System;
using System.Threading.Tasks;
using Ursula.Core.DI;
using Ursula.Environment.Settings;
using Ursula.GameObjects.View;

namespace Ursula.GameObjects.View
{
    public partial class HUDViewModel : IInjectable, IHUDViewModel
    {
        public string nameMap;
        public string info;

        [Inject]
        private ISingletonProvider<GameObjectCommonLibraryView> _gameObjectCommonLibraryView;

        //[Inject]
        //private ISingletonProvider<GameObjectAddUserSourceModel> _gameObjectAddUserSourceModel;

        public bool IsGameObjectLibraryVisible { get; private set; } = false;
        public bool IsGameObjectAddUserSourceVisible { get; private set; } = false;   

        public event EventHandler GameObjectLibraryVisible_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public void SetNameMap(string nameMap)
        {
            //var model = _hud != null ? await _hud.GetAsync() : null;
            //model.SetNameMap(nameMap);
        }

        public void SetInfo(string info)
        {
            //var model = _hud != null ? await _hud.GetAsync() : null;
            //model.SetInfo(info);
            
        }

        public HUDViewModel SetGameObjectLibraryVisible(bool value)
        {
            IsGameObjectLibraryVisible = value;
            InvokeGameObjectLibraryVisibleEvent();
            return this;
        }

        public async void ShowCollectionView()
        {
            var model = _gameObjectCommonLibraryView != null ? await _gameObjectCommonLibraryView.GetAsync() : null;
            model?.Show();
        }

        private void InvokeGameObjectLibraryVisibleEvent()
        {
            var handler = GameObjectLibraryVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

    }
}