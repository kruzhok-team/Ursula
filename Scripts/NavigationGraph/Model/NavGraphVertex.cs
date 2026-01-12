using Godot;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Model
{
    public class NavGraphVertex
    {
        public Vector3 position;
        public List<NavGraphEdge> edges;
        public NavGraphVertexShedule shedule;
        public List<NavGraphVertex> sheduleGroup;

        public bool ContainsShedule => shedule != null;

        public NavGraphVertex(Vector3 position)
        {
            this.position = position;
            edges = new List<NavGraphEdge>();
        }

        public override string ToString()
        {
            return $"NavGraphVertex ({position})";
        }
    }
}
