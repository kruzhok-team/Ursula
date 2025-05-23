using Godot;
using System;

public static class Frustum
{
    private static Camera3D _camera;

    public static Camera3D GetCamera(Node3D node3D)
    {
        if (_camera == null || !Node.IsInstanceValid(_camera))
            _camera = node3D.GetViewport().GetCamera3D();

        return _camera;
    }

    public static bool In(Node3D obj)
    {
        Camera3D cam = GetCamera(obj);

        var position = obj.GlobalTransform.Origin;


        if (cam != null)
        {
            return cam.IsPositionInFrustum(position);
        }
        else
        {
            return false;
        }
    }

    public static float GetDistance(Node3D obj)
    {
        var cam = GetCamera(obj);
        return obj.GlobalTransform.Origin.DistanceSquaredTo(cam.GlobalTransform.Origin);
    }
}
