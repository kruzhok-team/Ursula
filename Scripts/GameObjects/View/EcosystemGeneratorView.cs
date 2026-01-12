using Fractural.Tasks;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using bearloga.addons.Ursula.Modules.LogicInjector;
using ursula.addons.Ursula.Scripts.GameObjects.Model;
using ursula.addons.Ursula.Scripts.GameObjects.Controller;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
    
    public static class EcosystemInjectorBuilder
    {
        public static Injector CreateInjector(float foodTimer, float childCount)
        {
            InjectorStateOverride initFoodTimerOverride = InitFoodTimerOverride(foodTimer);
            InjectorStateOverride foodTimerOverride = CreateFoodTimerOverride(foodTimer);
            InjectorStateOverride childCountOverride = CreateChildCountOverride(childCount);
            return new Injector(new List<InjectorStateOverride>() { initFoodTimerOverride, foodTimerOverride, childCountOverride });
        }

        private static InjectorStateOverride InitFoodTimerOverride(float foodTimer)
        {
            Random random = new Random();
            string initFoodTimer = (random.NextSingle() * foodTimer).ToString();
            
            InjectorStateCommandOverride commandOverride = new InjectorStateCommandOverride("Таймер2.ТаймерЗапуск", 0, initFoodTimer);
            InjectorStateEventOverride eventOverride = new InjectorStateEventOverride("Enter", new List<InjectorStateCommandOverride>() { commandOverride });
            return new InjectorStateOverride("[Inject] Инициализация", new List<InjectorStateEventOverride>() { eventOverride });
        }
        
        private static InjectorStateOverride CreateFoodTimerOverride(float foodTimer)
        {
            InjectorStateCommandOverride commandOverride = new InjectorStateCommandOverride("Таймер2.ТаймерЗапуск", 0, foodTimer.ToString());
            InjectorStateEventOverride eventOverride = new InjectorStateEventOverride("Enter", new List<InjectorStateCommandOverride>() { commandOverride });
            return new InjectorStateOverride("[Inject] Употребление", new List<InjectorStateEventOverride>() { eventOverride });
        }

        private static InjectorStateOverride CreateChildCountOverride(float childCount)
        {
            InjectorStateCommandOverride commandOverride = new InjectorStateCommandOverride("МодульИнтерактивныхОбъектов.Рождение", 0, childCount.ToString());
            InjectorStateEventOverride eventOverride = new InjectorStateEventOverride("Enter", new List<InjectorStateCommandOverride>() { commandOverride });
            return new InjectorStateOverride("[Inject] Рождение ребенка Ж", new List<InjectorStateEventOverride>() { eventOverride });
        }
    }
    
    public partial class EcosystemGeneratorView : Control, IInjectable
    {
        [Inject]
        protected ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        protected GameObjectCollectionModel _gameObjectCollectionModel;

        // Провайдер контроллера генерации
        [Inject]
        private ISingletonProvider<SimulationGeneratorController> _simulationGeneratorControllerProvider;
        private SimulationGeneratorController _simulationGeneratorController;

        [Export]
        public Array<EcosystemGeneratorAssetView> prefabs;

        [Export]
        public Button ButtonGenerate;

        [Export]
        public Button ButtonClear;

        private readonly List<Action<EcosystemGeneratorAssetView>> actions = new();
        private bool firstTimeOpened = false;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            if (prefabs != null)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] != null)
                        prefabs[i].clickItemEvent += OnItemClickEvent;
                }
            }

            if (ButtonGenerate != null)
                ButtonGenerate.ButtonDown += OnButtonGenerateClick;

            if (ButtonClear != null)
                ButtonClear.ButtonDown += OnButtonClearClick;

            VisibilityChanged += EcosystemGeneratorView_VisibilityChanged;
        }

        private void EcosystemGeneratorView_VisibilityChanged()
        {
            if (!firstTimeOpened)
                TryLoadDefaultAssets();
            firstTimeOpened = true;
        }

        private void TryLoadDefaultAssets()
        {
            TryLoad(prefabs[0], $"{GameObjectAssetsEmbeddedSource.LibId}.Cow", true, false);
            TryLoad(prefabs[1], $"{GameObjectAssetsEmbeddedSource.LibId}.Cow_black", false, false);
            TryLoad(prefabs[2], $"{GameObjectAssetsEmbeddedSource.LibId}.Cow_red", true, true);
            TryLoad(prefabs[3], $"{GameObjectAssetsEmbeddedSource.LibId}.Cow_dark_red", false, true);
        }

        private void TryLoad(EcosystemGeneratorAssetView assetInfoView, string id, bool isFemale, bool isHunter)
        {
            if (VoxLib.mapManager._gameObjectLibraryManager.TryGetItem(id, out IGameObjectAsset asset))
            {
                assetInfoView.Invalidate(asset.Info);
                assetInfoView.SetSex(isFemale);
                assetInfoView.SetType(isHunter);
                assetInfoView.LoadDefaultValues();
            }
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            if (prefabs != null)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] != null)
                        prefabs[i].clickItemEvent -= OnItemClickEvent;
                }
            }

            if (ButtonGenerate != null)
                ButtonGenerate.ButtonDown -= OnButtonGenerateClick;

            if (ButtonClear != null)
                ButtonClear.ButtonDown -= OnButtonClearClick;
        }

        private void OnButtonClearClick()
        {
            GD.Print("Clear Ecosystem");
            // Здесь позже можно добавить реальную очистку с помощью модели/контроллера
        }

        /// <summary>
        /// Генерируем КАЖДЫЙ объект из prefabs, у которого есть GameObjectAssetInfo
        /// и PopulationCount > 0, как 100% один тип (asset1 == asset2, percent = 100f).
        /// </summary>
        private void OnButtonGenerateClick()
        {
            if (_simulationGeneratorController == null)
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: {nameof(SimulationGeneratorController)} is not initialized.");
                return;
            }

            if (prefabs == null || prefabs.Count == 0)
            {
                GD.Print("EcosystemGeneratorView: no prefabs to generate.");
                return;
            }

            GD.Print("Generate Ecosystem");

            foreach (var view in prefabs)
            {
                if (view == null)
                    continue;

                var info = view.GameObjectAssetInfo;
                if (info == null)
                    continue;
                
                CheckAssetTemplate(info);
                
                int count = Mathf.Max(0, info.PopulationCount);
                if (count <= 0)
                    continue;

                var injector = EcosystemInjectorBuilder.CreateInjector(info.Famine, info.ChildCount);
                
                // Генерируем этот ассет как 100% один тип
                _simulationGeneratorController.GenerateSimulationItems(
                    info,   // asset1
                    info,   // asset2 тот же самый
                    count,  // количество объектов
                    100f,   // 100% первого (и единственного) типа
                    0f,      // coefficient — пока 0, при необходимости можно прокинуть из UI
                    injector
                );

                GD.Print($"EcosystemGeneratorView: generated {count} entities of {info.Name}.");
            }
        }

        private async GDTask SubscribeEvent()
        {
            if (_gameObjectCollectionModelProvider != null)
            {
                _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            }
            else
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: GameObjectCollectionModel provider is null.");
            }

            if (_simulationGeneratorControllerProvider != null)
            {
                _simulationGeneratorController = await _simulationGeneratorControllerProvider.GetAsync();
            }
            else
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: SimulationGeneratorController provider is null.");
            }
        }

        private void OnItemClickEvent(EcosystemGeneratorAssetView view)
        {
            if (_gameObjectCollectionModel == null)
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: GameObjectCollectionModel is not initialized.");
                return;
            }

            view.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        // Реализация IInjectable; логика инициализации — в SubscribeEvent()
        public void OnDependenciesInjected()
        {
        }
        
        private void CheckAssetTemplate(EcosystemGeneratorAssetInfo info)
        {
            if (info.Type == "Травоядное")
            {
                if (info.Sex == "Женский")
                {
                    info.Template.GameObjectSample = "ТравоядноеЖ";
                }
                else
                {
                    info.Template.GameObjectSample = "ТравоядноеМ";
                }
            }
            else
            {
                if (info.Sex == "Женский")
                {
                    info.Template.GameObjectSample = "ХищникЖ";
                }
                else
                {
                    info.Template.GameObjectSample = "ХищникМ";
                }
            }
        }
    }
}
