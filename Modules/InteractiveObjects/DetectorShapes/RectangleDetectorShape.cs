using System;
using Godot;

public class RectangleDetectorShape : IDetectorShape
{
    public Vector3 left_down;
    public Vector3 left_up;
    public Vector3 right_down;
    public Vector3 right_up;

    public Vector3 offset;
    public float width;
    public float height;

    public Node3D anchor;
    
    public RectangleDetectorShape(Node3D relativeTo, float width, float height, Vector3 offset)
    {
        left_down = new Vector3(-width / 2, 0, -height / 2) + offset;
        left_up = new Vector3(-width / 2, 0, height / 2) + offset;
        right_down = new Vector3(width / 2, 0, -height / 2) + offset;
        right_up = new Vector3(width / 2, 0, height / 2) + offset;
        this.offset = offset;
        this.width = width;
        this.height = height;
        anchor = relativeTo;
    }

    private Vector3 GetCenter()
    {
        if (anchor is MoveScript moveScript)
        {
            Vector3 position = moveScript.GlobalPosition;
            Transform3D transform = moveScript.Transform;
            return transform * offset;
        }
        else
        {
            return anchor.GlobalTransform * offset;
        }
    }

    public bool IsDetected(Vector3 point, out float distance)
    {
        //NOTE Pitch rotation is not ignored. If 3D actor is rotated in Pitch axis rectangle will be shrinked
        //NOTE make detection like box collider? Need to discuss 
        
        Vector3 point_copy = new Vector3(point.X, 0, point.Z);

        Transform3D anchorTransform;
        if (anchor is MoveScript moveScript)
        {
            anchorTransform = moveScript.Transform;
        }
        else
        {
            anchorTransform = anchor.GlobalTransform;
        }

        Vector3 left_down_after_rotation = anchorTransform * left_down;
        Vector3 left_up_after_rotation = anchorTransform * left_up;
        Vector3 right_down_after_rotation = anchorTransform * right_down;
        Vector3 right_up_after_rotation = anchorTransform * right_up;

        Vector3 left_side = left_up_after_rotation - left_down_after_rotation;
        Vector3 up_side = right_up_after_rotation - left_up_after_rotation;
        Vector3 right_side = right_down_after_rotation - right_up_after_rotation;
        Vector3 down_side = left_down_after_rotation - right_down_after_rotation;
        
        Vector3 point_left = left_down_after_rotation - point_copy;
        Vector3 point_up = left_up_after_rotation - point_copy;
        Vector3 point_right = right_up_after_rotation - point_copy;
        Vector3 point_down = right_down_after_rotation - point_copy;

        float cross_left = point_left.Cross(left_side).Y;
        float cross_up = point_up.Cross(up_side).Y;
        float cross_right = point_right.Cross(right_side).Y;
        float cross_down = point_down.Cross(down_side).Y;

        if (cross_left < 0 || cross_up < 0 || cross_right < 0 || cross_down < 0)
        {
            distance = -1.0f;
            return false;
        }

        distance = 1.0f;
        return true;
    }

    public SphereDetectorShape ToStaticSphere()
    {
        Vector3 center_after_rotation = GetCenter();
        float radius = Mathf.Sqrt((width / 2) * (width / 2) + (height / 2) * (height / 2));
        return new SphereDetectorShape(null, radius, center_after_rotation);
    }
}
