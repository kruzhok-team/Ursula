using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Modules.HSM
{
    public class GMLActionHolder
    {
        public string guid { get; private set; }
        public List<string> ConstructorStackTrace { get; private set; } = new();
        public string CallerVariableName { get; private set; } = "null (дл€ отладки укажите название переменной при создании класса nameof())";

        public GMLActionHolder()
        {
            Init();
        }

        /// <summary>
        /// ћетод предназначенный дл€ отладки ссылок
        /// </summary>
        /// <param name="thisVariableName"></param>
        public GMLActionHolder(string thisVariableName)
        {
            CallerVariableName = thisVariableName;
            Init();
        }

        private void Init()
        {
            guid = Guid.NewGuid().ToString();
            RegisterOwnerMethod();
        }

        private void RegisterOwnerMethod()
        {
            StackTrace stackTrace = new StackTrace(true);
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                StackFrame frame = stackTrace.GetFrame(i);
                MethodBase method = frame.GetMethod();
                string className = method.DeclaringType != null ? method.DeclaringType.Name : "UnknownClass";
                string methodName = method.Name;
                int lineNumber = frame.GetFileLineNumber();

                // ƒобавл€ем информацию в CallerMethodNames
                ConstructorStackTrace.Add($"Line {lineNumber}: {className}.{methodName}");
            }
        }

        public Dictionary<GMLAlgorithm.Signal, Action> registeredMethods { get; } = new();

        public void Invoke()
        {
            List<Action> linksFrom = new();
            List<Action> methodsInSignals = new();

            foreach (var invoke in registeredMethods)
            {
                if (invoke.Key.linkParent != null)
                {
                    if (invoke.Key.linkParent.From == invoke.Key.gml.currentState)
                    {
                        linksFrom.Add(invoke.Value);
                    }
                }
                else
                {
                    methodsInSignals.Add(invoke.Value);
                    //invoke.Value.Invoke();
                }
            }

            foreach (var links in methodsInSignals)
            {
                links.Invoke();
            }

            foreach (var links in linksFrom)
            {
                links.Invoke();
            }
        }

        public void Subscribe(GMLAlgorithm.Signal signal, Action action)
        {
            if (!registeredMethods.ContainsKey(signal))
                registeredMethods.Add(signal, action);
            else
                registeredMethods[signal] += action;
        }

        public void ClearSubscriptions()
        {
            registeredMethods.Clear();
        }

        public override string ToString()
        {
            var varName = "";

            if (!CallerVariableName.Contains("null"))
                varName = CallerVariableName + " ";


            return $"{varName}({guid}) [{registeredMethods.Count}]";
        }
    }
}
