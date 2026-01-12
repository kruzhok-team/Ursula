using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;


namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.PathFinding
{
    public static class NavGraphVertexFinder
    {
        public static NavGraphVertex GetVertex(NavGraph navGraph, Vector3 point, float vertexTolerance)
        {
            float vertexToleranceSqr = vertexTolerance * vertexTolerance;
            if (TryGetClosestVertex(navGraph, point, vertexTolerance, out NavGraphVertex vertex))
                return vertex;
            NavGraphEdge edge = GetClosestEdge(navGraph, point);
            return edge.v2;
        }

        private static NavGraphEdge GetClosestEdge(NavGraph navGraph, Vector3 point)
        {
            NavGraphEdge closestEdge = navGraph.edges[0];
            float distance = float.PositiveInfinity;
            foreach (NavGraphEdge edge in navGraph.edges)
            {
                float curDistance = GetDistanceToEdge(edge, point);
                if (curDistance < distance)
                {
                    distance = curDistance;
                    closestEdge = edge;
                }
            }
            return closestEdge;
        }

        private static float GetDistanceToEdge(NavGraphEdge edge, Vector3 point)
        {
            Vector3 v1 = edge.v1.position;
            Vector3 v2 = edge.v2.position;

            Vector3 edgeVector = v2 - v1;
            Vector3 pointVector = point - v1;

            // if edge is degenerate then return distance between two points
            float edgeLengthSq = edgeVector.LengthSquared();
            if (edgeLengthSq < Mathf.Epsilon)
            {
                return (point - v1).Length();
            }

            // Point projection
            float t = pointVector.Dot(edgeVector) / edgeLengthSq;
            t = Mathf.Clamp(t, 0, 1);
            Vector3 projection = v1 + t * edgeVector;

            return point.DistanceTo(projection);
        }

        private static bool TryGetClosestVertex(NavGraph navGraph, Vector3 point, float vertexToleranceSqr, out NavGraphVertex vertex)
        {
            foreach (NavGraphVertex candidateVertex in navGraph.vertices)
            {
                Vector3 dist = point - candidateVertex.position;
                if (dist.LengthSquared() <= vertexToleranceSqr)
                {
                    vertex = candidateVertex;
                    return true;
                }
            }
            vertex = null;
            return false;
        }
    }
}
