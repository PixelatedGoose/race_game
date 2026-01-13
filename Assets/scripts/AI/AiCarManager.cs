using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;

// Add AiSpawnPosition prefabs as children 
// to this manager to set spawn positions for AI cars

[RequireComponent(typeof(BezierBaker))]
public class AiCarManager : MonoBehaviour
{
    [Header("AI Car Settings")]
    [Tooltip("Number of AI cars to spawn. 0 = no AI cars.")]
    [Range(0, 100)]
    [SerializeField] private byte spawnedAiCarCount = 0;
    [SerializeField] private GameObject[] AiCarPrefabs;
    private HashSet<BetterNewAiCarController> aiCars;
    public Vector3[] Waypoints { get; private set; }
    
    private enum AIDifficulty { Beginner, Intermediate, Hard }

    private struct DifficultyStats
    {
        public float minSpeed, maxSpeed, minAccel, maxAccel;
        public DifficultyStats(float minS, float maxS, float minA, float maxA)
        {
            minSpeed = minS; maxSpeed = maxS; minAccel = minA; maxAccel = maxA;
        }
    }

    private readonly Dictionary<AIDifficulty, DifficultyStats> difficultyRanges = new()
    {
        { AIDifficulty.Beginner,     new DifficultyStats(105f, 115f, 240f, 290f) },
        { AIDifficulty.Intermediate, new DifficultyStats(120f, 130f, 270f, 290f) },
        { AIDifficulty.Hard,         new DifficultyStats(130f, 140f, 280f, 300f) }
    };

    void Start()
    {
        BezierBaker bezierBaker = GetComponent<BezierBaker>();
        Waypoints = bezierBaker.GetCachedPoints();

        GameManager gm = GameManager.instance;
        if (gm == null || gm.currentCar == null) return;

        // Spawn AI Cars at spawn points
        if (spawnedAiCarCount > 0)
        {
            // Find Spawn points in children
            Transform[] spawnPoints = GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
            
            // Iterate through spawn points
            for (int i = 0; i < spawnedAiCarCount; i++)
            {
                // Get a random prefab from the list
                GameObject prefab = AiCarPrefabs[UnityEngine.Random.Range(0, AiCarPrefabs.Length)];
                
                // Spawn the AI car
                BetterNewAiCarController betterNewAiCarController = Instantiate(prefab, spawnPoints[i % spawnPoints.Length].position, transform.rotation)
                .GetComponentInChildren<BetterNewAiCarController>();

                betterNewAiCarController.Initialize(this, gm.currentCar.GetComponentInChildren<Collider>());
            }
        }
    }


}
