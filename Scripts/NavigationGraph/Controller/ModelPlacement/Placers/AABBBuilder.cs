using Godot;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers
{
    public static class AABBBuilder
    {
        public static Aabb BuildAabb(Node3D root)
        {
            List<Vector3> points = new List<Vector3>();
            CollectPoints(root, Transform3D.Identity, points);

            if (points.Count == 0)
                return new Aabb(Vector3.Zero, Vector3.Zero);

            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            foreach (Vector3 p in points)
            {
                min = min.Min(p);
                max = max.Max(p);
            }

            return new Aabb(min, max - min);
        }

        private static void CollectPoints(Node3D node, Transform3D parentTransform, List<Vector3> outPoints)
        {
            Transform3D transform = parentTransform * node.Transform;

            if (node is VisualInstance3D vi)
            {
                Aabb aabb = vi.GetAabb();
                AddAabbCorners(aabb, transform, outPoints);
            }

            foreach (Node child in node.GetChildren())
            {
                if (child is Node3D child3D)
                    CollectPoints(child3D, transform, outPoints);
            }
        }

        private static void AddAabbCorners(Aabb aabb, Transform3D transform, List<Vector3> list)
        {
            foreach (Vector3 corner in GetCorners(aabb))
                list.Add(transform * corner);
        }

        private static Vector3[] GetCorners(Aabb a)
        {
            Vector3 p = a.Position;
            Vector3 s = a.Size;

            return new Vector3[]
            {
            p,
            p + new Vector3(s.X,0,0),
            p + new Vector3(0,s.Y,0),
            p + new Vector3(0,0,s.Z),
            p + new Vector3(s.X,s.Y,0),
            p + new Vector3(s.X,0,s.Z),
            p + new Vector3(0,s.Y,s.Z),
            p + s
            };
        }
    }
}
