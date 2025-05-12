using Godot;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Ursula.Core.DI
{
    public static class NodeDIInjector
    {
        public static bool IsInitialized = false;

        private static IServiceProvider _serviceProvider;

        public static void Init(IServiceProvider serviceProvider)
        {
            if (IsInitialized)
            {
                GD.PrintErr($"{typeof(NodeDIInjector).Name} already initialized!");
                return;
            }

            _serviceProvider = serviceProvider;
            IsInitialized = true;
        }

        public static void InjectDependencies(IInjectable injectable)
        {
            if (!IsInitialized)
            {
                GD.PrintErr($"{typeof(NodeDIInjector).Name} must be initialized first!");
                return;
            }

            if (_serviceProvider == null)
            {
                GD.PrintErr("The reference to the ServiceProvider is null!");
                return;
            }

            var theType = injectable.GetType();
            var fields = theType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var attributeFound = false;

            foreach (var field in fields)
            {
                var attribute = (Inject)Attribute.GetCustomAttribute(field, typeof(Inject));

                if (attribute != null)
                {
                    attributeFound = true;
                    var service = _serviceProvider.GetService(field.FieldType);
                    field.SetValue(injectable, service);
                }                
            }

            if (attributeFound)
                injectable.OnDependenciesInjected();
        }
    }
}
