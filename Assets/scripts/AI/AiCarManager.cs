using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Add AiSpawnPosition prefabs as children 
// to this manager to set spawn positions for AI cars


public class AiCarManager : MonoBehaviour
{
    [Header("AI Car Settings")]
    [SerializeField] private bool enableAiCars = true;
    [Tooltip("Number of AI cars to spawn. Optional.")]
    [Range(1, 100)]
    [SerializeField] private byte spawnedAiCarCount = 0;
    [SerializeField] private Transform path;
    [Tooltip("Density for bezier points (higher = smoother curve).")]
    [Range(1, 500)]
    [SerializeField] private int bezierResolution = 10;
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private GameObject[] AiCarPrefabs;
    private float bezierHeight;
    public List<Vector3> BezierPoints { get; private set; } = new();
    private BetterNewAiCarController[] aiCars;

    void Start()
    {
        // Height for Bezier curves
        bezierHeight = Physics.RaycastAll(transform.position + Vector3.up * 50, Vector3.down, 100).OrderBy(hit => hit.distance).First().point.y;
        ComputeBezierPoints();

        GameManager gm = GameManager.instance;
        if (gm == null || gm.currentCar == null) return;

        // Spawn AI Cars at spawn points
        if (spawnOnStart)
        {
            // Find Spawn points in children
            Transform[] spawnPoints = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
            
            // Iterate through spawn points
            for (int i = 0; i < spawnedAiCarCount; i++)
            {
                // Get a random prefab from the list
                GameObject prefab = AiCarPrefabs[UnityEngine.Random.Range(0, AiCarPrefabs.Length)];
                
                // Spawn the AI car
                BetterNewAiCarController aiCar = Instantiate(prefab, 
                spawnPoints[i % spawnPoints.Length].position, 
                transform.rotation)
                .GetComponentInChildren<BetterNewAiCarController>();
                aiCar.Initialize(this, gm.currentCar.GetComponentInChildren<Collider>());
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
                    bezierHeight,
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
