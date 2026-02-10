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

    private AudioSource loadingLoop;
    private AudioSource menuMusic;

    public enum Gamemode {Single, AI, Multi};
    [SerializeField] private Gamemode selectedGamemode = Gamemode.Single; //serializefield on debug
    private float schizophrenia;

    [Header("player data")]
    private string savedMapBaseName;
    private int savedLapCount;
    
    [Header("general selection data")]
    private TextAsset selectionDetails;
    [SerializeField] private TMP_Dropdown lapCountDropdown; 
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject backButton;
    private Dictionary<string, Dictionary<string, string>> details;
    [SerializeField] private TextMeshProUGUI detailsPanelText;
    private GameObject carStatsContainer;
    [SerializeField] private int selectionIndex = 0; //serializefield on debug
    private List<GameObject> selectionMenus;
    public List<GameObject> availableSelectionMenus; //public on debug

    [Header("car selection")]
    [SerializeField] private GameObject[] cars; //serializefield on debug
    public CarStats[] carStats;
    public int scoreAmount;
    public Text carNameText,
    speedText, accelerationText, handlingText,
    scoreMultText, turbeBoostText, turbeAmountText;
    private int activeCarIndex = 0;
    [SerializeField] private int index; //serializefield on debug

    RaceResultHandler handler;
    RaceResultCollection collection;



    //TODO: 1. setuppaa start button ja korjaa loading screen ✅
    //2. setuppaa auton skinien tarkistus paska (mitä sulla näkyy vaihtuu kun valittet basen)
    //3. data handling playerprefs kautta (esim. tällä hetkellä valittu map)

    //4. setuppaa map selectionin kuva juttu [ehkä]
    //5. ? ehkä jotain jonka unohin

    //6. score tai aika per auto: miten? mihin?
    //7. lisää tweenaukset kaikkeen tarpeelliseen

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
        .OrderBy(go => go.name).ToList();
        availableSelectionMenus = selectionMenus;
        foreach (var menu in availableSelectionMenus.Skip(1))
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

        lapCountDropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(lapCountDropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        Debug.Log(change.value + 1);
        PlayerPrefs.SetInt("LapCount", change.value + 1);
        PlayerPrefs.Save();
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

        //what bullshit. ei enää nii paskanen kuitenkaa
        if (selectedGamemode != Gamemode.AI)
            availableSelectionMenus = selectionMenus.Where((a, i) => i != 1).ToList();
        else
            availableSelectionMenus = selectionMenus;
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
        if (availableSelectionMenus[selectionIndex].name != "B_carSelection") return;

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
        if (availableSelectionMenus[selectionIndex].name != "B_carSelection") return;
        
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
            //TODO: setuppaa todennäkösesti variable tolle ja sen onchanged paskiainen tänne,
            //jotta voi yksinkertastaa koodia

            //vuoden indeksoinnit siitä
            if (details[availableSelectionMenus[selectionIndex].name].ContainsKey(current.name))
                detailsPanelText.text = details[availableSelectionMenus[selectionIndex].name][current.name];
            else if (details[availableSelectionMenus[selectionIndex].name].ContainsKey(cars[activeCarIndex].name))
                detailsPanelText.text
                = details[availableSelectionMenus[selectionIndex].name][cars[activeCarIndex].name];
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
        availableSelectionMenus[selectionIndex].SetActive(true);
        availableSelectionMenus[selectionIndex - 1].SetActive(false);
        ThePanelThing();

        carStatsContainer.SetActive(false);
        GameObject firstSelected = GameObject.FindGameObjectWithTag("firstSelectable");
        firstSelected.GetComponent<Selectable>().Select();

        if (availableSelectionMenus[selectionIndex].name == "B_carSelection")
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
            availableSelectionMenus[selectionIndex].SetActive(true);
            availableSelectionMenus[selectionIndex + 1].SetActive(false);
            ThePanelThing();
            
            carStatsContainer.SetActive(false);
            GameObject firstSelected = GameObject.FindGameObjectWithTag("firstSelectable");
            firstSelected.GetComponent<Selectable>().Select();

            if (availableSelectionMenus[selectionIndex].name == "A_mapSelection")
            {
                foreach (GameObject car in cars)
                    car.SetActive(false);
            }
            else if (availableSelectionMenus[selectionIndex].name == "B_carSelection")
            {
                carStatsContainer.SetActive(true);
            }
        }
        else
        {
            SceneManager.LoadSceneAsync("MainMenu");
        }
    }

    //helper
    //TODO: päivitä ottamaan huomioon autot (pieni juttu)
    private void ThePanelThing()
    {
        nextButton.SetActive(false);
        backButton.SetActive(false);
        startButton.SetActive(false);
        if (selectionIndex == 0) detailsPanel.SetActive(false);

        if (availableSelectionMenus[selectionIndex].name == "C_optionSelection")
        {
            startButton.SetActive(true);
            backButton.SetActive(true);
        }
        else if (availableSelectionMenus[selectionIndex].name == "1_AIoptionSelection")
        {
            nextButton.SetActive(true);
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