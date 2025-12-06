using UnityEngine;

[ExecuteAlways]
public class EditorDisableWaterInEditMode : MonoBehaviour
{
    [Tooltip("Either assign the water GameObject directly, or set the childName to find it under this parent.")]
    [SerializeField] private GameObject waterOverride;
    [SerializeField] private string childName = "waltuh";

    void OnEnable()
    {
        ApplyState();
    }

    void Update()
    {
        // keep state synced in editor and when entering/exiting play mode
        ApplyState();
    }

    private void ApplyState()
    {
        if (!TryGetWater(out var w)) return;

        if (Application.isPlaying)
        {
            // in play mode: keep the water alive
            if (!w.activeSelf) w.SetActive(true);
            return;
        }

        // in editor: disable the water
        if (w.activeSelf) w.SetActive(false);
    }

    private bool TryGetWater(out GameObject found)
    {
        if (waterOverride != null)
        {
            found = waterOverride;
            return true;
        }

        var t = FindChildByName(transform, childName);
        found = t != null ? t.gameObject : null;
        return found != null;
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var deeper = FindChildByName(child, name);
            if (deeper != null) return deeper;
        }
        return null;
    }
}