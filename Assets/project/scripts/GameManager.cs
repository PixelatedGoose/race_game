using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//HUOM. ÄLÄ POISTA KÄYTÖSTÄ MUITA AUTOJA HIERARKIASSA

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("score systeemi")]
    private int score;

    public float scoreAddWT = 0.01f; //WT = wait time

    private bool isAddingPoints = false;
    
    public TextMeshProUGUI Score;

    [Header("menut")]
    public bool isPaused = false;

    public int chosenMap = 1;

    public GameObject mapChangeButton;

    [Header("car selection")]
    public GameObject currentCar;
    public GameObject[] carListGM;
    public int carIndex;

    [Header("scene asetukset")]
    public string sceneSelected;

    void Awake()
    {
        carListGM = new GameObject[] 
        { 
            GameObject.Find("REALCAR_x"), 
            GameObject.Find("REALCAR"), 
            GameObject.Find("REALCAR_y")
        };

        carIndex = PlayerPrefs.GetInt("CarIndex");
        currentCar = carIndex >= 0 && carIndex < carListGM.Length ? carListGM[carIndex] : carListGM[0];
    
        sceneSelected = SceneManager.GetActiveScene().name;

        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); //poistin koska "DontDestroyOnLoad only works for root GameObjects or components on root GameObjects."
        }
        Debug.Log(instance);

    }
    
    void Start()
    {
        foreach (GameObject car in carListGM)
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

        Debug.Log("after defining:" + PlayerPrefs.GetInt("CarIndex")); //debug
        Debug.Log("after defining CURRENTCAR IS:" + currentCar); //debug
    }



    public void AddPoints()
    {
        Debug.Log("AddPoints executing");
        if (!isAddingPoints && currentCar.activeSelf && instance != null)
        {
            Debug.Log("adding points...");
            StartCoroutine(IncrementScoreWithDelay());
        }
        else
        {
            Debug.LogWarning("YOU SHOULD KILL YOURSELF... NOW!");
        }
    }

    private IEnumerator IncrementScoreWithDelay()
    {
        isAddingPoints = true;   

        while (isAddingPoints)   
        {
            yield return new WaitForSeconds(scoreAddWT);   
            score += 1;   
            Score.text = "Score: " + score.ToString();               
        }
    }

    public void StopAddingPoints()
    {
        isAddingPoints = false;
    }

    public void ChangeMap(Toggle toggle)
    {
        if (toggle != null)
        {
            chosenMap = toggle.isOn ? 2 : 1; //false = 1, true = 2
        }
    }
}
