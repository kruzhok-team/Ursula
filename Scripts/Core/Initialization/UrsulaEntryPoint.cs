using Godot;
using System;
using Microsoft.Extensions.DependencyInjection;
using Ursula.Core.DI;
using Ursula.Environment.Settings;
using Ursula.GameObjects.View;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Controller;
using Ursula.MapManagers.Model;
using Ursula.GameObjects.Controller;
using Ursula.StartupMenu.Model;
using Ursula.GameProjects.Model;
using Ursula.Terrain.Model;
using Ursula.ConstructorMenu.Model;
using Ursula.Water.Model;
using Ursula.MapManagers.Setters;
using Ursula.Settings.Model;


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

        public override void _Ready()
        {
            base._Ready();
        }

        private void InstallModel(ServiceCollection services)
        {
            InstallSingleton<EnvironmentSettingsModel>(services);
            InstallSingleton<ControlSettingsViewModel>(services);            

            InstallSingleton<MapManager>(services);
            InstallSingleton<MapManagerController>(services);
            InstallSingleton<MapManagerModel>(services);

            InstallSingleton<GameObjectCommonLibraryView>(services);
            InstallSingleton<GameObjectLibraryManager>(services);
            InstallSingleton<GameObjectAssetsUserSource>(services);
            InstallSingleton<GameObjectAssetsEmbeddedSource>(services);

            InstallSingleton<HUDViewModel>(services);

            InstallSingleton<GameObjectAddGameObjectAssetModel>(services);
            InstallSingleton<GameObjectAddGameObjectAssetView>(services);
            InstallSingleton<GameObjectAddGameObjectAssetUiView>(services);

            InstallSingleton<GameObjectCollectionModel>(services);
            InstallSingleton<GameObjectCollectionView>(services);

            InstallSingleton<GameObjectCreateItemsController>(services);
            InstallSingleton<GameObjectCreateItemsModel>(services);

            InstallSingleton<GameObjectCurrentInfoManager>(services);
            InstallSingleton<GameObjectCurrentInfoModel>(services);

            InstallSingleton<StartupMenuModel>(services);
            InstallSingleton<StartupMenuCreateNewProjectViewModel>(services);
            InstallSingleton<StartupMenuCreateGameViewModel>(services);
            InstallSingleton<StartupMenuCreateGameManager>(services);
            
            InstallSingleton<GameProjectLibraryManager>(services);
            InstallSingleton<GameProjectAssetsUserSource>(services);
            InstallSingleton<GameProjectAssetsEmbeddedSource>(services);

            InstallSingleton<GameProjectCollectionViewModel>(services);

            InstallSingleton<TerrainManager>(services);
            InstallSingleton<TerrainModel>(services);

            InstallSingleton<ConstructorMenuFileModel>(services);
            InstallSingleton<ConstructorLandscapeMenuViewModel>(services);

            InstallSingleton<WaterManager>(services);
            InstallSingleton<WaterModel>(services);
            InstallSingleton<ConstructorWaterMenuViewModel>(services);

            InstallSingleton<ConstructorFloraMenuViewModel>(services);



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
