using System;

namespace Modules.HSM
{
    public class Condition
    {
        public Variable LeftVar { get; set; }
        public Variable RightVar { get; set; }
        public string Operator { get; set; }

        public HierarchicalStateMachine gml;

        public override string ToString()
        {
            return $"{LeftVar} {Operator} {RightVar}";
        }

        public static Condition ParseFromData(string condition, HierarchicalStateMachine gml)
        {
            var result = new Condition();
            result.gml = gml;

            var formated = condition.Replace("/", "").Replace("]", "").Replace(" ", "");
            var splited = SplitByEx(formated);

            result.LeftVar = Variable.ParseFromData(splited.splits[0], gml);
            result.RightVar = Variable.ParseFromData(splited.splits[1], gml);

            result.Operator = splited.oper;

            return result;
        }

        private static (string[] splits, string oper) SplitByEx(string row)
        {
            string[] operators = new[] { "!=", ">=", "<=", ">", "<", "=" };

            foreach (var op in operators)
            {
                if (row.Contains(op))
                {
                    return (row.Split(op), op);
                }
            }

            return (null, null);
        }

        public bool EvaluateCondition()
        {
            var v1 = LeftVar.GetValue();
            var v2 = RightVar.GetValue();
            var value1 = float.Parse(v1);
            var value2 = float.Parse(v2);

            switch (Operator)
            {
                case "!=":
                    return !value1.Equals(value2);
                case ">=":
                    return value1 >= value2;
                case "<=":
                    return value1 <= value2;
                case ">":
                    return value1 > value2;
                case "<":
                    return value1 < value2;
                case "=":
                    return value1 == value2;
                default:
                    throw new ArgumentException("Unsupported operator: " + Operator);
            }
        }
    }
}
