using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Model
{
    public class NavGraphEdge
    {
        public NavGraphVertex v1;
        public NavGraphVertex v2;

        public NavGraphEdge(NavGraphVertex v1, NavGraphVertex v2, bool temp = false)
        {
            if (v1 is null)
                throw new ArgumentNullException(nameof(v1));

            if (v2 is null)
                throw new ArgumentNullException(nameof(v2));

            this.v1 = v1;
            this.v2 = v2;

            if (!temp)
            {
                v1.edges.Add(this);
                v2.edges.Add(this);
            }
        }

        public override string ToString()
        {
            return $"NavGraphEdge ({v1}; {v2})";
        }
    }
}
