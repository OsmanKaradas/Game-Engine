using OpenTK.Mathematics;

public class AABB
{
    public Vector3 Min;
    public Vector3 Max;

    public AABB(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public AABB()
    {
        Min = Vector3.Zero;
        Max = Vector3.Zero;
    }
    
    public bool Intersects(AABB other)
    {
        return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
               (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
               (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);
    }
}