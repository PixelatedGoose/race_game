using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;

// Add AiSpawnPosition prefabs as children 
// to this manager to set spawn positions for AI cars


public class AiCarManager : MonoBehaviour
{
    [Header("Path Settings")]
    [Tooltip("Parent transform containing waypoints for the AI path.")]
    [SerializeField] private Transform path;

    [Header("AI Car Settings")]
    [Tooltip("Number of AI cars to spawn. 0 = no AI cars.")]
    [Range(0, 100)]
    [SerializeField] private byte spawnedAiCarCount = 0;
    [SerializeField] private GameObject[] AiCarPrefabs;
    private BetterNewAiCarController[] aiCars;
    public Transform[] waypoints { get; private set; }

    void Start()
    {
        if (path == null) return;
        waypoints = path.GetComponentsInChildren<Transform>().Where(t => t != path).ToArray();

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
                BetterNewAiCarController aiCar = Instantiate(prefab, 
                spawnPoints[i % spawnPoints.Length].position, 
                transform.rotation)
                .GetComponentInChildren<BetterNewAiCarController>();
                aiCar.Initialize(this, gm.currentCar.GetComponentInChildren<Collider>());
            }
        }

    }
}
