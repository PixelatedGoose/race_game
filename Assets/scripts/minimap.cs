using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class minimap : MonoBehaviour
{
    public RectTransform minimapRect;
    public RectTransform playerArrowPrefab;
    public RectTransform aiArrowPrefab;
    public Vector2 worldMin;
    public Vector2 worldMax;

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
        var aiCars = FindObjectsByType<AICarController>(FindObjectsSortMode.None);
        foreach (var ai in aiCars)
        {
            if (!ai.gameObject.activeInHierarchy) continue;
            if (playerCar != null && ai.gameObject == playerCar) continue;

            carTransforms.Add(ai.transform);
            var arrow = Instantiate(aiArrowPrefab, minimapRect);
            arrows.Add(arrow);
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
#endif
}
