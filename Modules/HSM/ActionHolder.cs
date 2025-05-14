using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;


namespace Modules.HSM
{
    public class ActionHolder
    {
        public string guid { get; private set; }
        public List<string> ConstructorStackTrace { get; private set; } = new();
        public string CallerVariableName { get; private set; } = "null (для отладки укажите название переменной при создании класса nameof())";

        public ActionHolder()
        {
            Init();
        }

        /// <summary>
        /// Метод предназначенный для отладки ссылок
        /// </summary>
        /// <param name="thisVariableName"></param>
        public ActionHolder(string thisVariableName)
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

                // Добавляем информацию в CallerMethodNames
                ConstructorStackTrace.Add($"Line {lineNumber}: {className}.{methodName}");
            }
        }

        public Dictionary<Event, Action> registeredMethods { get; } = new();

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

        public void Subscribe(Event ev, Action action)
        {
            if (!registeredMethods.ContainsKey(ev))
                registeredMethods.Add(ev, action);
            else
                registeredMethods[ev] += action;
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
