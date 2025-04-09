using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RacerScript : MonoBehaviour, IDataPersistence
{
    // Public variables
    public RankManager rankManager;
    public GameObject winMenu;
    public GameObject Car1Hud;

    CarInputActions Controls;

    public float laptime;
    public float Rank;
    public float besttime;
    private bool startTimer = false;

    // Lists
    public List<Text> LtimeTexts;
    public List<Text> BtimeTexts;

    // Text UI elements
    public Text rankText;
    public Text LapCounter;
    public Text resetPrompt;

    // Other variables
    public Transform startFinishLine;
    public Transform[] checkpoints;

    private bool[] checkpointStates;

    private int currentLap = 1;
    private int totalLaps = 3;
    public bool raceFinished = false;

    private float inactivityTimer = 0f;
    private float inactivityThreshold = 8f;
    private Vector3 lastPosition;

    private Transform respawnPoint;

    public void LoadData(GameData data)
    {
        if (data != null)
        {
            this.besttime = data.besttime;
            foreach (var btimeText in BtimeTexts)
            {
                btimeText.text = "Record: " + besttime.ToString("F2");
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        data.besttime = this.besttime;
    }

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    private void OnEnable()
    {
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }

    void Start()
    {
        InitializeRace();
    }

    void Update()
    {
        if (raceFinished) return;

        HandleReset();
        Inactivity();
        Ranking(); // Continuously update the rank
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "StartFinish")
        {
            HandleStart();
        }
        else
        {
            HandleCheck(other);
        }
    }

    void InitializeRace()
    {
        LapCounter.text = $"{currentLap}/{totalLaps}";
        resetPrompt.gameObject.SetActive(false);
        lastPosition = transform.position;
        respawnPoint = startFinishLine;

        checkpointStates = new bool[checkpoints.Length];
    }

    void HandleReset()
    {
        if (Controls.CarControls.respawn.triggered)
        {
            ResetPosition();
            resetPrompt.gameObject.SetActive(false);
        }

        foreach (var ltimeText in LtimeTexts)
        {
            ltimeText.text = laptime.ToString("F2");
        }

        if (transform.position.y < -1)
        {
            ResetPosition();
            ResetCarState();
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

    void Inactivity()
    {
        if (startTimer)
        {
            laptime += Time.deltaTime;

            foreach (var ltimeText in LtimeTexts)
            {
                ltimeText.text = laptime.ToString("F2");
            }
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
            resetPrompt.gameObject.SetActive(false);
        }

        lastPosition = transform.position;
    }

    void HandleStart()
    {
        if (!startTimer)
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
            LapCounter.text = $"{currentLap}/{totalLaps}";

            if (currentLap > totalLaps)
            {
                raceFinished = true;
                startTimer = false;

                if (besttime == 0 || laptime < besttime)
                {
                    besttime = laptime;
                }

                foreach (var btimeText in BtimeTexts)
                {
                    btimeText.text = "Record: " + besttime.ToString("F2");
                }

                ResetRace();
            }
            else
            {
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

    void ResetCarState()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
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
            checkpointStates[i] = false;
        }
        respawnPoint = startFinishLine;
    }

    void ResetRace()
    {
        currentLap = 1;
        laptime = 0;
        startTimer = false;
        raceFinished = true;

        respawnPoint = startFinishLine;

        for (int i = 0; i < checkpointStates.Length; i++)
        {
            checkpointStates[i] = false;
        }

        LapCounter.text = $"{currentLap}/{totalLaps}";

        if (winMenu != null)
        {
            winMenu.SetActive(true); 
            raceFinished = true; 
        }

        if (startFinishLine != null)
            startFinishLine.gameObject.SetActive(false);

        if (Car1Hud != null)
            Car1Hud.SetActive(false);
    }

    public void RestartRace()
    {
        if (winMenu != null)
            winMenu.SetActive(false);

        if (startFinishLine != null)
            startFinishLine.gameObject.SetActive(true);

        if (Car1Hud != null)
            Car1Hud.SetActive(true);

        InitializeRace();
        Ranking();
    }

    public void Ranking()
    {
        if (GameManager.instance != null && laptime > 0)
        {
            float score = GameManager.instance.score;
            Rank = score / laptime;

            string assignedRank = rankManager != null ? rankManager.GetRank(Rank) : "N/A";

            rankText.text = $"Rank: {assignedRank} ({Rank:F2})";
        }
        else
        {
            rankText.text = "Rank: N/A";
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}