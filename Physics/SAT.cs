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
        float projection =
            obb.halfSize.X* MathF.Abs(Vector3.Dot(axis, obb.axes[0])) +
            obb.halfSize.Y * MathF.Abs(Vector3.Dot(axis, obb.axes[1])) +
            obb.halfSize.Z * MathF.Abs(Vector3.Dot(axis, obb.axes[2]));
            
        return projection;
    }

    // Tests if two OBBs collide using SAT.
    // Returns true if collision detected.
    public static bool CheckOBBCollision(OBB a, OBB b, out Vector3 mtv)
    {
        mtv = Vector3.Zero;
        float minOverlap = float.MaxValue;
        Vector3 smallestAxis = Vector3.Zero;

        // 15 potential separating axes for two OBBs:
        Vector3[] axes = new Vector3[15];
        axes[0] = a.axes[0];
        axes[1] = a.axes[1];
        axes[2] = a.axes[2];
        axes[3] = b.axes[0];
        axes[4] = b.axes[1];
        axes[5] = b.axes[2];

        // Cross products of axes
        int index = 6;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                axes[index++] = Vector3.Cross(a.axes[i], b.axes[j]);

        // Vector between centers
        Vector3 t = b.center - a.center;

        for (int i = 0; i < 15; i++)
        {
            Vector3 axis = axes[i];

            // If axis is near zero vector, skip (can't normalize zero vector)
            if (axis.LengthSquared < 1e-6f)
                continue;

            axis = axis.Normalized();

            // Project both OBBs onto axis
            float aProj = ProjectOBB(a, axis);
            float bProj = ProjectOBB(b, axis);

            // Distance between projections
            float distance = Math.Abs(Vector3.Dot(t, axis));

            // Calculate overlap
            float overlap = aProj + bProj - distance;

            // If no overlap, separation axis found -> no collision
            if (overlap <= 0)
                return false;

            // Track smallest overlap axis
            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                smallestAxis = axis;
            }
        }

        // Minimum Translation Vector points from A to B along smallest axis
        Vector3 direction = b.center - a.center;
        if (Vector3.Dot(direction, smallestAxis) < 0)
            smallestAxis = -smallestAxis;

        mtv = smallestAxis * minOverlap;

        return true;
    }
}

