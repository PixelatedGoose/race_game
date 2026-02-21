using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;



public class GameManager : MonoBehaviour, IDataPersistence
{
    public static GameManager instance;
    public static RacerScript racerscript;

    [Header("score systeemi")]
    public int score;

    public float scoreAddWT = 0.01f; //WT = wait time

    public bool isAddingPoints = false;

    public float scoreamount = 0;

    [Header("menut")]
    public bool isPaused = false;

    [Header("car selection")]
    public GameObject CurrentCar { get; private set; }
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform reverse_playerSpawn;
    [SerializeField] private GameObject[] cars;

    [Header("scene asetukset")]
    public string sceneSelected;
    private string[] maps = new string[]
    {
        "haukipudas",
        "haukipudas_night",
        "ai_haukipudas",
        "ai_haukipudas_night",
        "tutorial",
        "canyon",
        "canyon_night",
        "ai_canyon",
        "ai_canyon_night"
    };
    
    [Header("auto")]
    public float carSpeed;
    public bool turbeActive = false;
    void Awake()
    {
        instance = this;

        sceneSelected = SceneManager.GetActiveScene().name;

        if (sceneSelected == "tutorial") CurrentCar = GameObject.Find("REALCAR");
        else if (maps.Contains(sceneSelected) && cars.Length > 0)
        {
            GameObject selectedCar = cars.FirstOrDefault(c => c.name == PlayerPrefs.GetString("SelectedCar"));
            if (selectedCar == null) selectedCar = cars[0];
            Transform spawn = PlayerPrefs.GetInt("Reverse") == 1 ? reverse_playerSpawn : playerSpawn;
            CurrentCar = Instantiate(selectedCar, spawn.position, spawn.rotation);
        }
    }

    void OnEnable()
    {
        racerscript = FindAnyObjectByType<RacerScript>();
    }

    public void LoadData(GameData data)
    {
        if (data != null)
        {
            return;
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data != null)
        {
            data.scored += this.score;
        }       
    }

    //temp ja ota se pois sit
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (racerscript.winMenu.activeSelf) return;
            racerscript.EndRace();
        }
    }

    public void StopAddingPoints()
    {
        isAddingPoints = false;
    }
}