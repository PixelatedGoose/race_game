using UnityEngine;

public class camerathing : MonoBehaviour
{
    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0); // Transparent
    }
}
