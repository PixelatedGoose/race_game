using System;
using System.Collections.Generic;
using UnityEngine;

public class AdditionalColorChanger : MonoBehaviour
{
    public Light pointLight;

    public Light right;
    public Light left;

    public float duration = 1.0f;
    private float t = 0.0f;
    private float colorTime = 0.0f;

    private void FixedUpdate()
    {
        t = Mathf.PingPong(Time.time / duration, 1.0f);

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
            colorTime += 0.01f;
            
            if (colorTime < 1.0f)
            {
                pointLight.color = Color.red;
            }
            else if (colorTime < 2.0f)
            {
                pointLight.color = Color.green;
            }
            else if (colorTime < 3.0f)
            {
                pointLight.color = Color.blue;
            }
            else
            {
                colorTime = 0.0f;
            }
        }

        left.color = Color.Lerp(Color.red, Color.blue, t);
        right.color = Color.Lerp(Color.red, Color.blue, t);

    }
}