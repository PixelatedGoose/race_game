using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class RacerScript : MonoBehaviour, IDataPersistence
{
    public GameObject winMenu; // Reference to the Win Menu

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


    
    public float laptime;
    public float besttime;
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


    public void LoadData(GameData data)
    {
        if (data != null)
        {
            this.besttime = data.besttime;
            Btime.text = "Record: " + besttime.ToString("F2");
        }
    }

    public void SaveData(ref GameData data)
    {
        data.besttime = this.besttime;
    }



    void Start()
    {
        InitializeRace(); //tekee asiat kun peli alkaa
        // test();
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
        if (Controls.CarControls.respawn.triggered) // respawn to the last checkpoint
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
            LapCounter.text = "" + currentLap + "/" + totalLaps;

            if (currentLap > totalLaps)
            {
                raceFinished = true;
                startTimer = false;
                

                // Update best time after the race is finished
                if (besttime == 0 || laptime < besttime)
                {
                    besttime = laptime;
                }
                Btime.text = "Record: " + besttime.ToString("F2");

                ResetRace();
            }
            else
            {
                // Reset checkpoints for the next lap
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
                checkpointStates[i] = true;
                respawnPoint = checkpoints[i];
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
        laptime = 0; // Reset the timer here
        startTimer = false;
        raceFinished = false;
        respawnPoint = startFinishLine;

        for (int i = 0; i < checkpointStates.Length; i++)
        {
            checkpointStates[i] = false;
        }

        LapCounter.text = "" + currentLap + "/" + totalLaps;
        Debug.Log("Race Reset");

        // Show the Win Menu
        winMenu.SetActive(true);
    }

    public void RestartRace()
    {
        winMenu.SetActive(false); // Hide the Win Menu
        InitializeRace(); // Reinitialize the race
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit(); // Quit the application
    }

    void test()
    {
        print(besttime);
    }
}