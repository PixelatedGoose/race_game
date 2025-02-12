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
    public TextMeshProUGUI resetPrompt;

    public Transform startFinishLine;
    public Transform[] checkpoints;

    private bool[] checkpointStates;

    private int currentLap = 1;
    private int totalLaps = 3;
    private bool raceFinished = false;

    private float inactivityTimer = 0f;
    private float inactivityThreshold = 8f;
    private Vector3 lastPosition;

    private Transform respawnPoint; 

    void Start()
    {
        InitializeRace(); //tekee asiat kun peli alkaa
    }

    void Update()
    {
        if (raceFinished) return;

        HandleReset(); //spawn ja reset toiminnot
        inactivity(); // inactivity timer
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "StartFinish")
        {
            Handlestart();
        }
        else
        {
            HandleCheck(other);
        }
    }

    void InitializeRace()
    {
        LapCounter.text = "" + currentLap + "/" + totalLaps;
        resetPrompt.gameObject.SetActive(false);
        lastPosition = transform.position;
        respawnPoint = startFinishLine;

        checkpointStates = new bool[checkpoints.Length];
    }

    void HandleReset()
    {
        if (Input.GetKey(KeyCode.R)) // respawn to the last checkpoint
        {
            ResetPosition();
            resetPrompt.gameObject.SetActive(false);
        }

        Ltime.text = "" + laptime.ToString("F2");

        if (transform.position.y < -1)
        {
            ResetPosition();
            ResetCarstate();
        }
    }

    void ResetPosition()
    {
        transform.position = respawnPoint != null ? respawnPoint.position : startFinishLine.position;
        transform.rotation = respawnPoint != null ? respawnPoint.rotation : startFinishLine.rotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("Reset");
    }

    void inactivity()
    {
        if (startTimer)
        {
            laptime += Time.deltaTime;
        }

        if (transform.position == lastPosition)
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold)
            {
                resetPrompt.gameObject.SetActive(true);
            }
        }
        else
        {
            inactivityTimer = 0f;
            resetPrompt.gameObject.SetActive(false); //piilota timer
        }

        lastPosition = transform.position;
    }
    
    void Handlestart()
    {
        if (startTimer == false)
            {
                StartNewLap();
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
                    checkpointStates[i] = false;
                }
                respawnPoint = startFinishLine;
            }
        }
    }

    void HandleCheck(Collider other)
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

    void ResetCarstate()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        startTimer = false;
        laptime = 0;
    }

    void StartNewLap()
    {
        startTimer = true;
        laptime = 0;
        for (int i = 0; i < checkpointStates.Length; i++)
        {
            checkpointStates[i] = false; // Reset all checkpoints
        }
        respawnPoint = startFinishLine; // Set respawn point to finish line
    }

    void ResetRace()
    {
        currentLap = 1;
        laptime = 0;
        startTimer = false;
        raceFinished = false;
        respawnPoint = startFinishLine;
        for (int i = 0; i < checkpointStates.Length; i++)
        {
            checkpointStates[i] = false;
        }
        LapCounter.text = "" + currentLap + "/" + totalLaps;
        Debug.Log("Race Reset");
    }
}