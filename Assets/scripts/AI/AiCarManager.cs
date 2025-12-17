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
    [Tooltip("Length around the manager's position to spawn AI cars within.")]
    [Range(1, 100)]
    [SerializeField] private float spawnLenght = 3;
    [Tooltip("Width around the manager's position to spawn AI cars within.")]
    [Range(1, 100)]
    [SerializeField] private float spawnWidth = 3;
    private Vector3 spawnPosition;
    [SerializeField] private Transform path;
    [Tooltip("Number of points to calculate for Bezier curves per every point (higher = smoother).")]
    [Range(1, 500)]
    [SerializeField] private int bezierResolution = 10;
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private GameObject[] aiCarPrefabs;


    void Start()
    {
        spawnPosition = Physics.RaycastAll(transform.position + Vector3.up * 50, Vector3.down, 100).OrderBy(hit => hit.distance).First().point;
        // Get a random prefab from the list
        if (spawnOnStart)
        {
            for (int i = 0; i < spawnedAiCarCount; i++)
            {
                GameObject prefab = aiCarPrefabs[UnityEngine.Random.Range(0, aiCarPrefabs.Length)];
                Vector3 randomOffset = new(
                    UnityEngine.Random.Range(-spawnLenght, spawnLenght),
                    0,
                    UnityEngine.Random.Range(-spawnWidth, spawnWidth)
                );
                Instantiate(prefab, spawnPosition + randomOffset, transform.rotation);
            }
        }
        ComputeBezierPoints();
    }

    // May get used later
    void ComputeBezierPoints()
    {

        Transform[] waypoints = path.GetComponentsInChildren<Transform>().Where(t => t != path).ToArray();
        int size = waypoints.Length - 1;
        for (int i = 0; i < size; i++)
        {
            float t = 0.40f;
            do {
                BezierPoints.Add(BezierMath.CalculateBezierPoint(
                    t,
                    spawnPosition.y,
                    waypoints[(i - 2 + size) % size].position,
                    waypoints[(i - 1 + size) % size].position,
                    waypoints[i % size].position,
                    waypoints[(i + 1) % size].position, 
                    waypoints[(i + 2) % size].position
                    )
                );
                t += 1f / bezierResolution;
            } while (t <= 0.60f);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // LIGHT GOLDENROD YELLOW /Ö\
        Gizmos.color = Color.lightGoldenRodYellow;

        Gizmos.DrawWireCube(transform.position, new Vector3(spawnLenght * 2, 1, spawnWidth * 2));

        for (int i = 0; i < BezierPoints.Count() - 1; i++)
        {
            Gizmos.DrawWireSphere(BezierPoints[i], 0.2f);
            Gizmos.DrawLine(BezierPoints[i], BezierPoints[i + 1]);
        }
    }
}
#endif
