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
    public TextMeshProUGUI LapCounter;

    private bool checkpoint1 = false;
    private bool checkpoint2 = false;

    private int currentLap = 0;
    private int totalLaps = 3;
    private bool raceFinished = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LapCounter.text = "Lap: " + currentLap + "/" + totalLaps;
    }

    // Update is called once per frame
    void Update()
    {
        if (raceFinished) return;

        Ltime.text = "Time: " + laptime.ToString("F2");

        if (transform.position.y < 0)
        {
            ResetPosition();
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            startTimer = false;
            laptime = 0;
        }

        if (startTimer)
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
        if (other.gameObject.name == "StartFinish")
        {
            if (startTimer == false)
            {
                startTimer = true;
                laptime = 0;
                checkpoint1 = false;
                checkpoint2 = false;
            }

            if (checkpoint1 && checkpoint2)
            {
                currentLap++;
                Debug.Log("Current Lap: " + currentLap);
                LapCounter.text = "Lap: " + currentLap + "/" + totalLaps;

                if (currentLap >= totalLaps)
                {
                    raceFinished = true;
                    startTimer = false;
                    if (besttime == 0 || laptime < besttime)
                    {
                        besttime = laptime;
                    }
                    Btime.text = "Best Time: " + besttime.ToString("F2");
                    Debug.Log("Race Finished!");
                    ResetRace();
                }
                else
                {
                    laptime = 0;
                    checkpoint1 = false;
                    checkpoint2 = false;
                }
            }
        }
        else if (other.gameObject.name == "checkpoint1")
        {
            Debug.Log("chek1");
            checkpoint1 = true;
        }
        else if (other.gameObject.name == "checkpoint2")
        {
            Debug.Log("chek2");
            checkpoint2 = true;
        }
    }

    void ResetRace()
    {
        currentLap = 0;
        laptime = 0;
        startTimer = false;
        raceFinished = false;
        checkpoint1 = false;
        checkpoint2 = false;
        LapCounter.text = "Lap: " + currentLap + "/" + totalLaps;
        Debug.Log("Race Reset");
    }
}
