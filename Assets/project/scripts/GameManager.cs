using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



//HUOM. ÄLÄ POISTA KÄYTÖSTÄ MUITA AUTOJA HIERARKIASSA

public class GameManager : MonoBehaviour, IDataPersistence
{
    public static GameManager instance;

    [Header("score systeemi")]
    public int score;

    public float scoreAddWT = 0.01f; //WT = wait time

    public bool isAddingPoints = false;

    // Use an array or list to handle multiple Text components
    public Text[] ScoreTexts; // Array to hold multiple Text components
    public float scoreamount = 0;

    [Header("menut")]
    public bool isPaused = false;

    public int chosenMap = 1;

    [Header("car selection")]
    public GameObject currentCar;
    public GameObject[] cars;
    public int carIndex;

    [Header("scene asetukset")]
    public string sceneSelected;
    [Header("auto")]
    public float carSpeed;

    void OnEnable()
    {
        if (instance == null)
        {
            //Debug.Log("Pasia, olet tehnyt sen!");
            instance = this;
            // DontDestroyOnLoad(gameObject); //poistin koska "DontDestroyOnLoad only works for root GameObjects or components on root GameObjects."
        }
        else
        {
            Destroy(gameObject);
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

        if (sceneSelected == "test_mountain" || sceneSelected == "test_mountain_night" || sceneSelected == "haukipudas")
        {
            foreach (GameObject car in cars)
            {
                //jotta se löytää ne; inaktiivisesta gameobjectista ei pysty löytää mitää
                ScoreTexts[1] = GameObject.Find("Score").GetComponent<Text>();
                car.SetActive(false);
            }

            if (carIndex >= 0 && carIndex <= cars.Length)
            {
                cars[carIndex].SetActive(true);
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
                }
                else
                {
                    Destroy(car);
                    //tää TAPPAA kaikki ne muut että se ei vittuile se unity lol
                }
            }
        }
    }

    public void LoadData(GameData data)
    {
        if (data != null)
        {
            return;
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data != null)
        {
            data.scored += this.score;
        }       
    }



    public void AddPoints()
    {
        RacerScript racerScript = FindAnyObjectByType<RacerScript>();
        if (racerScript != null && racerScript.raceFinished)
        {
            return; 
        }

        if (!isAddingPoints && currentCar.activeSelf && instance != null)
        {
            StartCoroutine(IncrementScoreWithDelay());
        }
    }
    
    private IEnumerator IncrementScoreWithDelay()
    {
        isAddingPoints = true;   

        while (isAddingPoints)   
        {
            yield return new WaitForSeconds(scoreAddWT);   
            score += 1;   

           
            foreach (Text scoreText in ScoreTexts)
            {
                scoreText.text = "Score: " + score.ToString();
            }               
        }
    }

    public void StopAddingPoints()
    {
        isAddingPoints = false;
    }
}