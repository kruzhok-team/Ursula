using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

namespace Ursula.GameObjects.Controller
{
    public partial class GameObjectCollectionController : Node, IInjectable
    {
        [Export]
        private Control IndicatorCollectionVisible;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        private GameObjectCollectionModel _gameObjectCollectionModel;

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
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();

            IndicatorCollectionVisible.VisibilityChanged += IndicatorCollectionVisible_VisibilityChangedEventHandler;
        }

        private void IndicatorCollectionVisible_VisibilityChangedEventHandler()
        {
            _gameObjectCollectionModel.SetGameObjectCollectionVisible(IndicatorCollectionVisible.Visible);
        }


    }
}
