using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int score;

    private bool isAddingPoints = false;
    
    public TextMeshProUGUI Score;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    public void AddPoints()
    {
        if (!isAddingPoints)  
        {
            StartCoroutine(IncrementScoreWithDelay());
        }
    }

    private IEnumerator IncrementScoreWithDelay()
    {
        isAddingPoints = true;   

        while (isAddingPoints)   
        {
            yield return new WaitForSeconds(0.01f);   
            score += 1;   
            Score.text = "Points: " + score.ToString();               

        }
    }

    public void StopAddingPoints()
    {
        isAddingPoints = false;  
    }
}
