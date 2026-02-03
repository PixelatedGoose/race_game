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

    [Header("Pivot Correction")]
    [Tooltip("Rotation offset to correct for misaligned pivots (e.g., X=90 for some trees)")]
    public Vector3 rotationOffset = Vector3.zero;

    Vector3 originalScale;
    float objectHeight;
    bool initialized;

    public Quaternion RotationOffset => Quaternion.Euler(rotationOffset);

    void Awake()
    {
        originalScale = transform.localScale;
        
        // Get height from renderer bounds
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            objectHeight = rend.bounds.size.y;
        }
        
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
        float factor = 1f;

        if (allowScaling)
        {
            factor = 1f + Random.Range(minScalePercent, maxScalePercent);
            scale *= factor;
        }

        if (allowMirroring && Random.value > 0.5f)
        {
            scale.x *= -1f;
        }

        transform.localScale = scale;

        // Compensate for center pivot - lift object so base stays grounded
        if (allowScaling && factor != 1f)
        {
            float heightIncrease = (factor - 1f) * objectHeight * 0.5f;
            transform.position += Vector3.up * heightIncrease;
        }
    }
}
