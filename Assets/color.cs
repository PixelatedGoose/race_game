using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public Light pointLight;

    public Light right;
    public Light left;

    public float duration = 1.0f;

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.L))
        {
            left.enabled = !left.enabled;
            right.enabled = !right.enabled;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            pointLight.enabled = !pointLight.enabled;
        }

        if (pointLight.enabled)
        {
            float t = Mathf.PingPong(Time.time / duration, 1.0f);
            pointLight.color = Color.Lerp(Color.red, Color.blue, t);
        }
    }
}
