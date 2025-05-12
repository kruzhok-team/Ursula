using System.Collections.Generic;

namespace Modules.HSM
{
    public class Event
    {
        public HierarchicalStateMachine gml;

        public string Module { get; set; }
        public string ActionName { get; set; }
        public Condition _Condition { get; set; }
        public List<Command> Methods { get; set; } = new List<Command>();

        public ActionHolder OnExecute = null;

        public State signalParent = null; // если пустой значит событие в линке
        public Transition linkParent = null;

        private void ConnectAction()
        {
            if (gml.apiDictionary.HasSignal(Module, ActionName))
                OnExecute = gml.apiDictionary.SignalDictionary[Module][ActionName];
            else
            if (OnExecute == null)
                OnExecute = new(ActionName);
        }

        public void SubscribeForLinks()
        {
            ConnectAction();
        }

        public void Subscribe()
        {
            ConnectAction();

            OnExecute.Subscribe(this, () =>
            {
                ExecuteMethodsPayload();
            });
        }

        public void ExecuteMethodsPayload()
        {
            var availableSignal = signalParent == null || gml.currentState == signalParent;
            var availableSignalByLink = linkParent == null || gml.currentState == linkParent.From;

            if (availableSignal && availableSignalByLink)
            {
                ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вызван сигнал {Module}.{ActionName}{(signalParent != null ? $" в состоянии [{signalParent.Name}]" : $" в связи [{linkParent.From.Name}] -> [{linkParent.To.Name}]")}");

                bool cond = true;
                if (_Condition != null)
                {
                    cond = _Condition.EvaluateCondition();
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вычислено условие выполнения сигнала {_Condition} -> {cond}");
                }

                if (cond)
                {
                    foreach (var method in Methods)
                    {
                        method.Execute();
                    }
                }
                else
                {
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Сигнал {Module}.{ActionName} отозван условие не выполнено");
                }
            }
        }

        public static Event ParseFromData(string row, HierarchicalStateMachine gml)
        {
            var signal = new Event();

            var splitedRow = new string[1];

            if (row.Contains("\n")) // значит что этот сигнал находится в линке, в этом случае добавляем в массив методов все эти методы
            {
                splitedRow = row.Split("\n");

                for (int i = 1; i < splitedRow.Length; i++)
                {
                    var method = Command.ParseFromData(splitedRow[i], gml);
                    signal.Methods.Add(method);
                }
            }
            else
            {
                splitedRow[0] = row;
            }

            if (splitedRow[0].Contains('['))
            {
                var s = splitedRow[0].Split('[');
                signal._Condition = Condition.ParseFromData(s[1], gml);
                signal._Condition.gml = gml;

                splitedRow[0] = s[0]; // строка с сигналом без условий
            }

            if (splitedRow[0].Contains('.'))
            {
                var s = splitedRow[0].Split('.');
                signal.Module = s[0];
                signal.ActionName = s[1].Replace("/", "").Replace("\r", "");
            }
            else
            {
                signal.ActionName = splitedRow[0].Replace("/", "").Replace("\r", "");
            }

            signal.gml = gml;

            return signal;
        }

        public override string ToString()
        {
            return $"(Signal) {Module}.{ActionName}";
        }
    }
}
