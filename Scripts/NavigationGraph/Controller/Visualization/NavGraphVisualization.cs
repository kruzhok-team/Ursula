using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization
{
    public class NavGraphVisualization
    {
        private Dictionary<NavGraphVertex, MeshInstance3D> points = new Dictionary<NavGraphVertex, MeshInstance3D>();
        private List<MeshInstance3D> lines = new List<MeshInstance3D>();

        public void DrawPath(IEnumerable<Vector3> pathPoints, Node parent, float heightOffset)
        {
            List<NavGraphVertex> vertices = new List<NavGraphVertex>();
            List<NavGraphEdge> edges = new List<NavGraphEdge>();

            foreach (Vector3 point in pathPoints)
            {
                vertices.Add(new NavGraphVertex(point));
            }

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                edges.Add(new NavGraphEdge(vertices[i], vertices[i + 1]));
            }

            NavGraph navGraphPath = new NavGraph(edges, vertices);
            Draw(navGraphPath, parent, heightOffset);
        }

        public void Draw(NavGraph graph, Node parent, float heightOffset)
        {
            foreach (NavGraphVertex vertex in graph.vertices)
            {
                MeshInstance3D meshInstance = NavGraphVertexVisualization.InstantiateMeshInstance3D(vertex, parent, heightOffset);
                points[vertex] = (meshInstance);
            }

            foreach (NavGraphEdge edge in graph.edges)
            {
                MeshInstance3D meshInstance = NavGraphEdgeVisualization.InstantiateMeshInstance3D(edge, parent, heightOffset);
                lines.Add(meshInstance);
            }
        }

        public void Update(float time)
        {
            foreach (NavGraphVertex vertex in points.Keys)
            {
                if (vertex.ContainsShedule)
                {
                    if (vertex.shedule.IsOpen(time))
                    {
                        UpdateColor(vertex, new Color(0, 1, 0, 1));
                    }
                    else
                    {
                        UpdateColor(vertex, new Color(1, 0, 0, 1));
                    }
                }
            }
        }

        private void UpdateColor(NavGraphVertex vertex, Color color)
        {
            MeshInstance3D meshInstance = points[vertex];
            Material material = meshInstance.GetActiveMaterial(0);
            OrmMaterial3D ormMaterial = material as OrmMaterial3D;
            if (ormMaterial is null)
                throw new System.Exception($"Couldn't access OrmMaterial of meshInstance {meshInstance.Name}");
            ormMaterial.AlbedoColor = color;
        }

        public void Clear()
        {
            foreach (MeshInstance3D point in points.Values)
            {
                point.QueueFree();
            }

            foreach (MeshInstance3D line in lines)
            {
                line.QueueFree();
            }

            points.Clear();
            lines.Clear();
        }
    }
}
