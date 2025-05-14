namespace Modules.HSM
{
    public class Transition
    {
        public HierarchicalStateMachine gml;

        public Event When { get; set; }
        public State From { get; set; }
        public State To { get; set; }

        public void Subscribe()
        {
            When.OnExecute.Subscribe(When, () =>
            {
                bool cond = true;
                if (When._Condition != null)
                {
                    cond = When._Condition.EvaluateCondition();
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Вычислено условие перехода состояний {When._Condition} -> {cond}");
                }

                if (gml.currentState == From)
                {
                    if (cond)
                    {
                        //ContextMenu.ShowMessageS($"{gml.GetId()} Переход состояния из {From.nodeType} в {To.nodeType} по сигналу {When.Module}.{When.ActionName}");
                        ContextMenu.ShowMessageS($"{gml.GetPrefix()} Переход из [{From.Name}] в [{To.Name}] по событию {When.Module}.{When.ActionName}");

                        var exit = From.Events.Find(p => p.ActionName == "exit");
                        if (exit != null)
                            exit.OnExecute.Invoke();

                        When.ExecuteMethodsPayload();

                        gml.currentState = To;

                        var entry = To.Events.Find(p => p.ActionName == "entry");
                        if (entry != null)
                            entry.OnExecute.Invoke();


                    }
                }
                else
                    ContextMenu.ShowMessageS($"{gml.GetPrefix()} Отмена перехода из [{From.Name}] в [{To.Name}] по событию {When.Module}.{When.ActionName} так как алгоритм уже перешёл в состояние {gml.currentState.Name}");

            });
        }

        public override string ToString()
        {
            return $"{From} -> {To}: {When?.Module}.{When?.ActionName}";
        }
    }
}
