using UnityEngine;
using System.Collections.Generic;

public class AImanager : MonoBehaviour
{
    [Tooltip("List of possible AI car names.")]
    [SerializeField] private List<string> carNames = new List<string> { "Jeff", "Bob", "Bob the 3rd", "egg", "Joonas Kallio" };

    [Header("AI Car Materials")]
    [Tooltip("Possible materials to assign to AI cars.")]
    [SerializeField] private List<Material> carMaterials;

    private List<AICarController> aiCars = new List<AICarController>();

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

    void Awake()
    {
        // Find all AI cars in the scene at startup
        aiCars.AddRange(FindObjectsByType<AICarController>(FindObjectsSortMode.None));
        AssignNamesToCars();
        AssignCarStatsToCars();
    }

    void Update()
    {
        GetCarsByPlacement(); // This will update currentPlacement for all cars each frame
    }

    private void AssignNamesToCars()
    {
        // Make a copy so we can remove names as we assign them
        List<string> availableNames = new List<string>(carNames);

        for (int i = 0; i < aiCars.Count; i++)
        {
            string assignedName;
            if (availableNames.Count > 0)
            {
                int randomIndex = Random.Range(0, availableNames.Count);
                assignedName = availableNames[randomIndex];
                availableNames.RemoveAt(randomIndex);
            }
            else
            {
                assignedName = $"AI Car {i+1}";
            }
            aiCars[i].carName = assignedName;
        }
    }

    private void AssignCarStatsToCars()
    {
        // Shuffle car list for random assignment
        List<AICarController> shuffled = new List<AICarController>(aiCars);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int j = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        var difficulties = new List<AIDifficulty> { AIDifficulty.Beginner, AIDifficulty.Intermediate, AIDifficulty.Hard };
        int carIndex = 0;
        for (; carIndex < Mathf.Min(3, shuffled.Count); carIndex++)
        {
            var diff = difficulties[carIndex];
            var stats = difficultyRanges[diff];
            float speed = Random.Range(stats.minSpeed, stats.maxSpeed);
            float accel = Random.Range(stats.minAccel, stats.maxAccel);
            shuffled[carIndex].SetMaxSpeed(speed);
            shuffled[carIndex].SetMaxAcceleration(accel);
            shuffled[carIndex].carName += $" ({diff})";

            // Assign random material
            AssignRandomMaterial(shuffled[carIndex]);
        }
        for (; carIndex < shuffled.Count; carIndex++)
        {
            var stats = difficultyRanges[AIDifficulty.Beginner];
            float speed = Random.Range(stats.minSpeed, stats.maxSpeed);
            float accel = Random.Range(stats.minAccel, stats.maxAccel);
            shuffled[carIndex].SetMaxSpeed(speed);
            shuffled[carIndex].SetMaxAcceleration(accel);
            shuffled[carIndex].carName += " (Beginner)";

            // Assign random material
            AssignRandomMaterial(shuffled[carIndex]);
        }
    }

    // Helper method to assign a random material
    private void AssignRandomMaterial(AICarController car)
    {
        if (carMaterials != null && carMaterials.Count > 0)
        {
            var renderer = car.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = carMaterials[Random.Range(0, carMaterials.Count)];
            }
        }
    }

    // Example: Get current race positions (by distance to next waypoint, etc.)
    public List<AICarController> GetCarsByPlacement()
    {
        aiCars.Sort((a, b) =>
        {
            int indexCompare = b.CurrentWaypointIndex.CompareTo(a.CurrentWaypointIndex);
            if (indexCompare != 0)
                return indexCompare;

            if (a.WaypointsPublic != null && b.WaypointsPublic != null &&
                a.CurrentWaypointIndex < a.WaypointsPublic.Count && b.CurrentWaypointIndex < b.WaypointsPublic.Count)
            {
                float aDist = Vector3.Distance(a.transform.position, a.WaypointsPublic[a.CurrentWaypointIndex].position);
                float bDist = Vector3.Distance(b.transform.position, b.WaypointsPublic[b.CurrentWaypointIndex].position);
                return aDist.CompareTo(bDist);
            }
            return 0;
        });

        // Assign placement (1 = first place, etc.)
        for (int i = 0; i < aiCars.Count; i++)
        {
            aiCars[i].currentPlacement = i + 1;
        }

        return aiCars;
    }
}
