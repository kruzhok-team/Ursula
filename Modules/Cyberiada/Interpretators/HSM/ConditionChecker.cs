using System;
using System.Collections.Generic;
using System.Globalization;
using Talent.Logic.Bus;
#if UNITY && DEBUG
using UnityEngine;
#endif

namespace Talent.Logic.HSM
{
    public class ConditionEventArgs : EventArgs
    {
        public KeyValuePair<string, object> leftParameter { get; set; }
        public KeyValuePair<string, object> rightParameter { get; set; }

        public string CompareSymbol { get; set; }

        public bool Result { get; set; }
    }

    /// <summary>
    /// Класс для проверки условий, основанных на переменных и параметрах
    /// </summary>
    public class ConditionChecker
    {
        private const char SeparatorChar = ' ';

        private readonly IVariableBus _bus;
        private readonly string[] _parameters;

        /// <summary>
        /// Событие при проверке условия
        /// </summary>
        public event EventHandler<ConditionEventArgs> ConditionCheckEvent;

        /// <summary>
        /// Проверяемые параметры условия
        /// </summary>
        public string Parameters => _parameters != null ? string.Join(SeparatorChar, _parameters) : "";

        /// <summary>
        /// Конструктор класс для проверки условий
        /// </summary>
        /// <param name="bus">Шина переменных</param>
        /// <param name="parameters">Список параметров, используемых условием</param>
        public ConditionChecker(IVariableBus bus, string parameters)
        {
            _bus = bus;

            if (string.IsNullOrEmpty(parameters) == false)
            {
                _parameters = parameters.Split(SeparatorChar);

                if (_parameters.Length != 3)
                {
                    throw new Exception("parameters in transition's arguments is not valid");
                }
            }
        }

        /// <summary>
        /// Проверяет условие на основе предоставленных параметров и переменных.
        /// </summary>
        /// <returns>true, если условие удовлетворено, иначе false</returns>
        public bool Check()
        {
            if (_parameters == null)
            {
                return true;
            }

            if (TryGetVariableByName(_parameters[0], out float leftValue) == false && !float.TryParse(_parameters[0], out leftValue))
            {
#if UNITY && DEBUG
                Debug.Log($"cant find variable:{_parameters[0]}");
#endif
                return false;
            }

            if (TryGetVariableByName(_parameters[2], out float rightValue) == false && !float.TryParse(_parameters[2], out rightValue))
            {
#if UNITY && DEBUG
                Debug.Log($"cant find variable{_parameters[2]}");
#endif
                return false;
            }

            var result = CompareVariables(leftValue, rightValue);

            ConditionCheckEvent?.Invoke(this, new ConditionEventArgs()
            {
                leftParameter = new(_parameters[0], leftValue),
                rightParameter = new(_parameters[2], rightValue),
                CompareSymbol = _parameters[1],
                Result = result
            });

            return result;
        }

        private bool CompareVariables(float leftValue, float rightValue)
        {
            switch (_parameters[1])
            {
                case "==":
                    return Math.Abs(leftValue - rightValue) < float.Epsilon;
                case "<":
                    return leftValue < rightValue;
                case "<=":
                    return leftValue <= rightValue;
                case ">":
                    return leftValue > rightValue;
                case ">=":
                    return leftValue >= rightValue;
                case "!=":
                    return Math.Abs(leftValue - rightValue) >= float.Epsilon;
                default:
#if UNITY && DEBUG
                        Debug.LogError($"Cant resolve parameters {_parameters[1]} and to go next state");
#endif
                    break;
            }

            return false;
        }

        private bool TryGetVariableByName(string variableName, out float variable)
        {
            variable = 0;
            if (!_bus.TryGetVariableValue(variableName, out string variableValue))
            {
                variableValue = variableName;
            }

            bool isSuccessParse = float.TryParse(variableValue, out variable);

#if UNITY && DEBUG
            if (!isSuccessParse)
            {
                Debug.LogError($"Parse is failed: {variableName} {variableValue}");
            }
#endif

            return isSuccessParse;
        }
    }
}