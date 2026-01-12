using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Model
{
    public static class NavGraphCopy
    {
        public static NavGraph Copy(NavGraph navGraph)
        {
            Dictionary<NavGraphVertex, NavGraphVertex> vertexMap = new Dictionary<NavGraphVertex, NavGraphVertex>();
            List<NavGraphVertex> newVertices = new List<NavGraphVertex>();
            List<NavGraphEdge> newEdges = new List<NavGraphEdge>();

            // Copy vertices
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                NavGraphVertex newVertex = new NavGraphVertex(vertex.position);
                if (vertex.ContainsShedule)
                    newVertex.shedule = new NavGraphVertexShedule(vertex.shedule.timeOpen, vertex.shedule.timeClosed, vertex.shedule.offset);
                vertexMap[vertex] = newVertex;
                newVertices.Add(newVertex);
            }

            // Copy shedule groups
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                NavGraphVertex newVertex = vertexMap[vertex];
                if (vertex.sheduleGroup != null && newVertex.sheduleGroup == null)
                {
                    List<NavGraphVertex> newSheduleGroup = new List<NavGraphVertex>();
                    foreach (NavGraphVertex sheduleGroupVertex in vertex.sheduleGroup)
                    {
                        NavGraphVertex newSheduleGroupVertex = vertexMap[sheduleGroupVertex];
                        newSheduleGroup.Add(newSheduleGroupVertex);
                    }

                    foreach (NavGraphVertex sheduleGroupVertex in vertex.sheduleGroup)
                    {
                        NavGraphVertex newSheduleGroupVertex = vertexMap[sheduleGroupVertex];
                        newSheduleGroupVertex.sheduleGroup = newSheduleGroup;
                    }
                }
            }

            // Copy edges
            foreach (NavGraphEdge edge in navGraph.edges)
            {
                NavGraphVertex newVertex1 = vertexMap[edge.v1];
                NavGraphVertex newVertex2 = vertexMap[edge.v2];
                NavGraphEdge newEdge = new NavGraphEdge(newVertex1, newVertex2);
                newEdges.Add(newEdge);
            }

            return new NavGraph(newEdges, newVertices);
        }
    }
}
