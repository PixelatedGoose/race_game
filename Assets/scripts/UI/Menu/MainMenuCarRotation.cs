using UnityEngine;

public class MainMenuCarRotation : MonoBehaviour
{
    CarInputActions Controls;
    private Rigidbody car;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    void Start()
    {
        car = gameObject.GetComponent<Rigidbody>();
        Vector3 startRotation = car.transform.eulerAngles;

        LeanTween.value(gameObject, (Vector3 val) => {
            car.transform.eulerAngles = val;
        }, startRotation, new Vector3(0.0f, 360.0f, 0.0f), 8.0f)
        .setLoopClamp();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }

    private void OnDestroy()
    {
        Controls.Disable();
    }
}