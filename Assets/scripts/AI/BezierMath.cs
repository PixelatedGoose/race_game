using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// Funny bezier maths :D
public static class BezierMath
{
    // quadratic bezier point
    public static Vector3 CalculateBezierPoint(float t, float y, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return new(
            MathF.Pow(1 - t, 2f) * p0.x + 2 * t * (1 - t) * p1.x + MathF.Pow(t, 2f) * p2.x,
            y,
            MathF.Pow(1 - t, 2f) * p0.z + 2 * t * (1 - t) * p1.z + MathF.Pow(t, 2f) * p2.z
        );
    }
    
    // Cubic bezier point
    public static Vector3 CalculateBezierPoint(float t, float y, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return new(
            MathF.Pow(1 - t, 3f) * p0.x + 3 * MathF.Pow(1 - t, 2f) * t * p1.x + 3 * (1 - t) * MathF.Pow(t, 2f) * p2.x + MathF.Pow(t, 3f) * p3.x,
            y,
            MathF.Pow(1 - t, 3f) * p0.z + 3 * MathF.Pow(1 - t, 2f) * t * p1.z + 3 * (1 - t) * MathF.Pow(t, 2f) * p2.z + MathF.Pow(t, 3f) * p3.z
        );
    }

    // Quartic bezier point
    public static Vector3 CalculateBezierPoint(float t, float y, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        return new(
            MathF.Pow(1 - t, 4f) * p0.x + 4 * MathF.Pow(1 - t, 3f) * t * p1.x + 6 * MathF.Pow(1 - t, 2f) * MathF.Pow(t, 2f) * p2.x + 4 * (1 - t) * MathF.Pow(t, 3f) * p3.x + MathF.Pow(t, 4f) * p4.x,
            y,
            MathF.Pow(1 - t, 4f) * p0.z + 4 * MathF.Pow(1 - t, 3f) * t * p1.z + 6 * MathF.Pow(1 - t, 2f) * MathF.Pow(t, 2f) * p2.z + 4 * (1 - t) * MathF.Pow(t, 3f) * p3.z + MathF.Pow(t, 4f) * p4.z
        );
    }

    // Algorithm made by the french EWWWWWWWWWWWWWWW
    // De casteljau's algorithm for finding a point inside any amount of control points
    public static Vector3 CalculateBezierPoint(float t, float y, Vector3[] controlPoints)
    {
        if (controlPoints == null || controlPoints.Count() == 0) return Vector3.zero;

        List<Vector3> points = new(controlPoints);

        while (points.Count() > 1)
        {
            for (int i = 0; i < points.Count() - 1; i++)
            {
                points[i] = Vector3.Lerp(points[i], points[i + 1], t);
            }
            points.RemoveAt(points.Count() - 1);
        }

        return points[0];
    }

    // May get used later
    public static Vector3[] ComputeBezierPoints(int bezierResolution,  Transform path)
    {
        long startTime = DateTime.Now.Ticks;

        List<Vector3> bezierPoints = new();
        Vector3[] waypoints = path
            .GetComponentsInChildren<Transform>()
            .Where(t => t != path).Select(t => t.position)
            .ToArray();

        int size = waypoints.Count();
        float inverseResolution = Mathf.Pow(bezierResolution, -1);

        for (int i = 0; i < waypoints.Count(); i++)
        {
            float t = 0.4f;
            do
            {
                bezierPoints.Add(
                    CalculateBezierPoint(
                        t, 
                        path.position.y, 
                        waypoints[i >= 2 ? i - 2: i - 2 + size],
                        waypoints[i >= 1 ? i - 1: ^1],
                        waypoints[i],
                        waypoints[(i + 1) % size],
                        waypoints[(i + 2) % size]
                        )
                );
                t += inverseResolution;
            } while (t <= 0.6f);
        }

        Debug.Log($"Bezier points computed in {(DateTime.Now.Ticks - startTime) / 10} microseconds");

        return bezierPoints.ToArray();
    }
}