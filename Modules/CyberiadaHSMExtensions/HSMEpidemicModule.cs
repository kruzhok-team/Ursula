using Godot;

namespace ursula.addons.Ursula.Modules.CyberiadaHSMExtensions
{
    public partial class HSMEpidemicModule : Node
    {
        InteractiveObject _object;
        CyberiadaLogic _logic;

        const string ModuleName = "МодульЭпидемии";

        // Variables keys
        const string InfectionCoefficientVariableKey = $"{ModuleName}.КоэффициентЗаражения";

        public HSMEpidemicModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
        {
            _object = interactiveObject;
            _logic = logic;
            _logic.localBus.AddVariableGetter(InfectionCoefficientVariableKey,
                _object.epidemic.InfectionCoefficient);
        }
    }
}
