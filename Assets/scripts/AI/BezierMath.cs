using UnityEngine;
using System;

public static class BezierMath
{
    public static Vector3 CalculateBezierPoint(float t, float y, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return new(
            MathF.Pow(1 - t, 2f) * p0.x + 2 * t * (1 - t) * p1.x + MathF.Pow(t, 2f) * p2.x,
            y,
            MathF.Pow(1 - t, 2f) * p0.z + 2 * t * (1 - t) * p1.z + MathF.Pow(t, 2f) * p2.z
        );
    }
    
    public static Vector3 CalculateBezierPoint(float t, float y, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return new(
            MathF.Pow(1 - t, 3f) * p0.x + 3 * MathF.Pow(1 - t, 2f) * t * p1.x + 3 * (1 - t) * MathF.Pow(t, 2f) * p2.x + MathF.Pow(t, 3f) * p3.x,
            y,
            MathF.Pow(1 - t, 3f) * p0.z + 3 * MathF.Pow(1 - t, 2f) * t * p1.z + 3 * (1 - t) * MathF.Pow(t, 2f) * p2.z + MathF.Pow(t, 3f) * p3.z
        );
    }

    public static Vector3 CalculateBezierPoint(float t, float y, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        return new(
            MathF.Pow(1 - t, 4f) * p0.x + 4 * MathF.Pow(1 - t, 3f) * t * p1.x + 6 * MathF.Pow(1 - t, 2f) * MathF.Pow(t, 2f) * p2.x + 4 * (1 - t) * MathF.Pow(t, 3f) * p3.x + MathF.Pow(t, 4f) * p4.x,
            y,
            MathF.Pow(1 - t, 4f) * p0.z + 4 * MathF.Pow(1 - t, 3f) * t * p1.z + 6 * MathF.Pow(1 - t, 2f) * MathF.Pow(t, 2f) * p2.z + 4 * (1 - t) * MathF.Pow(t, 3f) * p3.z + MathF.Pow(t, 4f) * p4.z
        );
    }
}