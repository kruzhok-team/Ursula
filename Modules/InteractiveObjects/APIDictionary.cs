using System;
using System.Collections.Generic;
using Godot;

public class APIDictionary
{
    public InteractiveObject _interactiveObject { get; private set; }

    private Dictionary<string, Dictionary<string, Func<string, string, object>>> MethodDictionary;
    public Dictionary<string, Dictionary<string, GMLActionHolder>> SignalDictionary;
    public Dictionary<string, Dictionary<string, VariableHolder<float>>> VariableDictionary;

    public bool HasVar(string moduleName, string name)
    {
        if (VariableDictionary.ContainsKey(moduleName))
        {
            if (VariableDictionary[moduleName].ContainsKey(name))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasSignal(string moduleName, string signalName)
    {
        if (signalName == "entry" || signalName == "exit") return false;

        if (SignalDictionary.ContainsKey(moduleName))
        {
            if (SignalDictionary[moduleName].ContainsKey(signalName)) {
                return true;
            }
            ContextMenu.ShowMessageS($"Сигнал {moduleName}.{signalName} не зарегистрирован");
        }

        ContextMenu.ShowMessageS($"Модуль {moduleName} не зарегистрирован для сигналов");

        return false;
    }

    public Func<string, string, object> GetMethod(string moduleName, string methodName)
    {
        if (MethodDictionary.ContainsKey(moduleName))
        {
            if (MethodDictionary[moduleName].ContainsKey(methodName))
            {
                return MethodDictionary[moduleName][methodName];
            }
            ContextMenu.ShowMessageS($"Метод {moduleName}.{methodName} не зарегистрирован");
        }

        ContextMenu.ShowMessageS($"Модуль {moduleName} не зарегистрирован для методов");

        return (_,__) => { return _; };
    }

    public APIDictionary(InteractiveObject interactiveObject)
    {
        _interactiveObject = interactiveObject;

        SignalDictionary = new()
        {
            {"МодульОбнаружения", new() {
                { "ИгрокОбнаружен", _interactiveObject.detector.onPlayerDetected },
                { "ОбъектОбнаружен", _interactiveObject.detector.onObjectDetected },
                { "ЗвукОбнаружен", _interactiveObject.detector.onSoundDetected },
                { "ЦельПотеряна", _interactiveObject.move.moveScript?.onTargetLost },
                { "ВзаимодействиеИгрока", _interactiveObject.onThisInteraction },
                { "ВзаимодействиеИгрокаСОбъектом", _interactiveObject.detector.onPlayerInteractionObject },
                { "ОбъектовНеОбнаружено", _interactiveObject.detector.onAnyObjectsNotDetected },
            }},
            {"МодульДвижения", new() {
                { "ЗастреваниеПриДвижении", _interactiveObject.move.moveScript?.onStuckMoving },
                { "ПеремещениеВыполнено", _interactiveObject.move.moveScript?.onMovementFinished },
                { "СтолкновениеСПрепятствием", _interactiveObject.move.moveScript?.onCollision },
                { "ПройденноеРасстояние", _interactiveObject.move.moveScript?.onMovingDistanceFinished },
                { "АнимацияЗавершилась", _interactiveObject.move.animationCompleted },
                { "ЦиклАнимацийЗавершился", _interactiveObject.move.animationCompleted },
            }},
            {"МодульАнимаций", new() {
                { "АнимацияЗавершилась", _interactiveObject.move.animationCompleted },
                { "ЦиклАнимацийЗавершился", _interactiveObject.move.animationCompleted },
            }},
            {"ВзаимодействиеСМиром", new() {
                { "СменаТипаПоверхности", _interactiveObject.move.moveScript?.onChangeSurfaceType },
            }},
            {"Таймер", new() {
                { "ТаймерВыполнен", _interactiveObject.timer.TimerCompleted },
                { "Тик", ProjectTimer.Instance.Tick },
                { "Тик1Секунда", ProjectTimer.Instance.TickOneSecond },
            }},
            {"Счётчик1", new() {
                { "ЗначениеИзменилось", _interactiveObject.counter1.onValueChanged },
            }},
            {"Счётчик2", new() {
                { "ЗначениеИзменилось", _interactiveObject.counter2.onValueChanged },
            }},
        };

        MethodDictionary = new()
        {
            {"МодульОбнаружения", new() {
                { "ПоискИгрокаВРадиусе", (radius, _) => _interactiveObject.detector.StartPlayerScan(UniversalParse<float>(radius)) },
                { "ПоискОбъектаВРадиусеПоИмени", (name, radius) => _interactiveObject.detector.StartObjectScan(name, UniversalParse<float>(radius)) },
                { "ОбнаружениеВоспроизведенияЗвука", (soundName, radius) => _interactiveObject.detector.StartSoundScan(soundName, UniversalParse<float>(radius)) },
                { "ОстановкаПоиска", (_, __) => _interactiveObject.detector.StopScanning() },
                { "ВзаимодействиеИгрокаСОбъектом", (name, radius) => _interactiveObject.detector.StartPlayerObjectInteractionScan(name, UniversalParse<float>(radius)) },
            }},
            {"МодульДвижения", new() {
                { "ДвигатьсяСлучайно", (_, __) => _interactiveObject.move.MoveToRandom() },
                { "ДвигатьсяКОбъекту", (_, __) => _interactiveObject.move.MoveToTarget() },
                { "ДвигатьсяОтОбъекта", (_, __) => _interactiveObject.move.MoveFromTarget() },
                { "ДвигатьсяПоКоординатам", (_, __) => _interactiveObject.move.MoveToPosition() },
                { "ЗадатьКоординаты", (x, y) => _interactiveObject.move.SetPosition(UniversalParse<float>(x), UniversalParse<float>(y)) },
                { "ЗадатьКоординатуВверх", (n, _) => _interactiveObject.move.movePosition.X =  UniversalParse<float>(n) },
                { "ЗадатьКоординатуВниз", (n, _) => _interactiveObject.move.movePosition.X =  -UniversalParse<float>(n) },
                { "ЗадатьКоординатуВправо", (n, _) => _interactiveObject.move.SetPositionRight(UniversalParse<float>(n)) },// interactiveObject.move.movePosition.Z =  UniversalParse<float>(n) },
                { "ЗадатьКоординатуВлево", (n, _) => _interactiveObject.move.SetPositionLeft(UniversalParse<float>(n)) },
                { "ЗадатьСлучайныеКоординаты", (radius, _) => _interactiveObject.move.SetRandomPosition(UniversalParse<float>(radius)) },
                { "СбросКоординат", (_, __) => interactiveObject.move.ResetCoordinates() },
                { "Стоп", (_, __) => interactiveObject.move.StopMoving() },
                { "ЗадатьИмяОбъекта", (name, __) => interactiveObject.selectedObjectName = name },
                { "ЗадатьПройденноеРасстояние", (distance, _) => _interactiveObject.move.SetMoveDistance(UniversalParse<float>(distance)) },
            }},
            {"МодульАнимаций", new() {
                { "ВоспроизвестиАнимацию", (id, cyclesCount) => _interactiveObject.move.PlayAnimation(id, UniversalParse<int>(cyclesCount)) },
                { "ВоспроизвестиСлучайнуюАнимацию", (cyclesCount, _) => _interactiveObject.move.PlayRandomAnimation(UniversalParse<int>(cyclesCount)) },
                { "ОстановитьАнимацию", (id, cyclesCount) => _interactiveObject.move.StopAnimation() },
            }},

            {"ВоспроизведениеЗвука", new() {
                { "УстановитьРадиусСлышимости", (radius, __) => _interactiveObject.audio.SetMaxDistance(UniversalParse<float>(radius)) },
                { "ВоспроизвестиЗвук", (soundId, cycle) => _interactiveObject.audio.Play2D(soundId, cycle) },
                { "ВоспроизвестиСлучайныйЗвук", (cycle, _) => _interactiveObject.audio.PlayRandom2D(cycle) },
                { "Стоп", (_, __) => _interactiveObject.audio.Stop() },
                { "Пауза", (_, __) => _interactiveObject.audio.Pause() },
            }},
            {"Таймер", new() {
                { "ТаймерЗапуск", (time, __) => _interactiveObject.timer.StartTimer(UniversalParse<float>(time)) },
                { "ТаймерЗапускДиапазон", (time0, time1) => _interactiveObject.timer.StartRandom(UniversalParse<float>(time0), UniversalParse<float>(time1)) },
                { "ТаймерСтоп", (_, __) => _interactiveObject.timer.StopTimer() },
                { "ТаймерСброс", (_, __) => _interactiveObject.timer.ResetTimer() },
            }},
            {"Счётчик1", new() {
                { "ПрибавитьЗначение", (val, __) => _interactiveObject.counter1.AddValue(UniversalParse<int>(val)) },
                { "ОтнятьЗначение", (val, __) => _interactiveObject.counter1.SubValue(UniversalParse<int>(val)) },
                { "ОбнулитьЗначение", (_, __) => _interactiveObject.counter1.ResetValue() },
            }},
            {"Счётчик2", new() {
                { "ПрибавитьЗначение", (val, __) => _interactiveObject.counter2.AddValue(UniversalParse<int>(val)) },
                { "ОтнятьЗначение", (val, __) => _interactiveObject.counter2.SubValue(UniversalParse<int>(val)) },
                { "ОбнулитьЗначение", (_, __) => _interactiveObject.counter2.ResetValue() },
            }},
        };

        VariableDictionary = new()
        {
            {"МодульДвижения", new() {
                { "ПройденноеРасстояние", _interactiveObject.move.moveDistance },
            }},
            {"ВзаимодействиеСМиром", new() {
                { "ВремяСуток", _interactiveObject.move.timesOfDay },
                { "ТипПоверхности", _interactiveObject.move.surfaceType },
                { "ВысотаНадПоверхностью", _interactiveObject.move.heightWorld },
            }},
            {"Таймер", new() {
                { "ТекущееЗначениеТаймера", _interactiveObject.timer.CurrentTimerValue },
            }},
            {"Счётчик1", new() {
                { "ТекущееЗначение", _interactiveObject.counter1.variable },
            }},
            {"Счётчик2", new() {
                { "ТекущееЗначение", _interactiveObject.counter2.variable },
            }},
        };

        ClearActionHolders();
    }

    public void ClearActionHolders()
    {
        foreach(var module in SignalDictionary)
        {
            foreach (var action in module.Value)
            {
                action.Value?.ClearSubscriptions();
            }
                
        }
    }

    public static T UniversalParse<T>(string input) where T : struct
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return default(T);
        }

        if (typeof(T) == typeof(int) && int.TryParse(input, out int intResult))
        {
            return (T)(object)intResult;
        }

        if (typeof(T) == typeof(float) && float.TryParse(input, out float floatResult))
        {
            return (T)(object)floatResult;
        }

        if (typeof(T) == typeof(double) && double.TryParse(input, out double doubleResult))
        {
            return (T)(object)doubleResult;
        }

        if (typeof(T) == typeof(decimal) && decimal.TryParse(input, out decimal decimalResult))
        {
            return (T)(object)decimalResult;
        }

        // Если тип не поддерживается или парсинг не удался
        return default(T); // Вернет 0
    }

    //public static Dictionary<string, Func<int>> VariableDictionary = new Dictionary<string, Func<int>>()
    //{
    //    // InteractiveObjectMove.cs
    //    { "ПройденноеРасстояние", () => InteractiveObjectMove.ПройденноеРасстояние() },

    //    // InteractiveObjectWorld.cs
    //    { "ВремяСуток", () => InteractiveObjectWorld.ВремяСуток() },
    //    { "ТипПоверхности", () => InteractiveObjectWorld.ТипПоверхности() },
    //    { "ВысотаНадПоверхностью", () => InteractiveObjectWorld.ВысотаНадПоверхностью() }
    //};
}
