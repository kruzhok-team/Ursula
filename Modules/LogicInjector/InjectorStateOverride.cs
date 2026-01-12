using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Talent.Logic.HSM;

namespace bearloga.addons.Ursula.Modules.LogicInjector
{
    public class InjectorStateOverride
    {
        [JsonInclude]
        private string stateName;
        [JsonInclude]
        private List<InjectorStateEventOverride> eventOverrides;

        public InjectorStateOverride(string stateName, List<InjectorStateEventOverride> eventOverrides)
        {
            this.stateName = stateName;
            this.eventOverrides = eventOverrides;
        }

        public void TryApply(State state)
        {
            if (eventOverrides == null)
                return;

            if (state.Label == stateName)
            {
                List<Command> enterCommands = state.EnterCommands.ToList();
                List<Command> exitCommands = state.ExitCommands.ToList();
                List<Event> events = state.Events.ToList();

                foreach (InjectorStateEventOverride eventOverride in eventOverrides)
                {
                    if (events != null)
                    {
                        foreach (Event hsmEvent in events)
                        {
                            eventOverride.TryApply(hsmEvent);
                        }
                    }

                    eventOverride.TryApplyEnter(enterCommands);
                    eventOverride.TryApplyExit(exitCommands);
                }

                state.Init(state.ID, state.Label, state.Parent, enterCommands, exitCommands, events, state.Transition, state.GetOwnerHSM());
            }
        }
    }
}
