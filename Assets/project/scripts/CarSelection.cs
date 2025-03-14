using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CarSelection : MonoBehaviour
{
    public GameObject[] cars;
    public Button left;
    public Button right;

    private int index;

    void Awake()
    {
        cars = GameObject.FindGameObjectsWithTag("thisisacar");
        //ei tarvi enää manuaalisesti lisätä
    }

    void Start()
    {
        index = PlayerPrefs.GetInt("CarIndex", 0); 

        foreach (GameObject car in cars)
        {
            car.SetActive(false);
        }

        // Activate the selected car
        if (index >= 0 && index < cars.Length)
        {
            cars[index].SetActive(true);
        }
    }

    void Update()
    {

        right.interactable = index < cars.Length - 1;
        left.interactable = index > 0;
    }

    public void RightButton()
    {
        if (index < cars.Length - 1)
        {
            cars[index].SetActive(false);
            index++;
            cars[index].SetActive(true); 
            PlayerPrefs.SetInt("CarIndex", index);
            PlayerPrefs.Save();
        }
    }

    public void LeftButton()
    {
        if (index > 0)
        {
            cars[index].SetActive(false);
            index--;
            cars[index].SetActive(true);

            PlayerPrefs.SetInt("CarIndex", index);
            PlayerPrefs.Save();
        }
    }

    public void PlayGame()
    {

        SceneManager.LoadSceneAsync(2); //load scene
    }
}