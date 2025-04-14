using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;

    void Awake()
    {
        //PlayerPrefs.DeleteAll(); //VAIN DEBUGAAMISTA VARTEN
        if (PlayerPrefs.HasKey("pixel_value"))
        {
            Debug.Log("pixel_value löydetty; ei muuteta");
        }
        else
        {
            OptionScript.pixelCount.SetFloat("_pixelcount", 256);
            Debug.Log("pixel_value ei löydetty; arvo on nyt 256");
        }

        fullMenu = GameObject.Find("menuCanvas");
        OptionScript = GameObject.Find("Optionspanel").GetComponent<optionScript>();
        GameObject.Find("Optionspanel").SetActive(false);
    }

    void Start()
    {
        LeanTween.moveLocalY(fullMenu, 0.0f, 1.5f).setEase(LeanTweenType.easeOutBounce);
    }

    public void Playgame()
    {
        SceneManager.LoadSceneAsync(3); //menee ny carselectioniin suoraan
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}