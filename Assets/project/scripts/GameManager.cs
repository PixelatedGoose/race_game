using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

//HUOM. ÄLÄ POISTA KÄYTÖSTÄ MUITA AUTOJA HIERARKIASSA

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("score systeemi")]
    private int score;
    private int savedScore;

    public float scoreAddWT = 0.01f; //WT = wait time

    private bool isAddingPoints = false;
    
    public TextMeshProUGUI Score;
    public float scoreamount = 0;

    [Header("menut")]
    public bool isPaused = false;

    public int chosenMap = 1;

    public GameObject mapChangeButton;

    [Header("car selection")]
    public GameObject currentCar;
    public GameObject[] cars;
    public int carIndex;

    [Header("scene asetukset")]
    public string sceneSelected;

    void Awake()
    {
        if (instance == null)
        {
            Debug.Log("Pasia, olet tehnyt sen!");
            instance = this;
            // DontDestroyOnLoad(gameObject); //poistin koska "DontDestroyOnLoad only works for root GameObjects or components on root GameObjects."
        }

        //etsi autot järjestyksessä (pitäs olla aika ilmiselvää)
        cars = new GameObject[] 
        { 
            GameObject.Find("REALCAR_x"), 
            GameObject.Find("REALCAR"), 
            GameObject.Find("REALCAR_y")
        };

        carIndex = PlayerPrefs.GetInt("CarIndex");
        currentCar = carIndex >= 0 && carIndex < cars.Length ? cars[carIndex] : cars[0];
        chosenMap = PlayerPrefs.GetInt("chosenMap");

        sceneSelected = SceneManager.GetActiveScene().name;

        if (sceneSelected == "test_mountain" || sceneSelected == "test_mountain_night")
        {
            foreach (GameObject car in cars)
            {
                car.SetActive(false);
            }
        }
    }
    
    void Start()
    {   
        if (sceneSelected == "test_mountain" || sceneSelected == "test_mountain_night")
        {
            Debug.Log("in HELL");

            if (carIndex >= 0 && carIndex <= cars.Length)
            {
                cars[carIndex].SetActive(true);
                Debug.Log("Loaded CarIndex: " + carIndex);
            }
            else
            {
                Debug.LogError("Car index out of range: " + carIndex);
            }

            foreach (GameObject car in cars)
            {
                if (car.activeInHierarchy)
                {
                    Debug.Log("onnittelut, voitit paketin hiivaa!: " + car.name);
                    //toi määrittelee mikä on se oikea auto
                    Score = GameObject.Find("Score").GetComponent<TextMeshProUGUI>();
                    //so nanoka?
                }
                else
                {
                    Debug.Log("Thy end is now! Die! Crush! Prepare thyself! Judgement!");
                    Destroy(car);
                    //tää TAPPAA kaikki ne muut että se ei vittuile se unity lol
                }
            }
        }
        savedScore = PlayerPrefs.GetInt("Score", 0);
        Debug.Log("retrieved score" + score);
    }



    public void AddPoints()
    {
        Debug.Log("AddPoints executing");
        if (!isAddingPoints && currentCar.activeSelf && instance != null)
        {
            Debug.Log("adding points...");
            StartCoroutine(IncrementScoreWithDelay());
        }
    }

    public void OnScoreChanged()
    {
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.Save();
        Debug.Log("Score saved: " + score);
    }



    private IEnumerator IncrementScoreWithDelay()
    {
        isAddingPoints = true;   

        while (isAddingPoints)   
        {
            yield return new WaitForSeconds(scoreAddWT);   
            score += 1;   
            Score.text = "Score: " + score.ToString();
            OnScoreChanged();               
        }
    }

    public void StopAddingPoints()
    {
        isAddingPoints = false;
    }

    public void UpdateScoreAmount()
    {
        scoreamount = score;
    }
}