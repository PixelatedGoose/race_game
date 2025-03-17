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
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager.instance is null");
        }
        else
        {
            Debug.Log("GameManager.instance is not null");
            Debug.Log("Chosen Map: " + GameManager.instance.chosenMap);
        }

        cars = GameObject.FindGameObjectsWithTag("thisisacar");
    }

    void Start()
    {
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
            Debug.Log("Saved CarIndex: " + index);
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
            Debug.Log("Saved CarIndex: " + index);
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
            Debug.Log("Loading scene: " + GameManager.instance.chosenMap);
            SceneManager.LoadSceneAsync(GameManager.instance.chosenMap);
        }
    }
}






