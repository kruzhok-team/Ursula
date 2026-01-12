using Godot;

public class SphereDetectorShape : IDetectorShape
{
    public Vector3 center;
    public float radius;
    public Node3D anchor = null;

    public SphereDetectorShape(Node3D relativeTo, float radius, Vector3 center)
    {
        anchor = relativeTo;
        this.radius = radius;
        this.center = center;
    }

    private Vector3 GetCenter()
    {
        if (anchor is MoveScript moveScript)
        {
            Vector3 position = moveScript.GlobalPosition;
            Transform3D transform = moveScript.Transform;
            return transform * center;
        }
        else
        {
            return anchor.GlobalTransform * center;
        }
    }

    public bool IsDetected(Vector3 point, out float distance)
    {
        Vector3 center_after_rotation = GetCenter();

        float dist2 = point.DistanceSquaredTo(center_after_rotation);
        if (dist2 <= radius * radius)
        {
            distance = dist2;
            return true;
        }

        distance = -1.0f;
        return false;
    }

    public SphereDetectorShape ToStaticSphere()
    {
        Vector3 center_after_rotation = GetCenter();
        return new SphereDetectorShape(null, radius, center_after_rotation);
    }
}
