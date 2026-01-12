using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Serialization
{
    public static class NavGraphSerializer
    {
        private static readonly string verticesEntryName = "vertices";
        private static readonly string edgesEntryName = "edges";

        public static string Serialize(NavGraph navGraph)
        {
            if (navGraph == null)
            {
                return default(string);
            }
            
            Dictionary<NavGraphVertex, int> vertToIndex = new Dictionary<NavGraphVertex, int>();
            List<string> SerializedVertices = new List<string>();
            List<string> SerializedEdges = new List<string>();

            for (int i = 0; i < navGraph.vertices.Count; i++)
            {
                NavGraphVertex vertex = navGraph.vertices[i];
                SerializedVertices.Add(vertex.position.ToString());
                vertToIndex[vertex] = i;
            }

            for (int i = 0; i < navGraph.edges.Count; i++)
            {
                NavGraphEdge edge = navGraph.edges[i];
                int vertIdx1 = vertToIndex[edge.v1];
                int vertIdx2 = vertToIndex[edge.v2];
                SerializedEdges.Add($"({vertIdx1}, {vertIdx2})");
            }

            Dictionary<string, List<string>> serializedNavGraph = new Dictionary<string, List<string>>();
            serializedNavGraph[verticesEntryName] = SerializedVertices;
            serializedNavGraph[edgesEntryName] = SerializedEdges;
            return JsonSerializer.Serialize(serializedNavGraph);
        }

        public static NavGraph Deserialize(string jsonSerializedNavGraph)
        {
            if (jsonSerializedNavGraph == default)
            {
                return null;
            }
            
            Dictionary<string, List<string>> serializedNavGraph = 
                JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonSerializedNavGraph);

            List<NavGraphVertex> vertices = new List<NavGraphVertex>();
            List<NavGraphEdge> edges = new List<NavGraphEdge>();

            List<string> SerializedVertices = serializedNavGraph[verticesEntryName];
            List<string> SerializedEdges = serializedNavGraph[edgesEntryName];

            for (int i = 0; i < SerializedVertices.Count; i++)
            {
                string serializedVertex = SerializedVertices[i];
                if (!TryParseVector3(serializedVertex, out Vector3 point))
                {
                    return null;
                }
                NavGraphVertex navGraphVertex = new NavGraphVertex(point);
                vertices.Add(navGraphVertex);
            }

            for (int i = 0; i < SerializedEdges.Count; i++)
            {
                string serializedEdge = SerializedEdges[i];
                serializedEdge = StripBrackets(serializedEdge);
                string[] indicesStr = serializedEdge.Split(",");
                if (!TryParseIntArray(indicesStr, out int[] indices))
                {
                    return null;
                }
                NavGraphVertex v1 = vertices[indices[0]];
                NavGraphVertex v2 = vertices[indices[1]];
                NavGraphEdge edge = new NavGraphEdge(v1, v2);
                edges.Add(edge);
            }

            return new NavGraph(edges, vertices);
        }

        private static string StripBrackets(string s)
        {
            if (s.StartsWith("("))
            {
                s = s.Substring(1);
            }
            if (s.EndsWith(")"))
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }

        private static bool TryParseVector3(string s, out Vector3 vector)
        {
            s = StripBrackets(s);
            string[] coords = s.Split(",");
            if (coords.Length != 3)
            {
                vector = Vector3.Zero;
                return false;
            }
            
            if (!TryParseFloatArray(coords, out float[] values))
            {
                vector = Vector3.Zero;
                return false;
            }

            vector = new Vector3(values[0], values[1], values[2]);
            return true;
        }

        private static bool TryParseFloatArray(IList<string> strings, out float[] floats)
        {
            float[] result = new float[strings.Count];
            for (int i = 0; i < strings.Count; i++)
            {
                if (!float.TryParse(strings[i].StripEdges(), NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                {
                    floats = null;
                    return false;
                }
                result[i] = value;
            }
            floats = result;
            return true;
        }

        private static bool TryParseIntArray(IList<string> strings, out int[] ints)
        {
            int[] result = new int[strings.Count];
            for (int i = 0; i < strings.Count; i++)
            {
                if (!int.TryParse(strings[i].StripEdges(), NumberStyles.Any, CultureInfo.InvariantCulture, out int value))
                {
                    ints = null;
                    return false;
                }
                result[i] = value;
            }
            ints = result;
            return true;
        }
    }
}
