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

        //target = GameManager.instance.currentCar.GetComponentInChildren<Rigidbody>();
        //jos rikki, laita inspectorissa ^^^^^^^^^^^^^^^^
        Debug.Log(target);
        Debug.Log("HUOM. jos target ei löydy (eli siitä tulee NullReferenceException), tää on paskana");
    }

    private void Update()
    {
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
