using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Modules.InteractiveObjects.DetectorShapes.DetectorShapeVisualiation
{
    public class DetectorShapeVisualization
    {
        MeshInstance3D instance;

        public void Draw(IDetectorShape shape, Node parent)
        {
            if (shape is SphereDetectorShape sphere)
            {
                instance = SphereDetectorShapeVisualization.InstantiateMeshInstance3D(sphere, parent);
            }
            if (shape is RectangleDetectorShape rectangle)
            {
                instance = RectangleDetectorShapeVisualization.InstantiateMeshInstance3D(rectangle, parent);
            }
        }

        public void Hide()
        {
            instance?.QueueFree();
            instance = null;
        }
    }
}
