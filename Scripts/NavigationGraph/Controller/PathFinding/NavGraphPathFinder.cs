using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using System;
using System.Collections.Generic;
using Godot;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.PathFinding
{
    public static class NavGraphPathFinder
    {
        // A*
        public static List<NavGraphVertex> GetPath(NavGraphVertex from, NavGraphVertex to)
        {
            if (from == null || to == null) return null;
            if (ReferenceEquals(from, to)) return new List<NavGraphVertex> { from };

            // Path recovery map
            Dictionary<NavGraphVertex, NavGraphVertex> cameFrom = new Dictionary<NavGraphVertex, NavGraphVertex>();

            // gScore: cumulative edge transition cost.
            Dictionary<NavGraphVertex, float> gScore = new Dictionary<NavGraphVertex, float>();
            gScore[from] = 0f;

            // fScore: total score (gScore + heuristic)
            Dictionary<NavGraphVertex, float> fScore = new Dictionary<NavGraphVertex, float>();
            fScore[from] = Heuristic(from, to);

            // Vertices to visit
            // Priority queue with priority by fScore
            SimplePriorityQueue<NavGraphVertex> openSet = new SimplePriorityQueue<NavGraphVertex>();
            openSet.Enqueue(from, fScore[from]);

            // Visited vertices
            HashSet<NavGraphVertex> closedSet = new HashSet<NavGraphVertex>();

            while (openSet.Count > 0)
            {
                NavGraphVertex current = openSet.Dequeue();

                if (ReferenceEquals(current, to))
                    return ReconstructPath(cameFrom, current);

                closedSet.Add(current);

                if (current.edges == null || current.edges.Count == 0)
                    throw new Exception($"Vertex {current} has no connected edges");

                foreach (var edge in current.edges)
                {
                    // Skip directed edge if is doesn't originate from current vertex 
                    if (!ReferenceEquals(edge.v1, current))
                        continue;

                    NavGraphVertex neighbor = edge.v2;
                    if (neighbor == null)
                        throw new Exception($"Encountered null reference in edge {edge}");

                    if (closedSet.Contains(neighbor))
                        continue;

                    // Transition cost
                    float cost = Heuristic(current, neighbor);
                    float tentativeG = gScore[current] + cost;

                    float neighborG;
                    if (!gScore.TryGetValue(neighbor, out neighborG) || tentativeG < neighborG)
                    {
                        // Path to neightbor is improved
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float f = tentativeG + Heuristic(neighbor, to);
                        fScore[neighbor] = f;

                        // UpdateQueue
                        if (openSet.Contains(neighbor))
                        {
                            openSet.UpdatePriority(neighbor, f);
                        }
                        else
                        {
                            openSet.Enqueue(neighbor, f);
                        }
                    }
                }
            }

            // Path is not found
            throw new Exception($"Couldn't build path from {from} to {to}");
        }

        private static float Heuristic(NavGraphVertex a, NavGraphVertex b)
        {
            // Euclidean distance
            return a.position.DistanceTo(b.position);
        }

        private static List<NavGraphVertex> ReconstructPath(Dictionary<NavGraphVertex, NavGraphVertex> cameFrom, NavGraphVertex current)
        {
            List<NavGraphVertex> totalPath = new List<NavGraphVertex> { current };
            while (cameFrom.TryGetValue(current, out NavGraphVertex prev))
            {
                current = prev;
                totalPath.Add(current);
            }
            totalPath.Reverse();
            return totalPath;
        }

    }
}
