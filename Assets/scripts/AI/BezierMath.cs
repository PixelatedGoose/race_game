using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // May get used later
    public static Vector3[] ComputeBezierPoints(int bezierResolution, Transform path)
    {
        long startTime = DateTime.Now.Ticks;

        List<Vector3> bezierPoints = new();
        Transform[] waypoints = path
            .GetComponentsInChildren<Transform>()
            .Where(t => t != path)
            .ToArray();

        int size = waypoints.Length;
        
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 p0 = waypoints[Mathf.Max(i - 1, 0)].position;
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[i + 1].position;
            Vector3 p3 = waypoints[Mathf.Min(i + 2, waypoints.Length - 1)].position;

            Vector3 b0 = p1;
            Vector3 b1 = p1 + (p2 - p0) / 6f;
            Vector3 b2 = p2 - (p3 - p1) / 6f;
            Vector3 b3 = p2;

            bezierPoints.Add(b0);

            for (int j = 0; j < bezierResolution; j++)
            {
                // Skip duplicate start points
                if (i > 0 && j == 0) continue;

                float t = j / (float)bezierResolution;

                bezierPoints.Add(
                    BezierMath.CalculateBezierPoint(t, path.position.y, b0, b1, b2, b3)
                );
            }
        }

        // Final endpoint
        bezierPoints.Add(waypoints[^1].position);

        Debug.Log($"Bezier points computed in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond} ms");

        return bezierPoints.ToArray();
    }
}