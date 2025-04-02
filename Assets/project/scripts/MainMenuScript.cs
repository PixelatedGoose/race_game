using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;
    public Toggle mapChangeButton;

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

        mapChangeButton = GameObject.Find("setMapVariant").GetComponent<Toggle>();
        fullMenu = GameObject.Find("menuCanvas");
    }

    void Start()
    {
        LeanTween.moveLocalY(fullMenu, 0.0f, 2.2f).setEase(LeanTweenType.easeOutBounce);

        if (GameManager.instance.chosenMap == 1)
        {
            mapChangeButton.isOn = false;
        }
        else if (GameManager.instance.chosenMap == 2)
        {
            mapChangeButton.isOn = true;
        }
    }

    public void mapSelectedChange()
    {
        //Debug.Log("YOU INSIGNIFICANT FUCK!");
        GameManager.instance.chosenMap = mapChangeButton.isOn ? 2 : 1; //false = 1, true = 2
        PlayerPrefs.SetInt("chosenMap", GameManager.instance.chosenMap);
        PlayerPrefs.Save();
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