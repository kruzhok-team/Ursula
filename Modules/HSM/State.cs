using System.Collections.Generic;

namespace Modules.HSM
{
    public class State
    {
        public string Name { get; set; }

        public State ParentState { get; set; } = null;

        public HierarchicalStateMachine gml;

        public string nodeType = "";

        public string Id { get; set; }
        public List<Event> Events { get; set; } = new List<Event>(); // События, которые выполняются внутри состояния (обычно это entry и exit)

        public override string ToString()
        {
            return $"{Id}({Name})";
        }

    }
}
