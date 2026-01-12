using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Core.UI.Constructor;
using Ursula.Core.DI;

namespace ursula.addons.Ursula.Scripts.DebugSettings.Model
{
    public partial class DebugViewModel : ConstructorViewModel, IInjectable
    {
        private bool _physicsOn;
        private bool _shapeVisibility;
        private bool _navGraphVisibility;
        private bool _logCompressOn;

        public bool PhysicsOn => _physicsOn;
        public bool ShapeVisibility => _shapeVisibility;
        public bool NavGraphVisibility => _navGraphVisibility;
        public bool LogCompressOn => _logCompressOn;

        public void SetPhysicsOn(bool value)
        {
            _physicsOn = value;
            MoveScript.IsPhysicsOn = _physicsOn;
        }

        public void SetShapeVisibility(bool value)
        {
            _shapeVisibility = value;
            InteractiveObjectDetector.IsDrawDebug = _shapeVisibility;
        }

        public void SetNavGraphVisibility(bool value)
        {
            _navGraphVisibility = value;
            if(_navGraphVisibility)
            {
                NavGraphManager.Instance.ShowDebugGraph();
            }
            else
            {
                NavGraphManager.Instance.HideDebugGraph();
            }
        }

        public void SetLogCompressOn(bool value)
        {
            _logCompressOn = value;
            HSMLogger.CompressionEnabled = _logCompressOn;
        }

        public void OnDependenciesInjected()
        {

        }
    }
}
