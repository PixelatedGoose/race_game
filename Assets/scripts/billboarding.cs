using UnityEngine;
public class billboarding : MonoBehaviour
{
    [Header("Billboard Options")]
    [Tooltip("Enable random scaling on start.")]
    public bool enableScaling = true;
    [Tooltip("Enable random mirroring on start.")]
    public bool enableMirroring = true;
    [Tooltip("Minimum scale factor.")]
    public float minScale = 0.3f;
    [Tooltip("Maximum scale factor.")]
    public float maxScale = 0.6f;

    private Camera bbCamera; // Reference to the billboard camera

    void Start()
    {
        // Find the camera with the tag "Trailercam"
        GameObject camObj = GameObject.FindGameObjectWithTag("Trailercam");
        if (camObj != null)
            bbCamera = camObj.GetComponent<Camera>();
        if (bbCamera == null)
        {
            enabled = false;
            return;
        }

        float randomScale = 1f;
        if (enableScaling)
        {
            randomScale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }

        if (enableMirroring && Random.value > 0.5f)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    void Update()
    {
        if (bbCamera != null)
        {
            // Rotate to face the camera on the Y-axis only
            Vector3 lookDirection = bbCamera.transform.position - transform.position;
            lookDirection.y = 0f; // Only rotate around Y
            if (lookDirection.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            enabled = false;
        }
    }
}