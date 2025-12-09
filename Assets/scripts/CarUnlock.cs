using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CarUnlock : MonoBehaviour, IDataPersistence
{
    public List<GameObject> carsl;
    public List<CarStats> carStats; 
    private Dictionary<GameObject, int> carPointRequirements;
    public int scoreamount;
    public Button button;
    private Button left, right;
    private readonly HashSet<GameObject> unlockedCars = new();

    protected struct CarDataTexts
    {
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI carNameText; // For car name
        public TextMeshProUGUI speedText;   // For speed
        public TextMeshProUGUI accelerationText; // For acceleration
        public TextMeshProUGUI handlingText; // For handling
        public TextMeshProUGUI CurrentScoreText;
    }
    protected CarDataTexts carDataTexts;
    private int activeCarIndex = 0;
    private int requiredScore;

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
    void Awake()
    {
        carsl = new List<GameObject>
        {
            GameObject.Find("REALCAR_x"),
            GameObject.Find("REALCAR"),
            GameObject.Find("REALCAR_y"),
            GameObject.Find("Lada")
        };

        left = GameObject.Find("L_ChangeCar").GetComponent<Button>();
        left.onClick.AddListener(UnlockCar);
        right = GameObject.Find("R_ChangeCar").GetComponent<Button>();
        right.onClick.AddListener(UnlockCar);

    }

    void Start()
    {
        scoreamount = 100000;
        if (GameManager.instance == null)
        {
            Debug.Log("GameManager instance is null.");
            return;
        }

        if (carsl.Count == 4 && carsl[0] != null && carsl[1] != null && carsl[2] != null && carsl[3] != null)
        {
            carPointRequirements = new Dictionary<GameObject, int>
            {
                { carsl[0], 0 },
                { carsl[1], 2 },
                { carsl[2], 10000 },
                { carsl[3], 10000000 }
            };
        }

        activeCarIndex = carsl.FindIndex(car => car.activeInHierarchy);

        if (activeCarIndex >= 0 && activeCarIndex < carsl.Count)
        {
            GameObject activeCar = carsl[activeCarIndex];

            if (activeCar.name == "Lada")
            {
                carDataTexts.scoreText.text = $"Score needed to unlock: {requiredScore}";
                button.interactable = false;
            }
        }
        else
        {
            Debug.LogWarning("No active car found!");
        }
        if (scoreamount > 90000)
        {
            carDataTexts.scoreText.text = "Car alreary Unlocked!";
        }
    }

    void Update()
    {
        carDataTexts.CurrentScoreText.text = "Your Score: " + scoreamount.ToString();
    }
    
    public void UnlockCar()
    {
        button.interactable = true;

        foreach (var car in carsl)
        {
            if (car.activeInHierarchy)
            {
                if (car.name == "Lada")
                {
                    button.interactable = false;
                    continue;
                }
                if (!unlockedCars.Contains(car) && scoreamount >= carPointRequirements[car])
                {
                    unlockedCars.Add(car);
                    car.SetActive(true);
                    button.interactable = true;
                }
            }
        }



        activeCarIndex = -1;
        for (int i = 0; i < carsl.Count; i++)
        {
            if (carsl[i].activeInHierarchy)
            {
                activeCarIndex = i;
                break;
            }
        }

        if (activeCarIndex == -1)
        {
            Debug.LogWarning("No active car found!");
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
            carDataTexts.carNameText.text = $"Car Name: {stats.carName}";
            carDataTexts.speedText.text = $"Speed: {stats.speed}";
            carDataTexts.accelerationText.text = $"Acceleration: {stats.acceleration}";
            carDataTexts.handlingText.text = $"Handling: {stats.handling}";

            if (unlockedCars.Contains(activeCar))
            {
                carDataTexts.scoreText.text = "Car already unlocked!";
            }
            else
            {
                requiredScore = carPointRequirements[activeCar];
                carDataTexts.scoreText.text = $"Score needed to unlock: {requiredScore}";
            }
        }
        else
        {
            carDataTexts.scoreText.text = "Invalid car selection!";
        }
    }
}