using UnityEngine;
using System.Collections.Generic;

public class AImanager : MonoBehaviour
{
    [Tooltip("List of possible AI car names.")]
    [SerializeField] private List<string> carNames = new List<string> { "Jeff", "Bob", "Bob the 3rd", "egg", "Joonas Kallio" };

    private List<AICarController> aiCars = new List<AICarController>();

    void Awake()
    {
        // Find all AI cars in the scene at startup
        aiCars.AddRange(FindObjectsByType<AICarController>(FindObjectsSortMode.None));
        AssignNamesToCars();
        // You can call AssignCarStatsToCars() here in the future
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
