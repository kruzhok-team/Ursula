using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization
{
    public static class NavGraphEdgeVisualization
    {
        public static MeshInstance3D InstantiateMeshInstance3D(NavGraphEdge edge, Node parent, float heightOffset, Color color = default)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            ImmediateMesh mesh = new ImmediateMesh();
            OrmMaterial3D material = new OrmMaterial3D();

            meshInstance.Mesh = mesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            Vector3 direction = edge.v2.position - edge.v1.position;
            direction = direction.Normalized();
            Vector3 right = Vector3.Up.Cross(direction);

            float arrowOffset = 1f;
            Vector3 p1 = edge.v1.position;
            Vector3 p2 = edge.v2.position;
            Vector3 arrowBase = p2 - direction * arrowOffset;
            Vector3 arrowRight = arrowBase + right * arrowOffset / 2;
            Vector3 arrowLeft = arrowBase - right * arrowOffset / 2;

            mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
            mesh.SurfaceAddVertex(p1);
            mesh.SurfaceAddVertex(p2);
            mesh.SurfaceAddVertex(p2);
            mesh.SurfaceAddVertex(arrowRight);
            mesh.SurfaceAddVertex(p2);
            mesh.SurfaceAddVertex(arrowLeft);
            mesh.SurfaceEnd();

            
            direction = (direction + Vector3.One) / 2;
            if (color == default)
                color = new Color(direction.X, direction.Y, direction.Z, 1);

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;

            parent.AddChild(meshInstance);
            meshInstance.GlobalPosition = Vector3.Up * heightOffset;

            return meshInstance;
        }
    }
}
