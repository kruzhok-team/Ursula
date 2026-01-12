using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation.Stages
{
    public static class NavGraphIslandStage
    {
        // Removes all connected subgraphs except one that contains maximum amount of vertices
        public static NavGraph EraseIslands(NavGraph navGraph)
        {
            List<NavGraph> connectedSubgraphs = new List<NavGraph>();
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                if (connectedSubgraphs.Exists(graph => graph.vertices.Contains(vertex)))
                    continue;

                connectedSubgraphs.Add(GetConnectedSubgraph(vertex));
            }

            return connectedSubgraphs.MaxBy(graph => graph.vertices.Count);
        }

        // Depth search that returns connected subgraph that contains startVertex
        private static NavGraph GetConnectedSubgraph(NavGraphVertex startVertex)
        {
            HashSet<NavGraphEdge> edges = new HashSet<NavGraphEdge>();
            HashSet<NavGraphVertex> vertices = new HashSet<NavGraphVertex>();
            Queue<NavGraphVertex> vertexQueue = new Queue<NavGraphVertex>();
            vertexQueue.Enqueue(startVertex);

            while (vertexQueue.Count != 0)
            {
                NavGraphVertex vertex = vertexQueue.Dequeue();
                vertices.Add(vertex);

                foreach (NavGraphEdge edge in vertex.edges)
                {
                    if (!edges.Contains(edge))
                        edges.Add(edge);

                    NavGraphVertex next;
                    if (edge.v1 == vertex)
                    {
                        next = edge.v2;
                    }
                    else
                    {
                        next = edge.v1;
                    }

                    if (!vertexQueue.Contains(next) && !vertices.Contains(next))
                        vertexQueue.Enqueue(next);
                }
            }

            return new NavGraph(edges.ToList(), vertices.ToList());
        }
    }
}
