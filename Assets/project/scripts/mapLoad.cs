using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mapLoad : MonoBehaviour
{
    //fuck this shit im out
    
    public GameObject[] cars;

    void Awake()
    {
        //etsi autot järjestyksessä (pitäs olla aika ilmiselvää)
        cars = new GameObject[] 
        {
            GameObject.Find("REALCAR_x"), 
            GameObject.Find("REALCAR"), 
            GameObject.Find("REALCAR_y") 
        };

        foreach (GameObject car in cars)
        {
            car.SetActive(false);
        }

        /* if (GameManager.instance.sceneSelected == "test_mountain")
        { */
            Debug.Log("in HELL");

            if (GameManager.instance.carIndex >= 0 && GameManager.instance.carIndex <= cars.Length)
            {
                cars[GameManager.instance.carIndex].SetActive(true);

                /* Debug.Log("FINAL:" + PlayerPrefs.GetInt("CarIndex")); //debug
                Debug.Log("FINAL CURRENTCAR IS:" + GameManager.instance.currentCar); //debug */

                Debug.Log("Loaded CarIndex: " + GameManager.instance.carIndex);
            }
            else
            {
                Debug.LogError("Car index out of range (mapLoad): " + GameManager.instance.carIndex);
            }
        /* } */
    }
}
