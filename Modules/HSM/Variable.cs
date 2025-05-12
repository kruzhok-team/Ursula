using Godot;

namespace Modules.HSM
{
    public class Variable
    {
        public string ValueOnly { get; set; }
        public string Module { get; set; }
        public string VarName { get; set; }

        public HierarchicalStateMachine gml;

        public static Variable ParseFromData(string data, HierarchicalStateMachine gml)
        {
            var variable = new Variable();
            variable.gml = gml;

            if (data.Contains('.'))
            {
                var s = data.Split('.');
                variable.Module = s[0];
                variable.VarName = s[1].Replace("/", "").Replace("\r", "");
            }
            else
            {
                variable.ValueOnly = data;
            }

            return variable;
        }

        public string GetValue()
        {
            if (!string.IsNullOrEmpty(ValueOnly))
            {
                return ValueOnly;
            }
            else if (gml.apiDictionary.HasVar(Module, VarName))
            {
                return gml.apiDictionary.VariableDictionary[Module][VarName].Value.ToString();
            }
            else
            {
                GD.Print($"Variable {ToString()} is not defined");
                return "0";
            }

        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ValueOnly))
            {
                return $"{ValueOnly}";
            }
            else
            {
                return $"[{Module}.{VarName}] {GetValue()}";
            }
        }
    }
}
