using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation.Stages;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Talent.Graphs;
using static Godot.Control;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation
{
    public static class NavGraphGenerator
    {
        public static NavGraph Generate(float range, float height, float delta, float connectionProbability)
        {
            if (range < 0 || !float.IsNormal(range))
                throw new ArgumentOutOfRangeException(nameof(range));

            if (float.IsInfinity(height) || float.IsNaN(height))
                throw new ArgumentOutOfRangeException(nameof(height));

            if (delta < 0 || !float.IsNormal(delta))
                throw new ArgumentOutOfRangeException(nameof(delta));

            if (connectionProbability < 0 || connectionProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(connectionProbability));

            NavGraph navGraph = NavGraphUndirectedStage.CreateBaseForm(range, height, delta, connectionProbability);
            navGraph = NavGraphIslandStage.EraseIslands(navGraph);
            return navGraph;
        }

        public static NavGraph PostProcess(NavGraph navGraph, float subdivisionOffset, float directionsOffset, float sheduleTimeOpen)
        {
            if (navGraph == null)
                throw new ArgumentNullException(nameof(navGraph));

            if (float.IsNaN(directionsOffset) || float.IsInfinity(directionsOffset))
                throw new ArgumentOutOfRangeException(nameof(directionsOffset));

            navGraph = NavGraphSubdivisionStage.Subdivide(navGraph, subdivisionOffset);
            navGraph = NavGraphDirectedStage.ApplyDirections(navGraph, directionsOffset);
            NavGraphSheduleStage.CreateShedules(navGraph, sheduleTimeOpen);

            return navGraph;
        }
    }
}
