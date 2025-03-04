using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.MapManagers.Model;


namespace Ursula.MapManagers.View
{
    public partial class MapManagerView : Node, IInjectable
    {
        [Export]
        public Control buildControl;

        [Export]
        public Control gameControl;

        [Inject]
        private ISingletonProvider<MapManagerModel> _mapManagerModelProvider;

        private MapManagerModel _mapManagerModel;

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
            _mapManagerModel = await _mapManagerModelProvider.GetAsync();
            //_AddGameObjectAssetModel.GameGameObjectAddGameObjectAssetVisible_EventHandler += GameObjectAddGameObjectAssetModel_ShowAddGameObjectAssetVisible_EventHandler;
            //_AddGameObjectAssetModel.GameObjectAddAssetToCollection_EventHandler += AddGameObjectAssetModel_GameObjectAddAssetToCollection_EventHandler;
        }


    }
}
