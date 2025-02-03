using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RacerScript : MonoBehaviour
{

    public float laptime;
    public float besttime = 0;
    private bool startTimer = false;

    public TextMeshProUGUI Ltime;
    public TextMeshProUGUI Btime;

    private bool checkpoint1 = false;
    private bool checkpoint2 = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ltime.text = "Time: " + laptime.ToString("F2");

        if (Input.GetKey(KeyCode.R)) {
            ResetPosition();
        }

        if(transform.position.y < -1)
        {
            ResetPosition();
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            startTimer = false;
            laptime = 0;
        }

        if(startTimer == true) 
        {
            laptime += Time.deltaTime;
        }
    }

    void ResetPosition()
    {
        transform.position = new Vector3(255, 0.0015f, 845);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("Reset");
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "StartFinish")
        {
            if (startTimer == false)
            {
                startTimer = true;
                laptime = 0;
                checkpoint1 = false;
                checkpoint2 = false;
            }

            if(checkpoint1 == true && checkpoint2 == true)
            {
                startTimer = false;

                if(besttime == 0)
                {
                    besttime = laptime;
                }
                if(laptime<besttime)
                {
                    besttime = laptime;
                }

                Btime.text = "Best: " + besttime.ToString("F2");
            }
            
        }


        if(other.gameObject.name == "checkpoint1")
        {
            Debug.Log("chek1");
            checkpoint1 = true;
        }

        if(other.gameObject.name == "checkpoint2")
        {
            Debug.Log("chek2");
            checkpoint2 = true;
        }

    }
}
