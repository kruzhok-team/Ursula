using Core.UI.Constructor;
using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.StartupMenu.Model;

namespace Ursula.StartupMenu.View
{
    public partial class StartupMenuView : StartupMenuModel, IDisposable
    {
        [Export]
        Button ButtonClose;

        [Export]
        Button ButtonCreate;

        [Export]
        Button ButtonLoad;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        private StartupMenuModel _startupMenuModel { get; set; }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();

            _startupMenuModel.StartupMenuVisible_EventHandler += StartupMenuModel_StartupMenuVisible_EventHandler;
            _startupMenuModel.StartupMenuMouseFilterEvent_EventHandler += StartupMenuModel_StartupMenuMouseFilterEvent_EventHandler; ;

            ButtonClose.ButtonDown += ButtonClose_ButtonDownEvent;
            ButtonCreate.ButtonDown += ButtonCreate_ButtonDownEvent;
            ButtonLoad.ButtonDown += ButtonLoad_ButtonDownEvent;
        }

        private void StartupMenuModel_StartupMenuMouseFilterEvent_EventHandler(object sender, EventArgs e)
        {

            //this.MouseFilter = Control.MouseFilterEnum.Ignore;
            //this.ReleaseFocus();
        }

        public override void _ExitTree()
        {
            Dispose();
        }

        public new void Dispose()
        {
            base.Dispose();
            ButtonCreate?.Dispose();
            ButtonLoad?.Dispose();
        }

        private void ButtonClose_ButtonDownEvent()
        {
            Visible = false;
            _startupMenuModel.SetStartupMenuVisible(false);
        }

        private void ButtonCreate_ButtonDownEvent()
        {
            _startupMenuModel.SetCreateGame();
        }

        private void ButtonLoad_ButtonDownEvent()
        {
            _startupMenuModel.SetLoadGame();
        }

        private void StartupMenuModel_StartupMenuVisible_EventHandler(object sender, EventArgs e)
        {
            Visible = _startupMenuModel.Visible;
        }
    }
}
