using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Model
{
    public class NavGraph
    {
        public List<NavGraphEdge> edges;
        public List<NavGraphVertex> vertices;

        public NavGraph(List<NavGraphEdge> edges, List<NavGraphVertex> vertices)
        {
            this.edges = edges;
            this.vertices = vertices;
        }

        public NavGraph Copy()
        {
            return NavGraphCopy.Copy(this);
        }
    }
}
