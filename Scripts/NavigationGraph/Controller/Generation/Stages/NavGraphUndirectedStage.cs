using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation.Stages
{
    public static class NavGraphUndirectedStage
    {
        // Creates undirected graph on a rectangular grid
        public static NavGraph CreateBaseForm(float range, float height, float delta, float connectionProbability)
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();

            int gridSizeX = (int)(range / delta) - 1;
            int gridSizeY = (int)(range / delta) - 1;

            // Create grid filled with placeholders
            NavGraphVertex[] vertices = new NavGraphVertex[gridSizeX * gridSizeY];
            List<NavGraphEdge> edges = new List<NavGraphEdge>();

            // Iterate through every vertex
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    // Determine if edge to the right neighbor should be created
                    float prob = rng.Randf();
                    if (i != gridSizeX - 1 && prob < connectionProbability)
                    {
                        // Create current vertex or check it's position if already exists
                        int index1 = GetGridIndex(i, j, gridSizeX);
                        Vector3 position1 = GetPosition(i, j, height, delta, delta);
                        CheckCreateVertex(index1, position1, vertices);

                        // Create neighbor vertex or check it's position if already exists
                        int index2 = GetGridIndex(i + 1, j, gridSizeX);
                        Vector3 position2 = GetPosition(i + 1, j, height, delta, delta);
                        CheckCreateVertex(index2, position2, vertices);

                        // Create edge
                        NavGraphVertex v1 = vertices[index1];
                        NavGraphVertex v2 = vertices[index2];
                        NavGraphEdge edge = new NavGraphEdge(v1, v2); // Adds reference for this edge into v1 and v2
                        edges.Add(edge);
                    }

                    // Determine if edge to the down neighbor should be created
                    prob = rng.Randf();
                    if (j != gridSizeY - 1 && prob < connectionProbability)
                    {
                        // Create current vertex or check it's position if already exists
                        int index1 = GetGridIndex(i, j, gridSizeX);
                        Vector3 position1 = GetPosition(i, j, height, delta, delta);
                        CheckCreateVertex(index1, position1, vertices);

                        // Create neighbor vertex or check it's position if already exists
                        int index2 = GetGridIndex(i, j + 1, gridSizeX);
                        Vector3 position2 = GetPosition(i, j + 1, height, delta, delta);
                        CheckCreateVertex(index2, position2, vertices);

                        // Create edge
                        NavGraphVertex v1 = vertices[index1];
                        NavGraphVertex v2 = vertices[index2];
                        NavGraphEdge edge = new NavGraphEdge(v1, v2); // Adds reference for this edge into v1 and v2
                        edges.Add(edge);
                    }
                }
            }

            // Clear all placeholders
            List<NavGraphVertex> resultVertices = new List<NavGraphVertex>();
            foreach (NavGraphVertex vertex in vertices)
            {
                if (vertex != null)
                    resultVertices.Add(vertex);
            }

            return new NavGraph(edges, resultVertices);
        }


        private static void CheckCreateVertex(int index, Vector3 position, NavGraphVertex[] vertices)
        {
            if (vertices[index] is null)
                vertices[index] = new NavGraphVertex(position);
            else if (vertices[index].position != position)
                throw new Exception("Couldn't generate navigation graph vertex. Space is already occupied.");
        }

        private static int GetGridIndex(int x, int y, int gridSizeX)
        {
            return y * gridSizeX + x;
        }

        private static Vector3 GetPosition(int x, int y, float height, float dx, float dy)
        {
            Vector3 origin = new Vector3(dx, height, dy);
            Vector3 offset = new Vector3(x * dx, 0, y * dy);
            return origin + offset;
        }
    }
}
