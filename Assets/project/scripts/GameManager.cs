using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject mapChangeButton;
    public static GameManager instance;

    private int score;
    public float scoreAddWT = 0.01f; //WT = wait time

    private bool isAddingPoints = false;
    
    public TextMeshProUGUI Score;

    public GameObject currentCar;

    public bool isPaused = false;

    public int chosenMap = 1;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // //else
        // {
        //     Destroy(gameObject);
        // }
    }

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }



    public void AddPoints()
    {
        if (!isAddingPoints && currentCar.activeInHierarchy)
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
            Score.text = "Points: " + score.ToString();               
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
