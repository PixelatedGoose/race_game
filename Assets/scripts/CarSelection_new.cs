using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    void Awake()
    {
        mapSelection = GameObject.Find("mapselect").GetComponent<mapSelection>();
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
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].activeInHierarchy)
            {
                activeCarIndex = i;
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
        GameObject.Find("cars").SetActive(false);
        csObjects.SetActive(false);
        msObjects.SetActive(true);
    }

    public void Back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}