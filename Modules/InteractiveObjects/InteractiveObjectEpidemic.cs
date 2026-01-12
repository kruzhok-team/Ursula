using Fractural.Tasks;
using Godot;
using ursula.addons.Ursula.Scripts.GameObjects.Controller;
using Ursula.Core.DI;

namespace ursula.addons.Ursula.Modules.InteractiveObjects
{
    public partial class InteractiveObjectEpidemic : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<SimulationGeneratorController> _SimulationGeneratorControllerProvider;
        private SimulationGeneratorController _simulationGeneratorController;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _simulationGeneratorController = await _SimulationGeneratorControllerProvider.GetAsync();
        }

        public float InfectionCoefficient()
        {
            return _simulationGeneratorController.Coefficient;
        }

        public void OnDependenciesInjected()
        {
        }
    }
}
