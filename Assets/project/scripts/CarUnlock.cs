using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CarUnlock : MonoBehaviour, IDataPersistence
{
    public List<GameObject> carsl;
    private Dictionary<GameObject, int> carPointRequirements;
    public int scoreamount;
    public Button button;
    private HashSet<GameObject> unlockedCars = new HashSet<GameObject>();

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
        else
        {
            Debug.LogError("One or more cars not found in the scene!");
        }
    }

    void Update()
    {
        UnlockCar();
        unlockedcars();
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
}