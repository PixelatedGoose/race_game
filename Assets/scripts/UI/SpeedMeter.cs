using UnityEngine;
using UnityEngine.UI;

public class SpeedMeter : MonoBehaviour
{
    public Rigidbody target;

    [Header("float valuet")]
    public float maxSpeed = 0.0f;

    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;

    private float speed;

    [Header("UI")] //kiitos leo's leikkimaa, very cool
    public Text speedLabel;//Leo our favourite gremlin
    public RectTransform arrow;

    private void Start()
    {
        if (speedLabel == null)
        {
            Debug.LogWarning("speedLabel EI OLE VITTU OLEMASSA");
        }

        TryBindTarget();
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnCurrentCarChanged += OnCurrentCarChanged;
    }

    private void OnDisable()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnCurrentCarChanged -= OnCurrentCarChanged;
    }

    private void OnCurrentCarChanged(GameObject currentCar)
    {
        target = currentCar != null ? currentCar.GetComponentInChildren<Rigidbody>() : null;
    }

    private void TryBindTarget()
    {
        if (GameManager.instance == null || GameManager.instance.CurrentCar == null)
            return;

        target = GameManager.instance.CurrentCar.GetComponentInChildren<Rigidbody>();
    }

    private void Update()
    {
        if (target == null)
        {
            TryBindTarget();
            if (target == null)
                return;
        }

        speed = target.linearVelocity.magnitude * 3.6f;

        if (speedLabel != null)
        {
            speedLabel.text = ((int)speed) + " km/h";
        }

        if (arrow != null)
        {
            arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speed / maxSpeed));
        }
    }
}
