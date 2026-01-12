using Core.UI.Constructor;
using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ursula.addons.Ursula.Scripts.DebugSettings.Model;
using Ursula.Core.DI;

namespace ursula.addons.Ursula.Scripts.DebugSettings.View
{
    public partial class DebugView : ConstructorViewModel, IInjectable
    {
        [Export]
        CheckButton CheckButtonPhysicsOn;

        [Export]
        CheckButton CheckButtonShapeVisibility;

        [Export]
        CheckButton CheckButtonNavGraphVisibility;

        [Export]
        CheckButton CheckButtonLogCompressOn;

        [Inject]
        private ISingletonProvider<DebugViewModel> _debugViewModelProvider;
        private DebugViewModel _debugViewModel;

        public override void _Ready()
        {
            base._Ready();

            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _debugViewModel = await _debugViewModelProvider.GetAsync();
            CheckButtonPhysicsOn.Toggled += OnPhysicsOnToggledEvent;
            CheckButtonShapeVisibility.Toggled += OnShapeVisibilityToggledEvent;
            CheckButtonNavGraphVisibility.Toggled += OnCheckButtonNavGraphVisibilityToggledEvent;
            CheckButtonLogCompressOn.Toggled += OnLogCompressOnToggledEvent;
        }

        private void OnLogCompressOnToggledEvent(bool toggledOn)
        {
            _debugViewModel.SetLogCompressOn(toggledOn);
        }

        private void OnShapeVisibilityToggledEvent(bool toggledOn)
        {
            _debugViewModel.SetShapeVisibility(toggledOn);
        }

        private void OnCheckButtonNavGraphVisibilityToggledEvent(bool toggledOn)
        {
            _debugViewModel.SetNavGraphVisibility(toggledOn);
        }

        private void OnPhysicsOnToggledEvent(bool toggledOn)
        {
            _debugViewModel.SetPhysicsOn(toggledOn);
        }

        public void OnDependenciesInjected()
        {
        }
    }
}
