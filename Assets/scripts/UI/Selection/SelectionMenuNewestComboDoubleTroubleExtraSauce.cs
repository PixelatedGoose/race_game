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

public class SelectionMenuNewestComboDoubleTroubleExtraSauce : MonoBehaviour
{
    public GameObject[] msObjectsList;
    private float schizophrenia;
    private GameObject loadObjects;
    private AudioSource loadingLoop;
    private Text scoreText;
    public Toggle AItoggle;

    public enum Gamemode {Single, AI, Multi};
    public Gamemode selectedGamemode;

    private TextAsset selectionDetails;
    private Dictionary<string, Dictionary<string, string>> details;
    [SerializeField] private TextMeshProUGUI detailsPanelText;

    [SerializeField] private int selectionIndex = 0;
    private GameObject[] selectionMenus;

    public GameObject[] cars;
    public Button left, right, select, back;

    public CarStats[] carStats; 
    public int scoreAmount;
    public Text carNameText,
    speedText, accelerationText, handlingText;

    private int activeCarIndex = 0;
    private int index;

    RaceResultHandler handler;
    RaceResultCollection collection;
    protected mapSelection mapSelection;

    private AudioSource menuMusic;



    void Awake()
    {
        selectionDetails = Resources.Load<TextAsset>("selectionDetails");
        //i'm dictionarying my dictionary
        details = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(selectionDetails.text);
        handler = new RaceResultHandler(Application.persistentDataPath, "race_result.json");
        collection = handler.Load();
        
        // mapSelection = GameObject.Find("MapSelection").GetComponent<mapSelection>();
        cars = GameObject.FindGameObjectsWithTag("thisisacar");

        selectionMenus = GameObject.FindGameObjectsWithTag("selectionMenu")
        .OrderBy(go => go.name).ToArray();
        selectionMenus[1].SetActive(false);
        selectionMenus[2].SetActive(false);

        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();
        loadingLoop = GameObject.Find("loadingLoop").GetComponent<AudioSource>();
    }

    void Start()
    {
        foreach (GameObject car in cars)
        {
            car.SetActive(false);
        }
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
        menuMusic.Play();
    }

    //selection alkaa kolmella valinnalla: singleplayer, ai botit, multiplayer
    //ai botit on sama ku singleplayer; ekana ai asetusten valinta
    //tätä ekaa valintaa käytetään määrittämään arrayn koko
    //(paitsi multiplayer koska se on eri scene)

    //ai options > map > car > options > gaming singleplayeris
    //uus scene: lobby > map > car > options > gaming multiplayeris
    public void SelectGamemode(Gamemode mode)
    {
        selectedGamemode = mode;
    }

    public void UpdateCarStats()
    {
        activeCarIndex = -1;
        foreach (GameObject car in cars)
        {
            if (car.activeInHierarchy)
            {
                activeCarIndex = Array.IndexOf(cars, car);
                break;
            }
        }

        if (activeCarIndex >= 0 && activeCarIndex < cars.Length)
        {
            CarStats activeCarStats = carStats[activeCarIndex];

            // Update UI Text
            carNameText.text = $"{activeCarStats.carName}";
            speedText.text = $"Speed: {activeCarStats.speed}";
            accelerationText.text = $"Acceleration: {activeCarStats.acceleration}";
            handlingText.text = $"Handling: {activeCarStats.handling}";
        }
    }

    //todo: muuta score timeksi ja ota se per base map
    public void UpdateResultsPerMap()
    {
        /* CarStats activeCarStats = carStats[activeCarIndex];

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

    private IEnumerator BeginLoading()
    {
        loadingLoop.Play();

        schizophrenia = UnityEngine.Random.Range(3.5f, 6.5f);
        LeanTween.moveLocalY(loadObjects.gameObject, -0.5f, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        foreach (GameObject theobject in msObjectsList)
        {
            LeanTween.moveLocalY(theobject, theobject.transform.position.y + 451, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        }

        Debug.Log("you will now wait for: " + schizophrenia + " seconds");
        yield return new WaitForSeconds(schizophrenia);
        
        SceneManager.LoadSceneAsync(PlayerPrefs.GetString("SelectedMap"));
    }

    private void Update()
    {
        //tän kaiken pitää executtaa AINOASTAAN kun se vaihtuu jos se on helppoa
        GameObject current = EventSystem.current.currentSelectedGameObject;
        //Debug.LogWarning(current);

        if (current != null)
        {
            //vuoden indeksoinnit siitä
            if (details[selectionMenus[selectionIndex].name].ContainsKey(current.name))
                detailsPanelText.text = details[selectionMenus[selectionIndex].name][current.name];
            //säilytä edellinen teksti details ruudus jos dropdown on valittuna
            else if (current.name.StartsWith("Item"))
                return;
            else
                detailsPanelText.text = "";
        }
    }

    /* 1. map, 2. car, 3. settings, 4. loading :))))
    vaihtaa sit indeksien mukaan tota
    HUOM. ylemmän buttonin teksti vois vaihtaa "go!" kun on viimeses osas */
    public void Next()
    {
        selectionIndex++;
        selectionMenus[selectionIndex].SetActive(true);
        selectionMenus[selectionIndex - 1].SetActive(false);
    }
    public void Back()
    {
        //normal back
        if (selectionIndex != 0)
        {
            selectionIndex--;
            selectionMenus[selectionIndex].SetActive(true);
            selectionMenus[selectionIndex + 1].SetActive(false);
            //jos index == 0 mentyään takasin
            if (selectionIndex == 0)
            {
                GameObject shorelineButton = GameObject.Find("Shoreline");
                shorelineButton.GetComponent<Button>().Select();

                GameObject nextButton = GameObject.Find("Next");
                GameObject backButton = GameObject.Find("Back");
                nextButton.SetActive(false);
                backButton.SetActive(false);
            }
        }
        else
        {
            SceneManager.LoadSceneAsync("MainMenu");
        }
    }

    //night mappeja ei lasketa tähän!!!
    private string GetFullMapName(string shortName)
    {
        string selectedMap;
        if (AItoggle.isOn)
            selectedMap = $"ai_{shortName}";
        else
            selectedMap = shortName;
        return selectedMap;
    }

    /// <summary>
    /// WIP
    /// </summary>
    /// <param name="selecta">WIP</param>
    private void OnMapSelected(string selecta)
    {
        string selectedMap = GetFullMapName(selecta);

        PlayerPrefs.SetString("SelectedMap", selectedMap);
        PlayerPrefs.Save();
        Debug.Log($"onnittelut, voitat lomamatkan kohteeseen: {selectedMap}");

        LeanTween.value(scoreText.gameObject, scoreText.rectTransform.anchoredPosition.x, -20.0f, 2f)
            .setEase(LeanTweenType.easeOutExpo)
            .setOnUpdate((float val) =>
            {
            scoreText.rectTransform.anchoredPosition = new Vector2(val, scoreText.rectTransform.anchoredPosition.y);
            });
    }
}
