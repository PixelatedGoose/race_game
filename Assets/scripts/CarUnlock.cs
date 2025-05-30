using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class CarStats
{
    public string carName;
    public int speed;
    public int acceleration;
    public int handling;
}

public class CarUnlock : MonoBehaviour, IDataPersistence
{
    public List<GameObject> carsl;
    public List<CarStats> carStats; 
    private Dictionary<GameObject, int> carPointRequirements;
    public int scoreamount;
    public Button button;
    private Button left, right;
    private HashSet<GameObject> unlockedCars = new HashSet<GameObject>();

    // UI Elements
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI carNameText; // For car name
    public TextMeshProUGUI speedText;   // For speed
    public TextMeshProUGUI accelerationText; // For acceleration
    public TextMeshProUGUI handlingText; // For handling
    private int activeCarIndex = 0;
    public TextMeshProUGUI CurrentScoreText;

    void Awake()
    {
        carsl = new List<GameObject>
        {
            GameObject.Find("REALCAR_x"),
            GameObject.Find("REALCAR"),
            GameObject.Find("REALCAR_y"),
            GameObject.Find("Lada")
        };

        left = GameObject.Find("left").GetComponent<Button>();
        left.onClick.AddListener(UnlockCar);
        right = GameObject.Find("right").GetComponent<Button>();
        right.onClick.AddListener(UnlockCar);
    }

    public void LoadData(GameData data)
    {
        if (data != null)
        {
            scoreamount = data.scored; 
        }
    }

    public void SaveData(ref GameData data)
    {
        return;        
    }

    void Start()
    {
        if (GameManager.instance == null)
        {
            return;
        }

        if (carsl.Count == 4 && carsl[0] != null && carsl[1] != null && carsl[2] != null && carsl[3] != null)
        {
            carPointRequirements = new Dictionary<GameObject, int>
            {
                { carsl[0], 0 },
                { carsl[1], 2 }, //98734
                { carsl[2], 2000 },
                { carsl[3], 10000000 }
            };
        }

        
        UpdateCarStats();
    }

    void Update()
    {
        CurrentScoreText.text = "Your Score: " + scoreamount.ToString();
    }

    public void UnlockCar()
    {
        foreach (var car in carsl)
        {
            if (car.activeInHierarchy)
            {
                if (!unlockedCars.Contains(car) && scoreamount >= carPointRequirements[car])
                {
                    button.interactable = true;
                    unlockedCars.Add(car);
                    car.SetActive(true);
                }
                else if (scoreamount < carPointRequirements[car])
                {
                    button.interactable = false;
                }

                if (unlockedCars.Contains(car))
                {
                    button.interactable = true;
                }
            }
        }

        for (int i = 0; i < carsl.Count; i++)
        {
            if (carsl[i].activeInHierarchy)
            {
                activeCarIndex = i; 
                break;
            }
        }
        UpdateCarStats();
    }

    public void SetActiveCarIndex(int index)
    {
        if (index >= 0 && index < carsl.Count)
        {
            activeCarIndex = index;
            UpdateCarStats(); 
        }
    }

    private void UpdateCarStats()
    {
        if (activeCarIndex >= 0 && activeCarIndex < carsl.Count)
        {
            GameObject activeCar = carsl[activeCarIndex];
            CarStats stats = carStats[activeCarIndex];

            // Update UI Text
            carNameText.text = $"Car Name: {stats.carName}";
            speedText.text = $"Speed: {stats.speed}";
            accelerationText.text = $"Acceleration: {stats.acceleration}";
            handlingText.text = $"Handling: {stats.handling}";

            if (!unlockedCars.Contains(activeCar))
            {
                int requiredScore = carPointRequirements[activeCar];
                scoreText.text = $"Score needed to unlock: {requiredScore}";
            }
            else
            {
                scoreText.text = "Car already unlocked!";
            }
        }
        else
        {
            scoreText.text = "Invalid car selection!";
        }
    }
}