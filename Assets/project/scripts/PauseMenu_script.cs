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

                if (isActive)
                {
                    Time.timeScale = 1; // Resume game
                }
                else
                {
                    Time.timeScale = 0; // Pause game
                }
            }
            else
            {
                Debug.LogError("PauseMenuObjects array is not assigned or empty.");
            }
        }
    }
}