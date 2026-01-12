using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;

namespace bearloga.addons.Ursula.Modules.InteractiveObjects.DetectorShapes.DetectorShapeVisualiation
{
    public static class SphereDetectorShapeVisualization
    {
        public static MeshInstance3D InstantiateMeshInstance3D(SphereDetectorShape shape, Node parent, Color color = default)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            SphereMesh mesh = new SphereMesh();
            OrmMaterial3D material = new OrmMaterial3D();

            meshInstance.Mesh = mesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            meshInstance.Position = shape.center;

            mesh.Radius = shape.radius;
            mesh.Height = shape.radius * 2;
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
    }
}
