using UnityEngine;
public class billboarding : MonoBehaviour
{
    private float minScale = 0.3f; // Minimum scale factor
    private float maxScale = 0.6f; // Maximum scale factor
    void Start()
    {
        // Generate a random scale factor between minScale and maxScale
        float randomScale = Random.Range(minScale, maxScale);
        // Apply the random scale uniformly
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        // 50% chance to mirror the tree
        if (Random.value > 0.5f) // Random.value generates a float between 0.0 and 1.0
        {
            // Mirror the tree by flipping the X-axis scale
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    void Update()
    {
        // Keep the tree facing the camera on the Y-axis
        transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
    }
}