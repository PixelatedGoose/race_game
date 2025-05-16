using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CarSelection : MonoBehaviour
{
    public GameObject[] cars;
    public Button left;
    public Button right;

    private int index;

    public GameObject csObjects;
    public GameObject msObjects;

    private AudioSource menuMusic;

    void Awake()
    {
        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();

        cars = new GameObject[] 
        { 
            GameObject.Find("REALCAR_x"),
            GameObject.Find("REALCAR"),
            GameObject.Find("REALCAR_y"), 
            GameObject.Find("Lada")
        };
    }

    void Start()
    {
        msObjects.SetActive(false);
        
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
        if (index >= 0 && index < cars.Length)
        {
            cars[index].SetActive(true);
        }
        else
        {
            Debug.LogError("Car index out of range: " + index);
            index = 0;
            cars[index].SetActive(true);
        }

        menuMusic.Play();
    }

    void Update()
    {
        // Removed button interactable logic for looping
    }

    public void RightButton()
    {
        cars[index].SetActive(false);
        index = (index + 1) % cars.Length;
        cars[index].SetActive(true);
        PlayerPrefs.SetInt("CarIndex", index);
        PlayerPrefs.Save();
    }

    public void LeftButton()
    {
        cars[index].SetActive(false);
        index = (index - 1 + cars.Length) % cars.Length;
        cars[index].SetActive(true);
        PlayerPrefs.SetInt("CarIndex", index);
        PlayerPrefs.Save();
    }

    public void ActivateMapSelection()
    {
        csObjects.SetActive(false);
        msObjects.SetActive(true);
    }

    public void back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}