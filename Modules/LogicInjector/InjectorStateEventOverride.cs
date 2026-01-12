using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Talent.Logic.HSM;

namespace bearloga.addons.Ursula.Modules.LogicInjector
{
    public class InjectorStateEventOverride
    {
        [JsonInclude]
        private string eventName;
        [JsonInclude]
        private List<InjectorStateCommandOverride> commandOverrides;

        public InjectorStateEventOverride(string eventName, List<InjectorStateCommandOverride> commandOverrides)
        {
            this.eventName = eventName;
            this.commandOverrides = commandOverrides;
        }

        public void TryApply(Event hsmEvent)
        {
            if (commandOverrides == null)
                return;

            if (hsmEvent.GetName() == eventName)
            {
                TryApplyInternal(hsmEvent.GetCommand());
            }
        }

        public void TryApplyEnter(List<Command> commands)
        {
            if (commandOverrides == null)
                return;

            if (eventName == "Enter")
            {
                TryApplyInternal(commands);
            }
        }

        public void TryApplyExit(List<Command> commands)
        {
            if (commandOverrides == null)
                return;

            if (eventName == "Exit")
            {
                TryApplyInternal(commands);
            }
        }

        private void TryApplyInternal(IEnumerable<Command> commands)
        {
            if (commands == null)
                return;

            foreach (Command command in commands)
            {
                foreach (InjectorStateCommandOverride commandOverride in commandOverrides)
                {
                    commandOverride.TryApply(command);
                }
            }
        }
    }
}
