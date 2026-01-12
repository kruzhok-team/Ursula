using Godot;

public interface IDetectorShape
{
    public bool IsDetected(Vector3 point, out float distance);

    public SphereDetectorShape ToStaticSphere();
}
