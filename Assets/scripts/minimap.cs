using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class minimap : MonoBehaviour
{
    public RectTransform minimapRect;
    public RectTransform playerArrowPrefab;
    public RectTransform aiArrowPrefab;
    public Vector2 worldMin = new Vector2(0, 500);
    public Vector2 worldMax = new Vector2(500, 1300);

    [Header("Arrow Offset (as % of minimap size)")]
    [Range(-1f, 1f)] public float arrowOffsetX = 0f;
    [Range(-1f, 1f)] public float arrowOffsetY = 0f;

    private List<RectTransform> arrows = new List<RectTransform>();
    private List<Transform> carTransforms = new List<Transform>();

    void Start()
    {
        arrows.Clear();
        carTransforms.Clear();

        // Player car from GameManager
        var playerCar = GameManager.instance.currentCar;
        var movingCar = playerCar.GetComponentInChildren<CarController>()?.transform;
        if (movingCar != null && movingCar.gameObject.activeInHierarchy)
        {
            carTransforms.Add(movingCar);
            var arrow = Instantiate(playerArrowPrefab, minimapRect);
            arrows.Add(arrow);
        }

        // Find all active AI cars in the scene
        var aiCars = FindFirstObjectByType<AiCarManager>();
        if (aiCars.AiCars != null && aiCars.AiCars.Count > 0)
        {
            foreach (var ai in aiCars.AiCars)
            {
                if (!ai.gameObject.activeInHierarchy) continue;
                if (playerCar != null && ai.gameObject == playerCar) continue;

                carTransforms.Add(ai.carRb.transform);
                var arrow = Instantiate(aiArrowPrefab, minimapRect);
                arrows.Add(arrow);
            }
        }
    }

    void Update()
    {
        Vector2 minimapSize = minimapRect.rect.size;
        Vector2 minimapPivot = minimapRect.pivot;

        Vector2 offset = new Vector2(arrowOffsetX * minimapSize.x, arrowOffsetY * minimapSize.y);

        for (int i = 0; i < carTransforms.Count; i++)
        {
            if (carTransforms[i] == null || !carTransforms[i].gameObject.activeInHierarchy)
            {
                arrows[i].gameObject.SetActive(false);
                continue;
            }
            else
            {
                arrows[i].gameObject.SetActive(true);
            }

            Vector3 carPos = carTransforms[i].position;
            float xNorm = (carPos.x - worldMin.x) / (worldMax.x - worldMin.x);
            float yNorm = (carPos.z - worldMin.y) / (worldMax.y - worldMin.y);

            Vector2 arrowPos = new Vector2(
                xNorm * minimapSize.x,
                yNorm * minimapSize.y
            );
            arrowPos -= minimapSize * minimapPivot;
            arrowPos += offset;

            arrows[i].anchoredPosition = arrowPos;

            float angle = carTransforms[i].eulerAngles.y;
            arrows[i].rotation = Quaternion.Euler(0, 0, -angle);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (minimapRect == null) return;

        // Draw the minimap rect in the Scene view
        Vector3[] corners = new Vector3[4];
        minimapRect.GetWorldCorners(corners);

        // Draw minimap border
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);

        // Draw the offset indicator
        Vector2 minimapSize = minimapRect.rect.size;
        Vector2 minimapPivot = minimapRect.pivot;
        Vector2 offset = new Vector2(arrowOffsetX * minimapSize.x, arrowOffsetY * minimapSize.y);

        // Center of minimap in local space
        Vector2 center = -minimapSize * minimapPivot + minimapSize * 0.5f + offset;
        Vector3 worldCenter = minimapRect.TransformPoint(center);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldCenter, minimapSize.magnitude * 0.01f);
    }

    void OnDrawGizmosSelected()
    {
        // Draw the minimap area in world space
        Gizmos.color = Color.green;
        Vector3 bl = new Vector3(worldMin.x, 0, worldMin.y); // Bottom Left
        Vector3 br = new Vector3(worldMax.x, 0, worldMin.y); // Bottom Right
        Vector3 tr = new Vector3(worldMax.x, 0, worldMax.y); // Top Right
        Vector3 tl = new Vector3(worldMin.x, 0, worldMax.y); // Top Left

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
#endif
}

public class MinimapBoundsHelper : MonoBehaviour
{
    public Transform cornerA;
    public Transform cornerB;

    void OnDrawGizmos()
    {
        if (cornerA && cornerB)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(cornerA.position, new Vector3(cornerA.position.x, cornerA.position.y, cornerB.position.z));
            Gizmos.DrawLine(cornerA.position, new Vector3(cornerB.position.x, cornerA.position.y, cornerA.position.z));
            Gizmos.DrawLine(cornerB.position, new Vector3(cornerA.position.x, cornerB.position.y, cornerB.position.z));
            Gizmos.DrawLine(cornerB.position, new Vector3(cornerB.position.x, cornerB.position.y, cornerA.position.z));
            Debug.Log($"worldMin: new Vector2({Mathf.Min(cornerA.position.x, cornerB.position.x)}, {Mathf.Min(cornerA.position.z, cornerB.position.z)})");
            Debug.Log($"worldMax: new Vector2({Mathf.Max(cornerA.position.x, cornerB.position.x)}, {Mathf.Max(cornerA.position.z, cornerB.position.z)})");
        }
    }
}
