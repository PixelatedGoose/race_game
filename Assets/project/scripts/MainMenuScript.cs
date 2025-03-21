using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;
    public GameObject mapChangeButton;

    private void Awake()
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

        // var optionSlider = GameObject.Find("pixel").GetComponent<UnityEngine.UI.Slider>();
        // optionSlider.value = OptionScript.pixelCount.GetFloat("_pixelcount");
    }

    private void Start()
    {
        GameManager.instance.chosenMap = 1;
        LeanTween.moveLocalY(fullMenu, 0.0f, 2.2f).setEase(LeanTweenType.easeOutBounce);
    }

    public void ChangeMap(Toggle toggle)
    {
        if (toggle != null)
        {
            GameManager.instance.chosenMap = toggle.isOn ? 2 : 1; //false = 1, true = 2
        }
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