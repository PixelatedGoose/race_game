using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.EditorTools;
using UnityEngine;

public class AiCarManager : MonoBehaviour
{
    public List<Vector3> BezierPoints { get; private set; } = new();
    private BetterNewAiCarController[] aiCars;
    [Header("AI Car Settings")]
    [Tooltip("Number of AI cars to spawn. Optional.")]
    [Range(1, 100)]
    [SerializeField] private byte spawnedAiCarCount = 0;
    [Tooltip("Radius around the manager within which to spawn AI cars.")]
    [Range(1, 100)]
    [SerializeField] private float spawnRadius = 3;
    [SerializeField] private Transform path;
    [Tooltip("Number of points to calculate for Bezier curves per every point (higher = smoother).")]
    [Range(1, 500)]
    [SerializeField] private int bezierResolution = 10;
    [SerializeField] private bool recalculate = true;


    void Start()
    {

        ComputeBezierPoints();
    }

    void FixedUpdate()
    {
        if (recalculate)
        {
            BezierPoints.Clear();
            ComputeBezierPoints();
            recalculate = false;
        }
    }

    // May get used later
    void ComputeBezierPoints()
    {

        Transform[] waypoints = path.GetComponentsInChildren<Transform>().Where(t => t != path).ToArray();
        int size = waypoints.Length - 1;
        for (int i = 0; i < size; i++)
        {
            float t = 0.39f;
            do {
                BezierPoints.Add(BezierMath.CalculateBezierPoint(
                    t,
                    transform.position.y,
                    waypoints[(i - 2 + size) % size].position,
                    waypoints[(i - 1 + size) % size].position,
                    waypoints[i % size].position,
                    waypoints[(i + 1) % size].position, 
                    waypoints[(i + 2) % size].position
                    )
                );
                t += 1f / bezierResolution;
            } while (t <= 0.61f);
        }
    }

    List<Vector3> GenerateMultiPointBezierCurve(Vector3[] points)
    {
        List<Vector3> curvePoints = new();

        // Using a high resolution may cause issues with navigation
        int resolution = Mathf.CeilToInt(bezierResolution * points.Length * 2); // Double the resolution
        for (float t = 0; t <= 1; t += 1.0f / resolution)
        {
            curvePoints.Add(CalculateBezierPoint(points, t));
        }

        // Ensure the final node is included in the curve
        if (curvePoints.Count == 0 || curvePoints[curvePoints.Count - 1] != points[^1])
        {
            curvePoints.Add(points[^1]);
        }

        return curvePoints;
    }

    Vector3 CalculateBezierPoint(Vector3[] points, float t)
    {
        if (points.Length == 1) return points[0];

        Vector3[] nextPoints = new Vector3[points.Length - 1];
        for (int i = 0; i < points.Length - 1; i++)
        {
            nextPoints[i] = Vector3.Lerp(points[i], points[i + 1], t);
        }

        return CalculateBezierPoint(nextPoints, t);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // LIGHT GOLDENROD YELLOW /Ö\
        Gizmos.color = Color.lightGoldenRodYellow;

        Gizmos.DrawWireCube(transform.position, new Vector3(spawnRadius * 2, 1, spawnRadius * 2));

        for (int i = 0; i < BezierPoints.Count() - 1; i++)
        {
            Gizmos.DrawWireSphere(BezierPoints[i], 0.2f);
            Gizmos.DrawLine(BezierPoints[i], BezierPoints[i + 1]);
        }
    }
}
#endif
