using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.StartupMenu.Model;

namespace Ursula.StartupMenu.View
{
    public partial class StartupMenuManager : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        private StartupMenuModel _startupMenuModel { get; set; }

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
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();

            _startupMenuModel.ButtonCreateGame_EventHandler += StartupMenuModel_ButtonCreateGame_EventHandler;
            _startupMenuModel.ButtonLoadGame_EventHandler += StartupMenuModel_ButtonLoadGame_EventHandler;

            _startupMenuModel.SetStartupMenuVisible(true);
        }

        private void StartupMenuModel_ButtonCreateGame_EventHandler(object sender, EventArgs e)
        {
            
        }

        private void StartupMenuModel_ButtonLoadGame_EventHandler(object sender, EventArgs e)
        {

        }
    }
}
