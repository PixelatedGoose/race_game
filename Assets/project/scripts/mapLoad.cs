using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mapLoad : MonoBehaviour
{
    //fuck this shit im out

    //mapLoad, CarController, GameManager, CarSelection ON KESKEN, ÄLÄ KOSKE
    
    public GameObject[] cars;
    private int index;

    private void Awake()
    {
        bool inScene = SceneManager.GetActiveScene().name == "test_mountain";
        int carSelected = PlayerPrefs.GetInt("CarIndex"); 

        cars = GameObject.FindGameObjectsWithTag("thisisacar"); //etsi autot tagin perusteella (pitäs olla aika ilmiselvää)

        foreach (GameObject car in cars)
        {
            car.SetActive(false);
        }

        if (inScene)
        {
            Debug.Log("in HELL");
            
            Debug.Log(index);
            if (index >= 0 && index < cars.Length)
            {
                cars[index].SetActive(true);
            }

            /* switch(carSelected) 
            {
                case 1:
                    Debug.Log("1. I already have an activation code.");
                    break;
                case 2:
                    Debug.Log("mdfasiodoi OLET AUTOMEKAANIKKO NUMERO KAKSI");
                    break;
                case 3:
                    Debug.Log("tämä on kolomas");
                    break;
                default:
                    Debug.Log("De Fault - fuck off copilot, Leo is gay");
                    break;
            } */
        }
    }
}
