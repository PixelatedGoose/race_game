using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CarUnlock : MonoBehaviour
{
    public List<GameObject> carsl;
    public Dictionary<GameObject, int> carPointRequirements;
    public int Neededpoints;
    public Button button;

    void Start()
    {
        // Initialize cars list
        carsl = new List<GameObject>();
        
        // Find cars and validate they exist
        GameObject car1 = GameObject.Find("REALCAR_x");
        GameObject car2 = GameObject.Find("REALCAR");
        GameObject car3 = GameObject.Find("REALCAR_y");

        if (car1 != null && car2 != null && car3 != null)
        {
            carsl.Add(car1);
            carsl.Add(car2);
            carsl.Add(car3);
            
            // Initialize requirements only if cars are found
            carPointRequirements = new Dictionary<GameObject, int>
            {
                { carsl[0], 10 },
                { carsl[1], 20 },
                { carsl[2], 30000 }
            };

            // Initial unlock check
            UnlockCar();
        }
        else
        {
            Debug.LogError("One or more cars not found in the scene!");
        }

        // Disable the button
        if (button != null)
        {
            button.interactable = false;
        }
    }

    public void UnlockCar()
    {
        if (GameManager.instance != null)
        {
            Neededpoints = Mathf.FloorToInt(GameManager.instance.scoreamount);
            bool anyCarUnlocked = false;

            foreach (var car in carsl)
            {
                if (Neededpoints >= carPointRequirements[car])
                {
                    car.SetActive(true);
                    anyCarUnlocked = true;
                }
                else
                {
                    car.SetActive(false);
                }
            }

            // Ensure the button remains disabled
            if (button != null)
            {
                button.interactable = false;
            }
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }
}