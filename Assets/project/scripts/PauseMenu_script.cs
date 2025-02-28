using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject[] pauseMenuObjects;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuObjects != null && pauseMenuObjects.Length > 0)
            {
                bool isActive = pauseMenuObjects[0].activeSelf;
                foreach (GameObject obj in pauseMenuObjects)
                {
                    obj.SetActive(!isActive);
                }
                LeanTween.cancel(pauseMenuObjects[0]);
                LeanTween.moveLocalY(pauseMenuObjects[0], 460.0f, 0.0f).setEaseInOutCirc().setIgnoreTimeScale(true);
                LeanTween.moveLocalY(pauseMenuObjects[0], 0.0f, 0.4f).setEaseInOutCirc().setIgnoreTimeScale(true);

                if (isActive)
                {
                    Time.timeScale = 1;
                    LeanTween.moveLocalY(pauseMenuObjects[0], 460.0f, 0.4f).setEaseInOutCirc().setIgnoreTimeScale(true);
                }
                else
                {
                    Time.timeScale = 0;
                }
            }
            else
            {
                Debug.LogError("PauseMenuObjects array is not assigned or empty.");
            }
        }
    }

    public void ContinueGame()
    {
        foreach (GameObject obj in pauseMenuObjects)
        {
            obj.SetActive(false);
        }
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
    }
}