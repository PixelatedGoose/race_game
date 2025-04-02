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
            GameObject.Find("REALCAR_y")
        };
    }

    public void LoadData(GameData data)
    {
        if (data != null)
        {
            this.scoreamount = data.scored; 
            Debug.Log($"Loading data: scored = {data.scored}");
        }
        else
        {
            Debug.LogError("GameData is null!");
        }
    }

    public void SaveData(ref GameData data)
    {
        return;        
    }

    void Start()
    {
        if (carsl.Count == 3 && carsl[0] != null && carsl[1] != null && carsl[2] != null)
        {
            carPointRequirements = new Dictionary<GameObject, int>
            {
                { carsl[0], 0 },
                { carsl[1], 98734 },
                { carsl[2], 30000 }
            };
        }
        UpdateScoreRequirementText();
    }

    void Update()
    {
        UnlockCar();
        unlockedcars();
        UpdateScoreRequirementText();
        CurrentScoreText.text = "Your Score: " + scoreamount.ToString();
    }

    public void UnlockCar()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager instance is null!");
            return;
        }
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
            }
        }
    }

    public void unlockedcars()
    {
        foreach (var car in carsl) 
        {
            if (car.activeInHierarchy) 
            {
                if (unlockedCars.Contains(car))
                {
                    button.interactable = true;
                }
            }
        }
    }

    private void UpdateScoreRequirementText()
    {
        for (int i = 0; i < carsl.Count; i++)
        {
            if (carsl[i].activeInHierarchy)
            {
                activeCarIndex = i; 
                break;
            }
        }

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

    public void SetActiveCarIndex(int index)
    {
        if (index >= 0 && index < carsl.Count)
        {
            activeCarIndex = index;
            UpdateScoreRequirementText(); 
        }
        else
        {
            Debug.LogError("Invalid car index!");
        }
    }
}