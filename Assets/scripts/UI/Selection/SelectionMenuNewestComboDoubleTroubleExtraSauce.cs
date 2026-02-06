using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;

[Serializable]
public class CarStats
{
    public string carName;
    public int speed;
    public int acceleration;
    public int handling;
    public float scoreMult;
    public int turbeBoost;
    public int turbeAmount;
}
public class SelectionMenuNewestComboDoubleTroubleExtraSauce : MonoBehaviour
{
    CarInputActions Controls;

    private float schizophrenia;
    private AudioSource loadingLoop;
    private Text scoreText;

    public enum Gamemode {Single, AI, Multi};
    [SerializeField] private Gamemode selectedGamemode = Gamemode.Single; //serializefield on debug

    //tallennettavia juttui, joita käytetää myöhemmin esim. PlayerPrefsien kautta
    [SerializeField] private string savedMapBaseName; //serializefield on debug
    
    private TextAsset selectionDetails;
    private Dictionary<string, Dictionary<string, string>> details;
    [SerializeField] private TextMeshProUGUI detailsPanelText;
    private GameObject carStatsContainer;

    [SerializeField] private int selectionIndex = 0; //serializefield on debug
    private GameObject[] selectionMenus;

    [SerializeField] private GameObject[] cars; //serializefield on debug
    public CarStats[] carStats;
    public int scoreAmount;
    public Text carNameText,
    speedText, accelerationText, handlingText,
    scoreMultText, turbeBoostText, turbeAmountText;
    private int activeCarIndex = 0;
    [SerializeField] private int index; //serializefield on debug
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject backButton;

    RaceResultHandler handler;
    RaceResultCollection collection;

    private AudioSource menuMusic;



    void Awake()
    {
        Controls = new CarInputActions();
        
        selectionDetails = Resources.Load<TextAsset>("selectionDetails");
        //i'm dictionarying my dictionary
        details = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(selectionDetails.text);
        handler = new RaceResultHandler(Application.persistentDataPath, "race_result.json");
        collection = handler.Load();
        cars = GameObject.FindGameObjectsWithTag("thisisacar")
        .OrderBy(c => c.name).ToArray();
        
        carStatsContainer = GameObject.Find("carStatsContainer");
        carStatsContainer.SetActive(false);
        selectionMenus = GameObject.FindGameObjectsWithTag("selectionMenu")
        .OrderBy(go => go.name).ToArray();
        foreach (var menu in selectionMenus.Skip(1))
        {
            menu.SetActive(false);
        }

        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();
        loadingLoop = GameObject.Find("loadingLoop").GetComponent<AudioSource>();
    }
    void OnEnable()
    {
        Controls.Enable();
        Controls.CarControls.carskinright.performed += ctx => RightButton();
        Controls.CarControls.carskinleft.performed += ctx => LeftButton();
        Controls.CarControls.menucancel.performed += ctx => Back();
    }
    void OnDisable()
    {
        Controls.Disable();
        Controls.CarControls.carskinright.performed -= ctx => RightButton();
        Controls.CarControls.carskinleft.performed -= ctx => LeftButton();
        Controls.CarControls.menucancel.performed -= ctx => Back();
    }

    void Start()
    {
        foreach (GameObject car in cars)
        {
            car.SetActive(false);
        }
        menuMusic.Play();
    }

    //selection alkaa kolmella valinnalla: singleplayer, ai botit, multiplayer
    //ai botit on sama ku singleplayer; ekana ai asetusten valinta
    //tätä ekaa valintaa käytetään määrittämään arrayn koko
    //(paitsi multiplayer koska se on eri scene)

    //ai options > map > car > options > gaming singleplayeris
    //uus scene: lobby > map > car > options > gaming multiplayeris

    //tätä käytetää vain alussa
    public void SelectGamemode(int mode)
    {
        selectedGamemode = (Gamemode)mode;
    }

    public void UpdateCarStats()
    {
        activeCarIndex = -1;
        //laita activeCarIndex kuntoon
        foreach (GameObject car in cars)
        {
            if (car.activeInHierarchy)
            {
                activeCarIndex = Array.IndexOf(cars, car);
                break;
            }
        }

        //indeksin mukaan auton statsit
        if (activeCarIndex >= 0 && activeCarIndex < cars.Length)
        {
            CarStats activeCarStats = carStats[activeCarIndex];

            carNameText.text = $"{activeCarStats.carName}";
            speedText.text = $"Speed: {activeCarStats.speed}";
            accelerationText.text = $"Acceleration: {activeCarStats.acceleration}";
            handlingText.text = $"Handling: {activeCarStats.handling}";
            scoreMultText.text = $"Score mult.: {activeCarStats.scoreMult}x";
            turbeBoostText.text = $"Turbo boost: {activeCarStats.turbeBoost}";
            turbeAmountText.text = $"Turbo amount: {activeCarStats.turbeAmount}";
        }
    }

    //todo: muuta score timeksi ja ota se per base map
    public void UpdateResultsPerMap()
    {
        /* CarStatsNew activeCarStats = carStats[activeCarIndex];

        string selectedMap = PlayerPrefs.GetString("SelectedMap");

        var bestResults = Array.Empty<RaceResultData>();
        if (collection != null && collection.results.Count != 0)
        {
            bestResults = collection.results
                .Where(r => string.Equals(r.map, selectedMap, StringComparison.OrdinalIgnoreCase))
                .Where(r => string.Equals(r.carName, activeCarStats.carName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.score)
                .ToArray();
        }
        else
        {
            Debug.Log("no race results exist; defaulting to empty");
            bestResults = Array.Empty<RaceResultData>();
        }

        int topResultsScore = 0;
        if (bestResults.Length != 0)
        {
            topResultsScore = bestResults[0].score;
            scoreText.text = $"Best score with {activeCarStats.carName}: {topResultsScore}";
        }
        else
            scoreText.text = $"No score yet with {activeCarStats.carName}"; */
    }
    
    public void RightButton()
    {
        if (selectionMenus[selectionIndex].name != "B_carSelection") return;

        cars[index].SetActive(false);
        index = (index + 1) % cars.Length;
        cars[index].SetActive(true);
        if (index >= 0 && index < cars.Length)
        {
            activeCarIndex = index;
            UpdateCarStats(); 
        }

        PlayerPrefs.SetInt("CarIndex", index);
        PlayerPrefs.Save();
    }

    public void LeftButton()
    {
        if (selectionMenus[selectionIndex].name != "B_carSelection") return;
        
        cars[index].SetActive(false);
        index = (index - 1 + cars.Length) % cars.Length;
        cars[index].SetActive(true);
        if (index >= 0 && index < cars.Length)
        {
            activeCarIndex = index;
            UpdateCarStats(); 
        }

        PlayerPrefs.SetInt("CarIndex", index);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;
        //Debug.LogWarning(current);

        if (current != null)
        {
            //vuoden indeksoinnit siitä
            if (details[selectionMenus[selectionIndex].name].ContainsKey(current.name))
                detailsPanelText.text = details[selectionMenus[selectionIndex].name][current.name];
            else if (details[selectionMenus[selectionIndex].name].ContainsKey(cars[activeCarIndex].name))
                detailsPanelText.text
                = details[selectionMenus[selectionIndex].name][cars[activeCarIndex].name];
            //säilytä edellinen teksti details ruudus jos dropdown on valittuna
            else if (current.name.StartsWith("Item"))
                return;
            else
                detailsPanelText.text = "";
        }
    }

    public void Next()
    {
        selectionIndex++;
        selectionMenus[selectionIndex].SetActive(true);
        selectionMenus[selectionIndex - 1].SetActive(false);

        carStatsContainer.SetActive(false);

        if (selectionMenus[selectionIndex].name == "B_carSelection")
        {
            carStatsContainer.SetActive(true);
            if (index >= 0 && index < cars.Length)
            {
                cars[index].SetActive(true);
            }
            else
            {
                Debug.LogError("Car index out of range: " + index);
                index = 0;
                cars[index].SetActive(true);
            }

            UpdateCarStats();
        }
    }
    public void Back()
    {
        //normal back
        if (selectionIndex != 0)
        {
            selectionIndex--;
            selectionMenus[selectionIndex].SetActive(true);
            selectionMenus[selectionIndex + 1].SetActive(false);
            
            carStatsContainer.SetActive(false);

            if (selectionMenus[selectionIndex].name == "A_mapSelection")
            {
                //TODO: korjata tämä koska tää systeemi on silti PASKAA TÄYNNÄ
                //tän sijasta ettii gameobjectin jolla on tietty tag
                //valittee sen tämän sijasta; helpottaa paljon
                //myös vois tehä jonku listan josta tarkistaa, mille laittaa päälle buttonit ja ei
                //koska vihaan miljoonaa if lausetta
                GameObject shorelineButton = GameObject.Find("Shoreline");
                shorelineButton.GetComponent<Button>().Select();

                foreach (GameObject car in cars)
                    car.SetActive(false);
            }
            else if (selectionMenus[selectionIndex].name == "B_carSelection")
            {
                GameObject base1Button = GameObject.Find("Base1");
                base1Button.GetComponent<Button>().Select();

                carStatsContainer.SetActive(true);
            }
            if (selectionMenus[selectionIndex].name != "C_optionSelection")
            {
                nextButton.SetActive(false);
                backButton.SetActive(false);
            }
        }
        else
        {
            SceneManager.LoadSceneAsync("MainMenu");
        }
    }

    private void SetMapToLoad()
    {
        string selectedMap = savedMapBaseName;
        TMP_Dropdown dayOrNight = GameObject.Find("Time").GetComponent<TMP_Dropdown>();

        if (selectedGamemode == Gamemode.AI)
            selectedMap = $"ai_{savedMapBaseName}";
        if (dayOrNight.value == 1)
            selectedMap += $"_night";
        PlayerPrefs.SetString("SelectedMap", selectedMap);
        PlayerPrefs.Save();

        Debug.Log($"onnittelut, voitat lomamatkan kohteeseen: {selectedMap}");
    }

    public void SaveBaseMapName(string selecta)
    {
        savedMapBaseName = selecta;
    }

    //tarkistan myöhemmin voiko tätä välttää... vitun coroutinet
    public void StartGame()
    {
        SetMapToLoad();
        StartCoroutine(LoadSelectedMap());
    }
    private IEnumerator LoadSelectedMap()
    {
        loadingLoop.Play();

        schizophrenia = UnityEngine.Random.Range(3.5f, 6.5f);
        //tweenaus myöhemmi
        Debug.Log("you will now wait for: " + schizophrenia + " seconds");
        yield return new WaitForSeconds(schizophrenia);
        
        SceneManager.LoadSceneAsync(PlayerPrefs.GetString("SelectedMap"));
    }
}