using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public List<GameObject> carsl;
    public Dictionary<GameObject, int> carPointRequirements;
    public int Neededpoints;
    public Button button;

    void Start()
    {
        print("your scorepoints: " + GameManager.instance.scoreamount);
        carsl = new List<GameObject>
        {
            GameObject.Find("REALCAR_x"),
            GameObject.Find("REALCAR"),
            GameObject.Find("REALCAR_y")
        };

        carPointRequirements = new Dictionary<GameObject, int>
        {
            { carsl[0], 10 }, 
            { carsl[1], 20 }, 
            { carsl[2], 30000 }  
        };
    }

    public void unlockcar()
    {
        Neededpoints = Mathf.FloorToInt(GameManager.instance.scoreamount);
        {
            foreach (var car in carsl)
            {
                if (Neededpoints >= carPointRequirements[car])
                {
                    car.SetActive(true);
                }
                else
                {
                    car.SetActive(false);
                }
            }
            button.interactable = false;
        }
    }
}