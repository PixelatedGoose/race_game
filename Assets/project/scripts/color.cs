using UnityEngine;
using UnityEngine.InputSystem;

public class ColorChanger : MonoBehaviour
{
    public Light pointLight;

    public Light right;
    public Light left;

    public float duration = 1.0f;


    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

    }

    CarInputActions Controls;



    private void Onable()
    {
        Controls.Enable();
    }

    private void Disable()
    {
        Controls.Disable();
    }


    private void Update()
    {

        if (Controls.CarControls.lights.triggered)
        {
            left.enabled = !left.enabled;
            right.enabled = !right.enabled;
        }

        if (Controls.CarControls.underglow.triggered)
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
