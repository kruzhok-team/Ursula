using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ursula.GameObjects.Model;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers
{
    public static class NavGraphPlacerUtils
    {
        public static bool IsRoadStraight(NavGraphVertex vertex)
        {
            if (vertex.edges.Count != 2)
            {
                throw new ArgumentException($"Vertex should be connected to 2 edges");
            }

            NavGraphEdge e1 = vertex.edges[0];
            NavGraphEdge e2 = vertex.edges[1];

            Vector3 dir1 = (e1.v2.position - e1.v1.position).Normalized();
            Vector3 dir2 = (e2.v2.position - e2.v1.position).Normalized();

            float value = Mathf.Abs(dir1.Dot(dir2));
            if (value > 0.5f)
                return true;
            return false;
        }

        public static NavGraphEdge OrientFromVertex(NavGraphEdge edge, NavGraphVertex vertex)
        {
            if (edge.v1 == vertex)
                return edge;
            else
                return new NavGraphEdge(edge.v2, edge.v1, temp: true);
        }

        public static byte EncodeDirection(Vector3 direction)
        {
            if (direction == Vector3.Zero)
                throw new ArgumentException(nameof(direction));

            if (direction.Dot(Vector3.Forward) >= 0.5f)
                return (byte)GameItemRotation.forward;
            if (direction.Dot(Vector3.Right) >= 0.5f)
                return (byte)GameItemRotation.right;
            if (direction.Dot(Vector3.Back) >= 0.5f)
                return (byte)GameItemRotation.backward;
            if (direction.Dot(Vector3.Left) >= 0.5f)
                return (byte)GameItemRotation.left;

            throw new Exception($"Couldn't encode model direction from vector {direction}");
        }

        public static Aabb GetNodeAABB(IGameObjectAsset asset)
        {
            Node3D node = asset.Model3d as Node3D;
            return AABBBuilder.BuildAabb(node);
        }

        public static float GetRadiusFromAABB(Aabb aabb)
        {
            Vector3 size = aabb.Size;
            float max = Mathf.Max(Mathf.Max(size.X, size.Y), size.Z);
            return max / 2;
        }
    }
}

