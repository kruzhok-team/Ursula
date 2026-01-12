using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using System.Collections.Generic;
using Godot;
using System;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation.Stages
{
    public static class NavGraphDirectedStage
    {
        // Makes directed graph.
        // Splits every edge into right and left handed directions.
        public static NavGraph ApplyDirections(NavGraph navGraph, float offset)
        {
            List<NavGraphVertex> vertices = new List<NavGraphVertex>();
            List<NavGraphEdge> edges = new List<NavGraphEdge>();
            // Maps old vertices to new ones
            Dictionary<NavGraphVertex, NavGraphEdge> vertexMap = new Dictionary<NavGraphVertex, NavGraphEdge>();

            foreach (NavGraphEdge edge in navGraph.edges)
            {
                Vector3 direction = edge.v2.position - edge.v1.position;
                direction = direction.Normalized();

                Vector3 rightDirection = Vector3.Up.Cross(direction);
                Vector3 leftDirection = -rightDirection;

                // Determine left and right vertices for the first vertex from the edge
                NavGraphVertex leftV1;
                NavGraphVertex rightV1;
                if (!vertexMap.ContainsKey(edge.v1))
                {
                    NavGraphEdge leftToRightV1 = CreateLeftToRightEdge(edge, edge.v1, offset);
                    vertexMap[edge.v1] = leftToRightV1;
                    leftV1 = leftToRightV1.v1;
                    rightV1 = leftToRightV1.v2;
                    vertices.Add(leftV1);
                    vertices.Add(rightV1);
                    UpdateSheduleGroup(rightV1, edge.v1);
                }
                else
                {
                    NavGraphEdge leftToRightV1 = OriendLeftToRightEdge(edge, vertexMap[edge.v1]);
                    leftV1 = leftToRightV1.v1;
                    rightV1 = leftToRightV1.v2;
                }

                // Determine left and right vertices for the second vertex from the edge
                NavGraphVertex leftV2;
                NavGraphVertex rightV2;
                if (!vertexMap.ContainsKey(edge.v2))
                {
                    NavGraphEdge leftToRightV2 = CreateLeftToRightEdge(edge, edge.v2, offset);
                    vertexMap[edge.v2] = leftToRightV2;
                    leftV2 = leftToRightV2.v1;
                    rightV2 = leftToRightV2.v2;
                    vertices.Add(leftV2);
                    vertices.Add(rightV2);
                    // update shedule group for intersections
                    UpdateSheduleGroup(leftV2, edge.v2);
                }
                else
                {
                    NavGraphEdge leftToRightV2 = OriendLeftToRightEdge(edge, vertexMap[edge.v2]);
                    leftV2 = leftToRightV2.v1;
                    rightV2 = leftToRightV2.v2;
                }

                NavGraphEdge left = new NavGraphEdge(leftV1, leftV2);
                NavGraphEdge right = new NavGraphEdge(rightV2, rightV1);


                edges.Add(left);
                edges.Add(right);

                // Connect right and left directions in dead ends
                if (edge.v1.edges.Count == 1)
                {
                    NavGraphEdge deadEnd = new NavGraphEdge(rightV1, leftV1);
                    edges.Add(deadEnd);
                }

                if (edge.v2.edges.Count == 1)
                {
                    NavGraphEdge deadEnd = new NavGraphEdge(leftV2, rightV2);
                    edges.Add(deadEnd);
                }
            }

            return new NavGraph(edges, vertices);
        }

        private static void UpdateSheduleGroup(NavGraphVertex newVertex, NavGraphVertex oldVertex)
        {
            if (newVertex.sheduleGroup != null || oldVertex.sheduleGroup == null)
                return;
            newVertex.sheduleGroup = oldVertex.sheduleGroup;
            int idx = newVertex.sheduleGroup.IndexOf(oldVertex);
            if (idx == -1)
                throw new Exception("Couldn't find vertex in it's shedule group.");
            newVertex.sheduleGroup[idx] = newVertex;
        }

        private static NavGraphEdge OriendLeftToRightEdge(NavGraphEdge edge, NavGraphEdge leftToRightEdge)
        {
            // Determine left and right vertices if map record is not empty
            if (IsLeft(edge.v1.position, edge.v2.position, leftToRightEdge.v1.position))
            {
                return leftToRightEdge;
            }
            else
            {
                return new NavGraphEdge(leftToRightEdge.v2, leftToRightEdge.v1, temp: true);
            }
        }

        private static NavGraphEdge CreateLeftToRightEdge(NavGraphEdge edge, NavGraphVertex vertex, float offset)
        {
            Vector3 direction = edge.v2.position - edge.v1.position;
            direction = direction.Normalized();

            Vector3 rightDirection = Vector3.Up.Cross(direction);
            Vector3 leftDirection = -rightDirection;

            // Create new vertices if map record is empty
            NavGraphVertex right = new NavGraphVertex(vertex.position + rightDirection * offset);
            NavGraphVertex left = new NavGraphVertex(vertex.position + leftDirection * offset);

            return new NavGraphEdge(left, right, temp: true);
        }

        private static bool IsLeft(Vector3 edgeV1, Vector3 edgeV2, Vector3 p)
        {
            // cross product
            return (edgeV2.X - edgeV1.X) * (p.Z - edgeV1.Z) - (edgeV2.Z - edgeV1.Z) * (p.X - edgeV1.X) > 0;
        }
    }
}
