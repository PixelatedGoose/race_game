using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject fullMenu;
    public GameObject mapChangeButton;
    public int chosenMap;

    private void Start()
    {
        chosenMap = 1;
        LeanTween.moveY(fullMenu, 267.5f, 2.2f).setEase(LeanTweenType.easeOutBounce);
    }

    public void ChangeMap(Toggle toggle)
    {
        if (toggle != null)
        {
            chosenMap = toggle.isOn ? 2 : 1; //false = 1, true = 2
        }
    }

    public void Playgame()
    {
        SceneManager.LoadSceneAsync(chosenMap);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}