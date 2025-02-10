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
    public TextMeshProUGUI resetPrompt; // Add a TextMeshProUGUI for the reset prompt

    public Transform startFinishLine; // Add a Transform variable for the start/finish line
    public Transform[] checkpoints; // Add an array of Transforms for the checkpoints

    private bool[] checkpointStates; // Add an array to track checkpoint states

    private int currentLap = 1;
    private int totalLaps = 3;
    private bool raceFinished = false;

    private float inactivityTimer = 0f;
    private float inactivityThreshold = 8f;
    private Vector3 lastPosition;

    private Transform respawnPoint; // Add a Transform variable for the respawn point

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LapCounter.text = "" + currentLap + "/" + totalLaps;
        resetPrompt.gameObject.SetActive(false); // Hide the reset prompt initially
        lastPosition = transform.position;
        respawnPoint = startFinishLine; // Initialize respawn point to start/finish line

        checkpointStates = new bool[checkpoints.Length]; // Initialize checkpoint states array
    }

    // Update is called once per frame
    void Update()
    {
        if (raceFinished) return;

        if (Input.GetKey(KeyCode.R))
        {
            ResetPosition();
            resetPrompt.gameObject.SetActive(false); // Hide the reset prompt when resetting
        }

        Ltime.text = "" + laptime.ToString("F2");

        if (transform.position.y < -1)
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

        // Check for inactivity
        if (transform.position == lastPosition)
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold)
            {
                resetPrompt.gameObject.SetActive(true); // Show the reset prompt
            }
        }
        else
        {
            inactivityTimer = 0f;
            resetPrompt.gameObject.SetActive(false); // Hide the reset prompt
        }

        lastPosition = transform.position;
    }

    void ResetPosition()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }
        else
        {
            transform.position = startFinishLine.position;
            transform.rotation = startFinishLine.rotation;
        }

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
                for (int i = 0; i < checkpointStates.Length; i++)
                {
                    checkpointStates[i] = false; // Reset all checkpoint states
                }
                respawnPoint = startFinishLine; // Set respawn point to start/finish line
            }

            bool allCheckpointsPassed = true;
            for (int i = 0; i < checkpointStates.Length; i++)
            {
                if (!checkpointStates[i])
                {
                    allCheckpointsPassed = false;
                    break;
                }
            }

            if (allCheckpointsPassed)
            {
                currentLap++;
                Debug.Log("Current Lap: " + currentLap);
                LapCounter.text = "" + currentLap + "/" + totalLaps;
                if (besttime == 0 || laptime < besttime)
                {
                    besttime = laptime;
                }
                Btime.text = "Record: " + besttime.ToString("F2");
                
                if (currentLap > totalLaps)
                {
                    raceFinished = true;
                    startTimer = false;
                    Debug.Log("Race Finished!");
                    ResetRace();
                }
                else
                {
                    laptime = 0;
                    for (int i = 0; i < checkpointStates.Length; i++)
                    {
                        checkpointStates[i] = false; // Reset all checkpoint states
                    }
                    respawnPoint = startFinishLine; // Reset respawn point to start/finish line
                }
            }
        }
        else
        {
            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (other.transform == checkpoints[i])
                {
                    Debug.Log("Checkpoint " + (i + 1) + " reached");
                    checkpointStates[i] = true;
                    respawnPoint = checkpoints[i]; // Set respawn point to the current checkpoint
                    break;
                }
            }
        }
    }

    void ResetRace()
    {
        currentLap = 1;
        laptime = 0;
        startTimer = false;
        raceFinished = false;
        respawnPoint = startFinishLine; // Reset respawn point to start/finish line
        for (int i = 0; i < checkpointStates.Length; i++)
        {
            checkpointStates[i] = false; // Reset all checkpoint states
        }
        LapCounter.text = "" + currentLap + "/" + totalLaps;
        Debug.Log("Race Reset");
    }
}