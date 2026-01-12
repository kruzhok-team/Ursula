using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Modules.InteractiveObjects.DetectorShapes.DetectorShapeVisualiation
{
    public static class RectangleDetectorShapeVisualization
    {
        public static MeshInstance3D InstantiateMeshInstance3D(RectangleDetectorShape shape, Node parent, Color color = default)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            BoxMesh mesh = new BoxMesh();
            OrmMaterial3D material = new OrmMaterial3D();

            meshInstance.Mesh = mesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            Vector3 center = (shape.right_down + shape.left_down + shape.left_up + shape.right_up) / 4;
            meshInstance.Position = center;

            float width = shape.right_down.DistanceTo(shape.left_down);
            float depth = shape.right_up.DistanceTo(shape.right_down);
            float height = 1;
            mesh.Size = new Vector3(width, height, depth);
            mesh.Material = material;

            if (color == default)
                color = new Color(0, 0.5f, 0, 1); // Dark green

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;

            if (shape.anchor != null)
                shape.anchor.AddChild(meshInstance);
            else
                parent.AddChild(meshInstance);

            return meshInstance;
        }

        private static Mesh BuildMesh(RectangleDetectorShape shape)
        {
            var vertices = new Vector3[] { shape.left_down, shape.left_up, shape.right_up, shape.right_down };
            var indices = new int[]
            {
            0, 1, 2,
            0, 2, 3
            };

            Vector3 normal = (shape.left_up - shape.left_down).Cross(shape.right_down - shape.left_down).Normalized();
            var normals = new Vector3[] { normal, normal, normal, normal };

            // UV – простые координаты
            var uvs = new Vector2[]
            {
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1)
            };

            var arr = new Godot.Collections.Array { vertices, normals, uvs, indices };

            var mesh = new ArrayMesh();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr);

            return mesh;
        }
    }
}
