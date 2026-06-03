using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.Collections.Generic;



public class GameManager : MonoBehaviour
{
    public static CarInputActions Controls;
    public static GameManager instance;
    public static RacerScript racerscript;
    public static SFXManager sfx;
    public GameObject CarUI;

    public static bool IsPaused => Time.timeScale == 0;

    public static GameObject CurrentCar { get; private set; }
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform reverse_playerSpawn;
    [SerializeField] private GameObject[] cars;
    #if UNITY_EDITOR
    [SerializeField] private int carSpawnIndex;
    [SerializeField] private bool spawnFromIndex = false;
    #endif
    [NonSerialized] public static HashSet<BaseCarController> spawnedCars = new();

    public static string SceneSelected => SceneManager.GetActiveScene().name;
    public static readonly string[] maps = new string[]
    {
        "shoreline",
        "shoreline_night",
        "canyon",
        "canyon_night"
    };

    void Awake()
    {
        instance = this;
        spawnedCars.Clear();
        Controls = new();
        Controls.Enable();

        if (maps.Contains(SceneSelected) && cars.Length > 0)
        {
            GameObject selectedCar = cars.FirstOrDefault(c => c.name == PlayerPrefs.GetString("SelectedCar"));
            if (selectedCar == null) selectedCar = cars[0];
            Transform spawn = PlayerPrefs.GetInt("Reverse") == 1 ? reverse_playerSpawn : playerSpawn;
            
            #if UNITY_EDITOR
                Controls.CarControls.Debug_Win.performed += context => ManualRaceEnd();

                if (spawnFromIndex) selectedCar = cars[carSpawnIndex];
            #endif
            
            CurrentCar = Instantiate(selectedCar, spawn.position, spawn.rotation);
            racerscript = CurrentCar.GetComponentInChildren<RacerScript>();
            spawnedCars.Add(CurrentCar.GetComponentInChildren<BaseCarController>());
            sfx = FindAnyObjectByType<SFXManager>();
            #if UNITY_EDITOR
                Controls.CarControls.Debug_Win.performed += context => ManualRaceEnd();
            #endif
            
        }
    }

        void OnDisable()
        {
            Controls.Disable();
            #if UNITY_EDITOR
                Controls.CarControls.Debug_Win.performed -= context => ManualRaceEnd();
            #endif
        }
        void OnDestroy()
        {
            Controls.Disable();
            #if UNITY_EDITOR
                Controls.CarControls.Debug_Win.performed -= context => ManualRaceEnd();
            #endif
        }

    public void ManualRaceEnd()
    {
        if (racerscript.raceFinished) return;
        StartCoroutine(racerscript.EndRace());
    }
}