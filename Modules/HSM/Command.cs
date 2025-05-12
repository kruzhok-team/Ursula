using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Modules.HSM
{
    public class Command
    {
        public HierarchicalStateMachine gml;

        public string Name { get; set; }
        public string Method { get; set; }

        private string[] parameters;

        public static Command ParseFromData(string row, HierarchicalStateMachine gml)
        {
            row = FixFormat(row);

            var method = new Command();

            method.parameters = ExtractArguments(row);

            if (row.Contains('.'))
            {
                var s = row.Split('.');
                method.Name = s[0];
                method.Method = s[1].Replace("/", "");
            }
            else
            {
                method.Method = row.Replace("/", "");
            }

            if (method.Method.Contains("("))
                method.Method = method.Method.Split('(')[0];

            method.gml = gml;

            return method;
        }

        public void Execute()
        {
            ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вызов метода {Name}.{Method}({string.Join(',', parameters)})");

            var p1 = parameters.Length > 0 ? parameters[0] : "";
            var p2 = parameters.Length > 1 ? parameters[1] : "";

            gml.apiDictionary.GetMethod(Name, Method).Invoke(p1, p2);
        }

        public override string ToString()
        {
            return $"(Method) {Name}.{Method}";
        }

        public static string[] ExtractArguments(string input)
        {
            string pattern = @"\((.*?)\)";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string arguments = match.Groups[1].Value;

                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    return arguments.Split(',');
                }
            }

            return new string[0];
        }

        public static string FixFormat(string row)
        {
            var source = row.Replace("\r", "").Replace(".(", "(");

            var split = source.Split('(');
            var path = split[0];

            var result = path + "(";

            var values = new List<string>();
            for (int i = 1; i < split.Length; i++)
            {
                var param = split[i].Replace(")", "");
                if (param.Contains('.'))
                {
                    var value = param.Split('.');
                    values.Add(value[1]);
                }
                else if (!string.IsNullOrEmpty(param))
                    values.Add(param);
            }
            var parameters = string.Join(",", values.ToArray());
            result += parameters + ")";

            return result;
        }
    }
}
