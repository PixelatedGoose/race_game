using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEngine.EventSystems;

[System.Serializable]
public class CarStats
{
    public string carName;
    public int speed;
    public int acceleration;
    public int handling;
}

public class CarSelection_new : MonoBehaviour
{
    public GameObject[] cars;
    public Button left, right, select, back;

    public CarStats[] carStats; 
    public int scoreAmount;
    public Text scoreText, carNameText,
    speedText, accelerationText, handlingText;

    private int activeCarIndex = 0;
    private int index;

    public GameObject csObjects;
    public GameObject msObjects;

    RaceResultHandler handler;
    RaceResultCollection collection;
    protected mapSelection mapSelection;

    private AudioSource menuMusic;
    private Text selectACarText;

    void Awake()
    {
        selectACarText = GameObject.Find("SelectYoMobile").GetComponent<Text>();
        handler = new RaceResultHandler(Application.persistentDataPath, "race_result.json");
        collection = handler.Load();
        
        mapSelection = GameObject.Find("MapSelection").GetComponent<mapSelection>();
        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();

        cars = new GameObject[]
        {
            GameObject.Find("REALCAR_x"),
            GameObject.Find("REALCAR"),
            GameObject.Find("REALCAR_y"),
            GameObject.Find("Lada")
        };

        left = GameObject.Find("L_changecar").GetComponent<Button>();
        right = GameObject.Find("R_changecar").GetComponent<Button>();
    }

    //lataus ja tallennus
    public void LoadData(GameData data)
    {
        if (data != null)
        {
            scoreAmount = data.scored;
        }
    }
    public void SaveData(ref GameData data)
    {
        return;    
    }

    void Start()
    {
        LeanTween.value(selectACarText.gameObject, selectACarText.color.a, 1f, 1.3f)
            .setOnUpdate(val =>
            {
                var color = selectACarText.color;
                color.a = val;
                selectACarText.color = color;
            });
        LeanTween.value(selectACarText.gameObject, selectACarText.rectTransform.anchoredPosition.x, -380.76f, 2.3f)
            .setOnUpdate((float val) =>
            {
            selectACarText.rectTransform.anchoredPosition = new Vector2(val, selectACarText.rectTransform.anchoredPosition.y);
            })
            .setLoopClamp();

        mapSelection.maps = new GameObject[]
        {
            GameObject.Find("haukipudas"),
            GameObject.Find("haukipudas_night")
        };
        mapSelection.MapFallAnimResetPos(); //ashfhjdskfkdshjsdfkjsdhjkfsdh
        msObjects.SetActive(false);
        csObjects.SetActive(true);
        
        index = PlayerPrefs.GetInt("CarIndex", 0);

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

    public void UpdateCarStats()
    {
        foreach (var car in cars)
        {
            if (car.activeInHierarchy)
            {
                if (car.name == "Lada")
                {
                    select.interactable = false;
                    continue;
                }
                car.SetActive(true);
                select.interactable = true;
            }
        }

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

    public void UpdateScorePerMap()
    {
        CarStats activeCarStats = carStats[activeCarIndex];

        string selectedMapIconName = EventSystem.current.currentSelectedGameObject.name;
        string selectedMap;

        //oletetaan toistaseksi vain kahta eri valintaa
        if (mapSelection.toggle.isOn)
        {
            selectedMap = selectedMapIconName == "haukipudas" ? "ai_haukipudas" : "night_ai_haukipudas";
        }
        else
        {
            selectedMap = selectedMapIconName;
        }
        selectedMap ??= "haukipudas";

        var bestResults = Array.Empty<RaceResultData>();
        if (collection != null && collection.results.Count != 0)
        {
            bestResults = collection.results
                .Where(r => string.Equals(r.map, selectedMap, StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.score)
                .ToArray();
        }
        else
        {
            bestResults = Array.Empty<RaceResultData>();
        }

        int topResultsScore = 0;
        if (bestResults.Length != 0)
        {
            topResultsScore = bestResults[0].score;
            scoreText.text = $"Best score with {activeCarStats.carName}: {topResultsScore}";
        }
        else
            scoreText.text = $"No score yet with {activeCarStats.carName}";
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

    public void ActivateMapSelection()
    {
        csObjects.SetActive(false);
        msObjects.SetActive(true);
    }

    public void Back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}