using Talent.Logic.HSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace bearloga.addons.Ursula.Modules.LogicInjector
{
    public class InjectorStateCommandOverride
    {
        [JsonInclude]
        private string commandName;
        [JsonInclude]
        private int parameterIdx;
        [JsonInclude]
        private string parameterValue;

        public InjectorStateCommandOverride(string commandName, int parameterIdx, string parameterValue)
        {
            this.commandName = commandName;
            this.parameterIdx = parameterIdx;
            this.parameterValue = parameterValue;
        }

        public void TryApply(Command command)
        {
            if (command.GetCommandName() == commandName)
            {
                List<Tuple<string, string>> parameters = command.GetParameters();
                if (parameters.Count - 1 < parameterIdx)
                {
                    throw new ArgumentOutOfRangeException(nameof(parameterIdx));
                }
                Tuple<string, string> parameter = parameters[parameterIdx];
                parameter = new Tuple<string, string>(parameter.Item1, parameterValue);
                parameters[parameterIdx] = parameter;
            }
        }
    }
}
