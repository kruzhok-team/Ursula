using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Model;
using Ursula.MapManagers.View;

namespace Ursula.MapManagers.Controller
{

    public partial class MapManagerController : Node, IInjectable
    {
        [Export]
        MapManagerView _mapManagerView;

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
            Init();
        }

        private async GDTask SubscribeEvent()
        {
            _mapManagerModel = await _mapManagerModelProvider.GetAsync();

            
        }

        private void Init()
        {
            this.AddChild(_mapManagerModel._mapManagerData.itemsGO);

        }
    }
}
