using bearloga.addons.Ursula.Modules.LogicInjector;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using System;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers.InjectorBuilders
{
    public static class TrafficLightsSheduleInjectorBuilder
    {
        public static Injector CreateInjector(NavGraphVertexShedule shedule)
        {
            InjectorStateOverride green = CreateGreenStateOverride(shedule.timeOpen);
            InjectorStateOverride red = CreateRedStateOverride(shedule.timeClosed);
            InjectorStateOverride offset = CreateOffsetStateOverride(shedule.offset);
            return new Injector(new List<InjectorStateOverride>() { green, red, offset });
        }

        private static InjectorStateOverride CreateGreenStateOverride(float timeOpen)
        {
            InjectorStateCommandOverride commandOverride = new InjectorStateCommandOverride("Таймер.ТаймерЗапуск", 0, timeOpen.ToString());
            InjectorStateEventOverride eventOverride = new InjectorStateEventOverride("Enter", new List<InjectorStateCommandOverride>() { commandOverride });
            return new InjectorStateOverride("[Inject] Green", new List<InjectorStateEventOverride>() { eventOverride });
        }

        private static InjectorStateOverride CreateRedStateOverride(float timeClosed)
        {
            InjectorStateCommandOverride commandOverride = new InjectorStateCommandOverride("Таймер.ТаймерЗапуск", 0, timeClosed.ToString());
            InjectorStateEventOverride eventOverride = new InjectorStateEventOverride("Enter", new List<InjectorStateCommandOverride>() { commandOverride });
            return new InjectorStateOverride("[Inject] Red", new List<InjectorStateEventOverride>() { eventOverride });
        }

        private static InjectorStateOverride CreateOffsetStateOverride(float offset)
        {
            InjectorStateCommandOverride commandOverride = new InjectorStateCommandOverride("Таймер.ТаймерЗапуск", 0, offset.ToString());
            InjectorStateEventOverride eventOverride = new InjectorStateEventOverride("Enter", new List<InjectorStateCommandOverride>() { commandOverride });
            return new InjectorStateOverride("[Inject] Time Offset", new List<InjectorStateEventOverride>() { eventOverride });
        }
    }
}
