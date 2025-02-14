using Godot;
using System;
using Microsoft.Extensions.DependencyInjection;
using Ursula.Core.DI;
using Ursula.Environment.Settings;

namespace Ursula.Core.Initialization
{

    public partial class UrsulaEntryPoint : Node
    {
        private ServiceCollection _services = new ServiceCollection();
        private IServiceProvider _serviceProvider;

        public UrsulaEntryPoint()
        {
            _services = new ServiceCollection();

            InstallModel(_services);
            _serviceProvider = _services.BuildServiceProvider();

            NodeDIInjector.Init(_serviceProvider);

            var sceneTree = GetSceneTree();

            if (sceneTree != null)
                sceneTree.NodeAdded += SceneTree_NodeAddedEventHandler;
        }

        private void InstallModel(ServiceCollection services)
        {
            InstallSingleton<EnvironmentSettingsModel>(services);
        }

        private void InstallSingleton<T>(ServiceCollection services) where T : class 
        {
            services.AddSingleton<T>();
            services.AddSingleton<ISingletonProvider<T>, SingletonHolder<T>>();
        }

        private static SceneTree GetSceneTree()
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;

            if (sceneTree == null)
                GD.PrintErr($"{typeof(NodeDIInjector).Name} Can't extract a SceneTree.");
            return sceneTree;
        }

        private void SceneTree_NodeAddedEventHandler(Node node)
        {
            if (node is IInjectable injectable)
                NodeDIInjector.InjectDependencies(injectable);
        }
    }
}
