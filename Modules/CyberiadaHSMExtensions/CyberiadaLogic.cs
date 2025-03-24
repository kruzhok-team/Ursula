using Godot;
using System;
using System.Xml.Linq;

using Talent.Graphs;
using Talent.Logic;
using Talent.Logic.Bus;
using Talent.Logic.HSM;

public class CyberiadaLogic
{
    public LocalBus localBus;

    HSMBehavior _hsmBehavior;

    State _rootState;

    HSMLogger _logger;

    public static CyberiadaLogic Load(string graphMLPath)
    {
        var element = XElement.Load(ProjectSettings.GlobalizePath(graphMLPath));

        return new CyberiadaLogic(element);
    }

    public CyberiadaLogic(XElement element)
    {
        var converter = new CyberiadaGraphMLConverter("SandaloneGraphEditor", "1.0");
        var graphDoc = converter.Deserialize(element);

        localBus = new LocalBus();

        var hsmConverter = new CyberiadaHSMConverter();
        _hsmBehavior = hsmConverter.Process(graphDoc.RootGraph, localBus) as HSMBehavior;

        _rootState = _hsmBehavior.GetRootState();
    }

    public void SubscribeLogger(HSMLogger logger)
    {
        _logger = logger;

        _rootState.StateEnterEvent += logger.OnStateEnter;
        _rootState.StateExitEvent += logger.OnStateExit;
        _rootState.TransitionTriggeredEvent += logger.OnTransitionTriggered;
        _rootState.CommandBeginEvent += logger.OnCommandMaked;
        _rootState.TransitionConditionCheckEvent += logger.OnConditionCheck;
    }

    public void UnsubscribeLogger(HSMLogger logger)
    {
        _rootState.StateEnterEvent -= logger.OnStateEnter;
        _rootState.StateExitEvent -= logger.OnStateExit;
        _rootState.TransitionTriggeredEvent -= logger.OnTransitionTriggered;
        _rootState.CommandBeginEvent -= logger.OnCommandMaked;
        _rootState.TransitionConditionCheckEvent -= logger.OnConditionCheck;

    }

    public void Start()
    {
        _hsmBehavior.Start();
    }

    public void Update()
    {
        _hsmBehavior.Update();
    }

    public void Stop()
    {
        //UnsubscribeLogger(_logger);

        _hsmBehavior.Stop();
    }

}
