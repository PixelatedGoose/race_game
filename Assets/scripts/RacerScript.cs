using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

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
    public bool racestarted = false; // <-- Add this
    private bool startTimer = false;
    private Waitbeforestart waitBeforeStart;

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
    public int CurrentLap => currentLap;

    private bool[] checkpointStates;

    private int currentLap = 1;
    private int totalLaps = 3;
    public bool raceFinished = false;

    private float inactivityTimer = 0f;
    private float inactivityThreshold = 8f;
    private Vector3 lastPosition;

    private Transform respawnPoint;
    private CarController carController;

    private musicControl musicControl;

    public void LoadData(GameData data)
    {
        if (data != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            var sceneBestTime = data.bestTimesByMap
                .FirstOrDefault(scene => scene.sceneName.ToLower() == currentSceneName.ToLower());

            if (sceneBestTime != null)
            {
                besttime = sceneBestTime.bestTime;
                Debug.Log($"Loaded best time for scene {currentSceneName}: {besttime}");
            }
            else
            {
                besttime = 0;
            }

            foreach (var btimeText in BtimeTexts)
            {
                if (GameManager.instance.sceneSelected != "tutorial")
                    btimeText.text = "Record: " + besttime.ToString("F2");
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (besttime > 0)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            DatapersistenceManager.instance.UpdateBestTime(currentSceneName, besttime);
            
        }
        else
        {
            Debug.LogWarning("Best time is 0. Nothing to save.");
        }
    }

    void Awake() //voi olla ongelmallinen!!!
    {
        Controls = new CarInputActions();
        Controls.Enable();
        carController = GetComponent<CarController>();
        musicControl = FindAnyObjectByType<musicControl>();
    }

    private void OnEnable()
    {
        Controls.Enable();
        
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("checkpointTag");
        List<Transform> checkpointsToMove = new List<Transform>();

        foreach (GameObject checkpoint in checkpointObjects)
        {
            Transform checkpointTransform;
            checkpointTransform = checkpoint.GetComponent<Transform>();

            checkpointsToMove.Add(checkpointTransform);
        }
        checkpoints = checkpointsToMove.ToArray();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }

    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();
    }

    void Start()
    {
        InitializeRace();
        racestarted = false; // Ensure race doesn't start until countdown is done
    }

    void Update()
    {
        if (!racestarted || raceFinished) return; // Only run race logic if started

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
        else if (other.gameObject.CompareTag("RespawnTrigger")) // Check for the respawn trigger
        {
            RespawnAtLastCheckpoint();
        }
        else
        {
            HandleCheck(other);
        }
    }

    void RespawnAtLastCheckpoint()
    {
        Debug.Log("Respawning at the last checkpoint...");
        transform.position = respawnPoint != null ? respawnPoint.position : startFinishLine.position;
        transform.rotation = respawnPoint != null ? respawnPoint.rotation : startFinishLine.rotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

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
            if (GameManager.instance.sceneSelected != "tutorial")
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
        rb.linearVelocity = Vector3.zero;//stop the car
        rb.angularVelocity = Vector3.zero;
        
        //StartCoroutine(TurnDownCarsValues());

    }

    // IEnumerator TurnDownCarsValues()
    // {
    //     if (carController == null)
    //     {
    //         carController_2.isTurnedDown = true;

    //         float BasicMaxAcceleration = carController_2.maxAcceleration;
    //         float BasicBaseSpeed = carController_2.basespeed;
    //         float BasicTargetTorque = carController_2.targetTorque;

    //         carController_2.maxAcceleration = 0;
    //         carController_2.basespeed = 0;
    //         carController_2.targetTorque = 0;
    //         GameManager.instance.turbeActive = false;

    //         yield return new WaitForSeconds(0.5f);

    //         carController_2.maxAcceleration = BasicMaxAcceleration;
    //         carController_2.basespeed = BasicBaseSpeed;
    //         carController_2.targetTorque = BasicTargetTorque;
    //         carController_2.isTurnedDown = false;
    //     }
    //     else
    //     {
    //         carController.isTurnedDown = true;

    //         if (carController.maxAcceleration <= 0 || carController.basespeed <= 0 || carController.targetTorque <= 0)
    //         {
               
    //         }

    //         float BasicMaxAcceleration = carController.maxAcceleration;
    //         float BasicBaseSpeed = carController.basespeed;
    //         float BasicTargetTorque = carController.targetTorque;

    //         carController.maxAcceleration = 0;
    //         carController.basespeed = 0;
    //         carController.targetTorque = 0;
    //         GameManager.instance.turbeActive = false;

    //         yield return new WaitForSeconds(0.5f);

    //         carController.maxAcceleration = BasicMaxAcceleration;
    //         carController.basespeed = BasicBaseSpeed;
    //         carController.targetTorque = BasicTargetTorque;
    //         carController.isTurnedDown = false;
    //     }
    // }

    void Inactivity()
    {
        if (startTimer)
        {
            laptime += Time.deltaTime;

            foreach (var ltimeText in LtimeTexts)
            {
                if (GameManager.instance.sceneSelected != "tutorial")
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
        // Save race result FIRST, before resetting anything
        if (RaceResultCollector.instance != null)
        {
            RaceResultCollector.instance.SaveRaceResult();
        }

        // Now reset everything
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
            Button restartButton = winMenu.GetComponentsInChildren<Button>(true)
                .First(b => b.name == "Back_to_Main_Menu");
            restartButton.Select();
            raceFinished = true;
            DatapersistenceManager.instance.SaveGame();
            print("data saved");
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
            if (GameManager.instance.sceneSelected != "tutorial")
                rankText.text = $"Rank: {assignedRank} ({Rank:F2})";
        }
        else
        {
            rankText.text = "Rank: N/A";
        }
    }

    public void StartRace() // <-- Call this from Waitbeforestart
    {
        racestarted = true;
        if (GameManager.instance.sceneSelected != "tutorial")
            musicControl.StartMusicTracks();
        startTimer = true;
    }
}