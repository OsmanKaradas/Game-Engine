using OpenTK.Mathematics;
using System;

public struct OBB
{
    public Vector3 center;    // Center position in world space
    public Vector3[] axes;    // Local axes in world space (normalized vectors)
    public Vector3 halfSize;  // Half-widths along each local axis

    public OBB(Vector3 center, Vector3 halfSize)
    {
        this.center = center;
        this.halfSize = halfSize;
        this.axes = new Vector3[]
        {
            Vector3.UnitX,
            Vector3.UnitY,
            Vector3.UnitZ
        };
    }
}

public static class SAT
{
    private const float EPSILON = 1e-6f;

    // Projects an OBB onto an axis and returns the radius of that projection
    private static float ProjectOBB(OBB obb, Vector3 axis)
    {
        return
            obb.halfSize.X * MathF.Abs(Vector3.Dot(axis, obb.axes[0])) +
            obb.halfSize.Y * MathF.Abs(Vector3.Dot(axis, obb.axes[1])) +
            obb.halfSize.Z * MathF.Abs(Vector3.Dot(axis, obb.axes[2]));
    }

    // Tests if two OBBs collide using SAT.
    // Returns true if collision detected.
    public static bool OBBvsOBB(OBB a, OBB b)
    {
        // Vector between centers
        Vector3 t = b.center - a.center;

        // The 15 axes to test
        Vector3[] testAxes = new Vector3[15];

        // 3 axes from A
        testAxes[0] = a.axes[0];
        testAxes[1] = a.axes[1];
        testAxes[2] = a.axes[2];

        // 3 axes from B
        testAxes[3] = b.axes[0];
        testAxes[4] = b.axes[1];
        testAxes[5] = b.axes[2];

        // 9 cross product axes
        int idx = 5;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 axis = Vector3.Cross(a.axes[i], b.axes[j]);
                if (axis.LengthSquared > EPSILON)
                    testAxes[++idx] = axis.Normalized();
                else
                    testAxes[++idx] = Vector3.Zero; // Ignore near-zero vectors
            }
        }

        // Check all axes for separation
        foreach (var axis in testAxes)
        {
            if (axis == Vector3.Zero)
                continue;

            float rA = ProjectOBB(a, axis);
            float rB = ProjectOBB(b, axis);

            float dist = MathF.Abs(Vector3.Dot(t, axis));

            if (dist > rA + rB)
                return false; // Separating axis found — no collision
        }

        return true; // No separating axis found — collision detected
    }
}

