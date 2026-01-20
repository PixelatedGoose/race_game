using UnityEngine;

public class BillboardObject : MonoBehaviour
{
    [Header("Variation")]
    public bool allowScaling = true;
    public bool allowMirroring = true;

    [Tooltip("Minimum scale increase in percent (0 = original size)")]
    [Range(0f, 1f)] public float minScalePercent = 0f;

    [Tooltip("Maximum scale increase in percent (0.3 = 30% larger)")]
    [Range(0f, 1f)] public float maxScalePercent = 0.3f;

    Vector3 originalScale;
    bool initialized;

    void Awake()
    {
        originalScale = transform.localScale;
        ApplyVariation();
        BillboardManager.Register(this);
    }

    void OnDestroy()
    {
        BillboardManager.Unregister(this);
    }

    void ApplyVariation()
    {
        if (initialized) return;
        initialized = true;

        Vector3 scale = originalScale;

        if (allowScaling)
        {
            float factor = 1f + Random.Range(minScalePercent, maxScalePercent);
            scale *= factor;
        }

        if (allowMirroring && Random.value > 0.5f)
        {
            scale.x *= -1f;
        }

        transform.localScale = scale;
    }
}
