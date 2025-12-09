using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

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

    protected mapSelection mapSelection;

    private AudioSource menuMusic;
    private GameObject carGameObjects;

    void Awake()
    {
        mapSelection = GameObject.Find("MapSelection").GetComponent<mapSelection>();
        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();
        carGameObjects = GameObject.Find("cars");

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
        mapSelection.maps = new GameObject[]
        {
            GameObject.Find("haukipudasDay"),
            GameObject.Find("haukipudasNight")
        };
        mapSelection.MapFallAnimResetPos(); //ashfhjdskfkdshjsdfkjsdhjkfsdh

        scoreText.text = $"Score with this car: {scoreAmount}";
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
        //pitää sallia valitteminen vasta tässä, että ei tuu erroreit
        select.Select();

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
        if (activeCarIndex < 0 || activeCarIndex >= cars.Length || cars[activeCarIndex] == null)
        {
            Debug.LogWarning($"ActivateMapSelection aborted: invalid activeCarIndex={activeCarIndex}");
            return;
        }

        GameObject car = cars[activeCarIndex];

        // Option A: detach the car so disabling the UI container won't stop the tween
        car.transform.SetParent(null, true);

        // start rotation
        LeanTween.rotateX(car, 360f, 4f).setLoopClamp();

        // switch UI after a short delay so tween can start (or after rotation finishes)
        StartCoroutine(SwitchToMapSelectionDelayed(0.1f));
    }

    private System.Collections.IEnumerator SwitchToMapSelectionDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        csObjects.SetActive(false);
        msObjects.SetActive(true);
    }

    public void ResetCarTweens()
    {
        LeanTween.cancel(cars[activeCarIndex]);
        LeanTween.rotateLocal(cars[activeCarIndex], new Vector3(0f, 0f, 0f), 0.0001f);
    }

    public void Back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}