using Godot;
using System;

public class Raycaster
{

    //private const float RayLength = 1000.0f;

    public static Action<Node, Vector3> onHitObject;

    public static bool HitByInputEvent(InputEvent @event, Camera3D camera, out Node collider, out Vector3 pos, bool isCheckUI = true)
    {
        if (@event is InputEventMouseButton eventMouseButton)
            return Hit(camera, eventMouseButton.Position, out collider, out pos, isCheckUI);
        else
            return HitFromCenterOfScreen(camera, out collider, out pos, isCheckUI);     
    }

    public static bool HitFromCenterOfScreen(Camera3D camera, out Node collider, out Vector3 pos, bool isCheckUI = true, float RayLength = 1000f)
    {
        Vector2 viewportSize = camera.GetViewport().GetTexture().GetSize();
        Vector2 centerOfScreen = viewportSize / 2;

        return Hit(camera, centerOfScreen, out collider, out pos, isCheckUI, RayLength);
    }

    public static bool Hit(Camera3D camera, Vector2 screenPosition, out Node collider, out Vector3 pos, bool isCheckUI = true, float RayLength = 1000f)
    {
        bool isCheck = (isCheckUI) ? !HoverUI(camera) : true;

        if (isCheck)
        {
            var from = camera.ProjectRayOrigin(screenPosition);
            var to = from + camera.ProjectRayNormal(screenPosition) * RayLength;

            PhysicsDirectSpaceState3D spaceState = camera.GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D p = new PhysicsRayQueryParameters3D();
            p.From = from;
            p.To = to;
            var result = spaceState.IntersectRay(p);
            if (result.Count > 0)
            {
                pos = (Vector3)result["position"];
                collider = (Node)result["collider"];

                return true;
            }
        }

        pos = Vector3.Zero;
        collider = null;
        return false;
    }

    private static bool HoverUI(Camera3D camera)
    {
        var viewport = camera.GetViewport();
        var hover = viewport.GuiGetHoveredControl();
        if (hover != null)
        {
            if (hover is Button or Panel or Slider or Label or OptionButton or ItemList && !hover.Name.ToString().Contains("Cross"))
            {
                return true;
            }
        }

        return false;
    }
}
