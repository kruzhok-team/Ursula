using Core.UI.Constructor;
using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameProjects.Model;
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

        [Export]
        private Button ButtonLoadFolderProject;

        [Inject]
        private ISingletonProvider<StartupMenuModel> _startupMenuModelProvider;

        private StartupMenuModel _startupMenuModel { get; set; }


        private FileDialogTool dialogTool;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);
        }

        private async GDTask SubscribeEvent()
        {
            _startupMenuModel = await _startupMenuModelProvider.GetAsync();

            _startupMenuModel.StartupMenuVisible_EventHandler += StartupMenuModel_StartupMenuVisible_EventHandler;
            _startupMenuModel.StartupMenuMouseFilterEvent_EventHandler += StartupMenuModel_StartupMenuMouseFilterEvent_EventHandler; ;

            ButtonClose.ButtonDown += ButtonClose_ButtonDownEvent;
            ButtonCreate.ButtonDown += ButtonCreate_ButtonDownEvent;
            ButtonLoad.ButtonDown += ButtonLoad_ButtonDownEvent;
            ButtonLoadFolderProject.ButtonDown += ButtonLoadFolderProject_ButtonDownEvent;
        }

        private void ButtonLoadFolderProject_ButtonDownEvent()
        {
            dialogTool.OpenDir((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {

                }
                else
                    GD.PrintErr($"Ошибка  открытия {path}");
            }
, FileDialog.AccessEnum.Filesystem);
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
            _startupMenuModel.SetVisibleView(false);
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
