using System;
using System.Collections.Generic;

namespace Talent.Logic.HSM
{
    /// <summary>
    /// Класс, представляющий состояние иерархической машины состояний
    /// </summary>
    public class State
    {
        private State _parent;
        private HierarchicalStateMachine _owner;

        /// <summary>
        /// Идентификатор состояния
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Метка состояния
        /// </summary>
        public string Label { get; private set; } = "";

        /// <summary>
        /// Дочерние состояния
        /// </summary>
        public IEnumerable<State> ChildStates => _owner?.States;

        /// <summary>
        /// Команды, вызываемые при входе в состояние
        /// </summary>
        public IEnumerable<Command> EnterCommands { get; private set; }

        /// <summary>
        /// Команды вызываемые при выходе из состояния
        /// </summary>
        public IEnumerable<Command> ExitCommands { get; private set; }

        /// <summary>
        /// События состояния
        /// </summary>
        public IEnumerable<Event> Events { get; private set; }

        /// <summary>
        /// Переходы состояния
        /// </summary>
        public IEnumerable<Transition> Transition { get; private set; }

        /// <summary>
        /// Событие входа в состояние
        /// </summary>
        public event EventHandler StateEnterEvent;

        /// <summary>
        /// Событие выхода из состояния
        /// </summary>
        public event EventHandler StateExitEvent;

        /// <summary>
        /// Событие при вызове перехода между состояниями
        /// </summary>
        public event EventHandler<Transition> TransitionTriggeredEvent;

        /// <summary>
        /// Событие при выполнении команды
        /// </summary>
        public event EventHandler<Command> CommandBeginEvent;

        /// <summary>
        /// Событие при проверке условия
        /// </summary>
        public event EventHandler<ConditionEventArgs> TransitionConditionCheckEvent;

        /// <summary>
        /// Инициализирует состояние через переданные параметры
        /// </summary>
        /// <param name="id">Идентификатор состояния</param>
        /// <param name="label">Метка состояния</param>
        /// <param name="parent">Родительское состояние</param>
        /// <param name="enter">Команды входа в состояния</param>
        /// <param name="exit">Команды выхода из состояния</param>
        /// <param name="events">События, ассоциированные с состояниями</param>
        /// <param name="transitions">Переходы из состояния</param>
        /// <param name="owner">Машина состояний, хранящая данное состояние</param>
        public void Init(
            string id,
            string label,
            State parent,
            IEnumerable<Command> enter,
            IEnumerable<Command> exit,
            IEnumerable<Event> events,
            IEnumerable<Transition> transitions,
            HierarchicalStateMachine owner)
        {
            ID = id;
            Label = label;
            EnterCommands = enter;
            ExitCommands = exit;
            Transition = transitions;
            _owner = owner;
            _parent = parent;
            Events = events;
        }

        /// <summary>
        /// Входит в состояние
        /// </summary>
        public void Enter()
        {
            StateEnterEvent?.Invoke(this,EventArgs.Empty);

            if (Transition != null)
            {
                foreach (Transition transition in Transition)
                {
                    transition.CommandBeginEvent += CommandBeginEvent;
                    transition.ConditionCheckEvent += OnConditionCheck;
                    transition.TriggeredEvent += OnTrigger;
                    transition.Active();
                }
            }

            if (EnterCommands != null)
            {
                foreach (Command commandStorage in EnterCommands)
                {
                    CommandBeginEvent?.Invoke(this, commandStorage);
                    commandStorage.Make();
                }
            }

            if (Events != null)
            {
                foreach (Event eventToCommand in Events)
                {
                    eventToCommand.Activate();
                }
            }
        }

        /// <summary>
        /// Входит в подсостояние с уникальным идентификатором
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        public void EnterSubState(string id)
        {
            _owner?.EnterState(id);
        }

        /// <summary>
        /// Выходит из состояния
        /// </summary>
        public void Exit()
        {
            _owner?.ExitCurrent();

            StateExitEvent?.Invoke(this, EventArgs.Empty);

            if (Transition != null)
            {
                foreach (Transition transition in Transition)
                {
                    transition.CommandBeginEvent -= CommandBeginEvent;
                    transition.ConditionCheckEvent -= OnConditionCheck;
                    transition.TriggeredEvent -= OnTrigger;
                    transition.Deactivate();
                }
            }

            if (Events != null)
            {
                foreach (Event eventToCommand in Events)
                {
                    eventToCommand.Deactivate();
                }
            }

            if (ExitCommands != null)
            {
                foreach (Command commandStorage in ExitCommands)
                {
                    CommandBeginEvent?.Invoke(this, commandStorage);
                    commandStorage.Make();
                }
            }
        }

        private void OnTrigger(object sender, string nextStateID)
        {
            if (_parent != null)
                TransitionTriggeredEvent?.Invoke(this, sender as Transition);

            if (_parent != null)
            {
                _parent.OnTrigger(sender, nextStateID);
            }
            else
            {
                _owner.EnterState(nextStateID);
            }
        }

        private void OnConditionCheck(object sender, ConditionEventArgs eventArgs)
        {
            TransitionConditionCheckEvent.Invoke(sender, eventArgs);
        }

        public HierarchicalStateMachine GetOwnerHSM()
        {
            return _owner;
        }

        public void SubscribeChildEvents(State childState)
        {
            childState.StateEnterEvent += (s, a) => StateEnterEvent.Invoke(s, a);
            childState.StateExitEvent += (s, a) => StateExitEvent.Invoke(s, a);
            childState.TransitionTriggeredEvent += (s, a) => TransitionTriggeredEvent.Invoke(s, a);
            childState.CommandBeginEvent += (s, a) => CommandBeginEvent.Invoke(s, a);
            childState.TransitionConditionCheckEvent += (s, a) => TransitionConditionCheckEvent.Invoke(s, a);
        }

    }
}