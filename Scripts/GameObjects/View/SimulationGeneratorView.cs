using bearloga.addons.Ursula.Modules.LogicInjector;
using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ursula.addons.Ursula.Scripts.GameObjects.Controller;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.GameProjects.Model;
using Ursula.MapManagers.Setters;
using Ursula.Terrain.Model;
using Ursula.Water.Model;
using static Godot.TileSet;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
    public static class EpidemicInjectorBuilder
    {
        // type – количество типов (в твоём случае всегда 2)
        public static Injector CreateInjector(int type, float distanceHealthy, float distanceIll, int pillCount)
        {
            InjectorStateOverride generateValueOverride = GenerateValueOverride(type);
            InjectorStateOverride distanceOverrideHealthy = DistanceHealthyOverride(distanceHealthy);
            InjectorStateOverride distanceOverrideIll = DistanceIllOverride(distanceIll);
            InjectorStateOverride pillCountOverride = PillCountOverride(pillCount);

            return new Injector(new List<InjectorStateOverride>
            {
                generateValueOverride,
                distanceOverrideHealthy,
                distanceOverrideIll,
                pillCountOverride
            });
        }

        private static InjectorStateOverride PillCountOverride(int pillCount)
        {
            var commandOverride = new InjectorStateCommandOverride(
                "Счётчик1.ПрибавитьЗначение",
                0,
                pillCount.ToString()
            );

            var eventOverride = new InjectorStateEventOverride(
                "Enter",
                new List<InjectorStateCommandOverride> { commandOverride });

            return new InjectorStateOverride(
                "[Inject] Инициализация здорового",
                new List<InjectorStateEventOverride> { eventOverride });
        }

        private static InjectorStateOverride DistanceIllOverride(float distance)
        {
            var commandOverride = new InjectorStateCommandOverride(
                "ВоспроизведениеЗвука.УстановитьРадиусСлышимости",
                0,
                distance.ToString()
            );

            var eventOverride = new InjectorStateEventOverride(
                "Enter",
                new List<InjectorStateCommandOverride> { commandOverride });

            return new InjectorStateOverride(
                "[Inject] Инициализация больного",
                new List<InjectorStateEventOverride> { eventOverride });
        }

        private static InjectorStateOverride DistanceHealthyOverride(float distance)
        {
            var commandOverride = new InjectorStateCommandOverride(
                "МодульОбнаружения.ОбнаружениеВоспроизведенияЗвука",
                1,
                distance.ToString()
            );

            var eventOverride = new InjectorStateEventOverride(
                "Enter",
                new List<InjectorStateCommandOverride> { commandOverride });

            // тут, судя по названию, должен быть Init_healty, а не Init_ill
            return new InjectorStateOverride(
                "[Inject] Инициализация здорового",
                new List<InjectorStateEventOverride> { eventOverride });
        }

        private static InjectorStateOverride GenerateValueOverride(int type)
        {
            var commandOverride = new InjectorStateCommandOverride(
                "МодульСлучайности.СгенерироватьИзПромежутка",
                0,
                type.ToString()
            );

            var commandOverride2 = new InjectorStateCommandOverride(
                "МодульСлучайности.СгенерироватьИзПромежутка",
                1,
                type.ToString()
            );

            var eventOverride = new InjectorStateEventOverride(
                "Enter",
                new List<InjectorStateCommandOverride> { commandOverride, commandOverride2 });

            return new InjectorStateOverride(
                "[Inject] Выбор",
                new List<InjectorStateEventOverride> { eventOverride });
        }
    }

    public partial class SimulationGeneratorView : Control, IInjectable
    {
        [Export]
        private Slider SliderEntitiesCount;

        [Export]
        private Slider SliderPercent;

        [Export]
        private Slider SliderCoefficient;

        [Export]
        private Slider SliderDistanceHealthy;

        [Export]
        private Slider SliderDistanceIll;

        [Export]
        private Slider SliderPillCount;
        
        [Export]
        private GameObjectAssetInfoView Asset;

        [Export]
        private Button ButtonGenerate;

        [Export]
        public MapManagerItemSetter mapManagerItemSetter;
        
        [Inject]
        protected ISingletonProvider<SimulationGeneratorController> _simulationGeneratorControllerProvider;
        protected SimulationGeneratorController _simulationGeneratorController;


        [Inject]
        protected ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        protected GameObjectCollectionModel _gameObjectCollectionModel;


        [Inject]
        protected ISingletonProvider<GameObjectCreateItemsModel> _gameObjectCreateItemsModelProvider;
        protected GameObjectCreateItemsModel _gameObjectCreateItemsModel;

        [Inject]
        protected ISingletonProvider<TerrainModel> _terrainModelProvider;
        protected TerrainModel _terrainModel;

        [Inject]
        protected ISingletonProvider<TerrainManager> _terrainManagerProvider;
        protected TerrainManager _terrainManager;

        [Inject]
        protected ISingletonProvider<WaterModel> _waterModelProvider;
        protected WaterModel _waterModel;

        private bool firstTimeOpened = false;


        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            Asset.clickItemEvent += OnAssetClickEvent;

            ButtonGenerate.ButtonDown += OnButtonGenerateClickEvent;
            VisibilityChanged += SimulationGeneratorView_VisibilityChanged;
        }

        private void SimulationGeneratorView_VisibilityChanged()
        {
            if (!firstTimeOpened)
                TryLoad(Asset, $"{GameObjectAssetsEmbeddedSource.LibId}.Cow");
            firstTimeOpened = true;
        }

        private void TryLoad(GameObjectAssetInfoView assetInfoView, string id)
        {
            if (VoxLib.mapManager._gameObjectLibraryManager.TryGetItem(id, out IGameObjectAsset asset))
            {
                assetInfoView.Invalidate(asset.Info);
            }
        }

        private void OnButtonClearClickEvent()
        {
            throw new NotImplementedException();
        }

        private async GDTask SubscribeEvent()
        {
            _simulationGeneratorController = await _simulationGeneratorControllerProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            
            _gameObjectCreateItemsModel = await _gameObjectCreateItemsModelProvider.GetAsync();
            _terrainModel = await _terrainModelProvider.GetAsync();
            _terrainManager = await _terrainManagerProvider.GetAsync();
            _waterModel = await _waterModelProvider.GetAsync();

            _simulationGeneratorController.Init(
                _gameObjectCreateItemsModel,
                _gameObjectCollectionModel,
                mapManagerItemSetter,
                _terrainModel,
                _terrainManager,
                _waterModel);
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonGenerate.ButtonDown -= OnButtonGenerateClickEvent;
        }

        private void OnButtonGenerateClickEvent()
        {
            int entitiesCount = Convert.ToInt32(SliderEntitiesCount.Value);
            float percent = Convert.ToSingle(SliderPercent.Value);        // процент здоровых
            float coefficient = Convert.ToSingle(SliderCoefficient.Value);
            float distanceHealthy = Convert.ToSingle(SliderDistanceHealthy.Value);
            float distanceIll = Convert.ToSingle(SliderDistanceIll.Value);
            int pillCount = Convert.ToInt32(SliderPillCount.Value);

            entitiesCount = Mathf.Max(0, entitiesCount);
            percent = Mathf.Clamp(percent, 0f, 100f);

            // Считаем, сколько будет здоровых и больных
            int healthyCount = Mathf.RoundToInt(entitiesCount * ((100 - percent) * 0.01f));
            healthyCount = Mathf.Clamp(healthyCount, 0, entitiesCount);
            int illCount = entitiesCount - healthyCount;

            GD.Print(
                $"Generate Simulation with params: " +
                $"entitiesCount={entitiesCount}, percent={percent}, coefficient={coefficient}, " +
                $"healthy={healthyCount}, ill={illCount}, " +
                $"distanceHealthy={distanceHealthy}, distanceIll={distanceIll}, pillCount={pillCount}"
            );

            // Инжектор для здоровых (тип 0)
            var healthyInjector = EpidemicInjectorBuilder.CreateInjector(
                type: 0,
                distanceHealthy: distanceHealthy,
                distanceIll: distanceIll,
                pillCount: pillCount
            );

            // Инжектор для больных (тип 1)
            var illInjector = EpidemicInjectorBuilder.CreateInjector(
                type: 1,
                distanceHealthy: distanceHealthy,
                distanceIll: distanceIll,
                pillCount: pillCount
            );

            // Генерация здоровых
            if (healthyCount > 0)
            {
                _simulationGeneratorController.GenerateSimulationItems(
                    Asset.GameObjectAssetInfo,   // используем один и тот же Asset
                    null,
                    healthyCount,
                    100f,                        // тут все объекты будут здоровыми
                    coefficient,
                    healthyInjector
                );
            }

            // Генерация больных
            if (illCount > 0)
            {
                _simulationGeneratorController.GenerateSimulationItems(
                    Asset.GameObjectAssetInfo,   // тот же Asset, другое поведение через Injector
                    null,
                    illCount,
                    100f,                        // тут все объекты будут больными
                    coefficient,
                    illInjector
                );
            }
        }


        private void OnAssetClickEvent(GameObjectAssetInfo info)
        {
            Asset.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }
    }
}
