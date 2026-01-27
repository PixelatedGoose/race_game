using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.EditorTools;
using UnityEngine;

// Change to .cs if you want to use these scripts

// Add AiSpawnPosition prefabs as children to this manager to set spawn positions for AI cars


public class BunnyAiManager : MonoBehaviour
{
    #pragma warning disable 0414
    public List<Vector3> BezierPoints { get; private set; } = new();
    private AiCarController[] aiCars;
    [Header("AI Car Settings")]
    [SerializeField] private bool enableAiCars = true;
    [Tooltip("Number of AI cars to spawn. Optional.")]
    [Range(1, 100)]
    [SerializeField] private byte spawnedAiCarCount = 0;
    private Vector3 spawnPosition;
    [SerializeField] private Transform path;
    [Tooltip("Number of points to calculate for Bezier curves per every point (higher = smoother).")]
    [Range(1, 500)]
    [SerializeField] private int bezierResolution = 10;
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private GameObject[] AiCarPrefabs;



    void Start()
    {
        ComputeBezierPoints();

        GameManager gm = GameManager.instance;
        if (gm == null || gm.currentCar == null) return;

        Collider playerCollider = gm.currentCar.GetComponentInChildren<Collider>();

        spawnPosition = Physics.RaycastAll(transform.position + Vector3.up * 50, Vector3.down, 100).OrderBy(hit => hit.distance).First().point;
        if (spawnOnStart)
        {
            Transform[] spawnPoints = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
            
            for (int i = 0; i < spawnedAiCarCount; i++)
            {
                // Get a random prefab from the list
                GameObject prefab = AiCarPrefabs[UnityEngine.Random.Range(0, AiCarPrefabs.Length)];
                BunnyAiCars aiCar = Instantiate(prefab, 
                spawnPoints[i % spawnPoints.Length].position, 
                transform.rotation)
                .GetComponentInChildren<BunnyAiCars>();
                aiCar.Initialize(this, playerCollider);
            }
        }

    }

    // May get used later
    void ComputeBezierPoints()
    {
        Transform[] waypoints = path.GetComponentsInChildren<Transform>().Where(t => t != path).ToArray();
        int size = waypoints.Length - 1;
        for (int i = 0; i < size; i++)
        {
            for (float t = 0.4f; t <= 0.6f; t += 1f / bezierResolution)
            {
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
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // LIGHT GOLDENROD YELLOW /Ö\
        Gizmos.color = Color.lightGoldenRodYellow;

        for (int i = 0; i < BezierPoints.Count() - 1; i++)
        {
            Gizmos.DrawWireSphere(BezierPoints[i], 0.2f);
            Gizmos.DrawLine(BezierPoints[i], BezierPoints[i + 1]);
        }
    }
}
#endif
