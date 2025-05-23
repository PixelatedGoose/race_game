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
    private float updateInterval = 0.2f; // Update every 0.1 seconds (10 times per second)
    private float timer = 0f;

    // Angle in degrees for lenient visibility (e.g. 70 for a 60 degree FOV camera)
    [Tooltip("Billboard updates if within this angle from camera forward.")]
    private float lenientAngle = 90f;

    void Start()
    {
        // Find the camera with the tag "Trailercam"
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
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
        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        if (bbCamera != null)
        {
            // Check if within lenient angle
            Vector3 toObject = (transform.position - bbCamera.transform.position).normalized;
            float angle = Vector3.Angle(bbCamera.transform.forward, toObject);
            if (angle > lenientAngle) return;

            // Rotate to face the camera on the Y-axis only
            Vector3 lookDirection = bbCamera.transform.position - transform.position;
            lookDirection.y = 0f; // Only rotate around Y
            if (lookDirection.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(-lookDirection);
        }
        else
        {
            enabled = false;
        }
    }
}