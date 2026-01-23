using UnityEngine;

public class AdditionalColorChanger : MonoBehaviour
{
    public Light pointLight;

    public Light right;
    public Light left;

    public float duration = 1.0f;
    private float t = 0.0f;
    private float colorTime = 0.0f;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            Light childLight = child.GetComponent<Light>();

            if (child.CompareTag("pl"))
            {
                if (childLight != null)
                {
                    pointLight = childLight;
                }
            }
            else if (child.CompareTag("rl"))
            {
                if (childLight != null)
                {
                    right = childLight;
                }
            }
            else if (child.CompareTag("ll"))
            {
                if (childLight != null)
                {
                    left = childLight;
                }
            }
        }

        Color amisPink = new(0.945f, 0.318f, 0.831f, 1.0f);
        Color amisGreen = new(0.129f, 0.933f, 0.541f, 1.0f);
        float tweenDuration = 1.7f;

        LeanTween.value(left.gameObject, amisGreen, amisPink, tweenDuration)
            .setOnUpdate((Color val) => { if (left != null) left.color = val; })
            .setLoopPingPong();
        LeanTween.value(right.gameObject, amisPink, amisGreen, tweenDuration)
            .setOnUpdate((Color val) => { if (right != null) right.color = val; })
            .setLoopPingPong();
    }

    private void FixedUpdate()
    {
        t = Mathf.PingPong(Time.time / duration, 1.0f);

        if (pointLight.enabled)
        {
            colorTime += 0.01f;

            if (colorTime < 0.6f)
            {
                pointLight.color = Color.red;
            }
            else if (colorTime < 1.2f)
            {
                pointLight.color = Color.green;
            }
            else if (colorTime < 1.8f)
            {
                pointLight.color = Color.blue;
            }
            else
            {
                colorTime = 0.0f;
            }
        }
    }

    public void CheckLightState()
    {
        bool lightsOptionEnabled = PlayerPrefs.GetInt("optionTest_value") == 1; //lights?

        pointLight.enabled = lightsOptionEnabled;
        left.enabled = lightsOptionEnabled;
        right.enabled = lightsOptionEnabled;
    }
}