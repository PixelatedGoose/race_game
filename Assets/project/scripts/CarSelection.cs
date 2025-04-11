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
        cars = new GameObject[] 
        { 
            GameObject.Find("REALCAR_x"),
            GameObject.Find("REALCAR"),
            GameObject.Find("REALCAR_y") 
        };
    }

    void Start()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager.instance is null");
        }
        else
        {
            Debug.Log("GameManager.instance is not null");
            Debug.Log("Chosen Map: " + GameManager.instance.chosenMap);
        }
        
        index = PlayerPrefs.GetInt("CarIndex", 0);
        Debug.Log("Loaded CarIndex: " + index);

        foreach (GameObject car in cars)
        {
            car.SetActive(false);
        }

        // Activate the selected car
        if (index >= 0 && index <= cars.Length)
        {
            cars[index].SetActive(true);
        }
        else
        {
            Debug.LogError("Car index out of range: " + index);
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
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager.instance is null! Cannot load scene.");
        }
        else
        {
            Debug.Log("Loading scene: " + PlayerPrefs.GetInt("chosenMap", 1));
            SceneManager.LoadSceneAsync(PlayerPrefs.GetInt("chosenMap", 1));
        }
    }

    public void back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}