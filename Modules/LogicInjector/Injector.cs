using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Talent.Logic.HSM;

namespace bearloga.addons.Ursula.Modules.LogicInjector
{
    public class Injector
    {
        [JsonInclude]
        private List<InjectorStateOverride> stateOverrides;

        public Injector(List<InjectorStateOverride> stateOverrides)
        {
            this.stateOverrides = stateOverrides;
        }

        public void TryApply(State parentState)
        {
            if (stateOverrides == null)
                return;

            foreach (InjectorStateOverride stateOverride in stateOverrides)
            {
                stateOverride.TryApply(parentState);
            }

            if (parentState.ChildStates == null)
                return;

            foreach (State child in parentState.ChildStates)
            {
                TryApply(child);
            }
        }
    }
}
