using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using System.Collections.Generic;
using Godot;


namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation.Stages
{
    public static class NavGraphSubdivisionStage
    {
        // Creates points at t and 1 - t of every edge
        // Connects edges similar to the original graph
        public static NavGraph Subdivide(NavGraph navGraph, float t)
        {
            List<NavGraphVertex> vertices = new List<NavGraphVertex>();
            List<NavGraphEdge> edges = new List<NavGraphEdge>();
            // maps old vertices to new ones
            Dictionary<NavGraphVertex, List<NavGraphVertex>> vertexMap = new Dictionary<NavGraphVertex, List<NavGraphVertex>>();

            ShortenEdges(navGraph, vertices, edges, vertexMap, t);
            FillGaps(navGraph, vertices, edges, vertexMap);

            return new NavGraph(edges, vertices);
        }

        private static void ShortenEdges(
            NavGraph navGraph,
            List<NavGraphVertex> vertices,
            List<NavGraphEdge> edges,
            Dictionary<NavGraphVertex, List<NavGraphVertex>> vertexMap,
            float t)
        {
            // Create new vertices and fill the vertexMap
            foreach (NavGraphEdge edge in navGraph.edges)
            {
                Vector3 source = edge.v1.position;
                Vector3 direction = edge.v2.position - edge.v1.position;

                // Create new vertices
                NavGraphVertex v1 = new NavGraphVertex(source + t * direction);
                NavGraphVertex v2 = new NavGraphVertex(source + (1 - t) * direction);
                // Create new edge in place of the old one but twice as short
                NavGraphEdge newEdge = new NavGraphEdge(v1, v2);
                vertices.Add(v1);
                vertices.Add(v2);
                edges.Add(newEdge);

                // Fill vertexMap
                if (!vertexMap.ContainsKey(edge.v1))
                    vertexMap[edge.v1] = new List<NavGraphVertex>();
                vertexMap[edge.v1].Add(v1);

                if (!vertexMap.ContainsKey(edge.v2))
                    vertexMap[edge.v2] = new List<NavGraphVertex>();
                vertexMap[edge.v2].Add(v2);
            }
        }

        private static void FillGaps(
            NavGraph navGraph,
            List<NavGraphVertex> vertices,
            List<NavGraphEdge> edges,
            Dictionary<NavGraphVertex, List<NavGraphVertex>> vertexMap
            )
        {
            // Create new edges where old ones intersect
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                List<NavGraphVertex> toConnect = vertexMap[vertex];
                for (int i = 0; i < toConnect.Count; i++)
                {
                    for (int j = i + 1; j < toConnect.Count; j++)
                    {
                        NavGraphEdge newEdge = new NavGraphEdge(toConnect[i], toConnect[j]);
                        edges.Add(newEdge);
                    }

                    if (toConnect.Count > 2)
                    {
                        // There sould be shedule group at each intersection
                        // Create shedule group for later
                        toConnect[i].sheduleGroup = toConnect;
                    }
                }
            }
        }
    }
}
