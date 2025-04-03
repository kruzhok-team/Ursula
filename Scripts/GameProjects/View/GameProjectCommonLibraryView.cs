using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameProjects.Model;

namespace Ursula.GameProjects.View
{
    public partial class GameProjectCommonLibraryView : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;

        private GameProjectLibraryManager _commonLibrary;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();

            _ = Load();
            _ = SubscribeEvent();
        }

        private async GDTask Load()
        {
            _commonLibrary = await _commonLibraryProvider.GetAsync();
            await _commonLibrary.Load();
        }

        private async GDTask SubscribeEvent()
        {

        }
    }
}
