using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

    }

    CarInputActions Controls;

    private void Onable()
    {
        Controls.Enable();
    }

    private void Disable()
    {
        Controls.Disable();
    }

    public GameObject[] pauseMenuObjects;



    void Update()
    {
        if (Controls.CarControls.pausemenu.triggered)
        {
            LeanTween.cancel(pauseMenuObjects[0]);
            if (pauseMenuObjects != null && pauseMenuObjects.Length > 0)
            {
                bool isActive = pauseMenuObjects[0].activeSelf;
                foreach (GameObject obj in pauseMenuObjects)
                {
                    obj.SetActive(!isActive);
                    GameManager.instance.isPaused = !isActive;
                }

                if (isActive)
                {
                    Time.timeScale = 1;
                    //GameManager.instance.isPaused = false;
                }
                else
                {
                    //GameManager.instance.isPaused = true;
                    pauseMenuObjects[0].transform.localPosition = new Vector3(0.0f, 470.0f, 0.0f);
                    Time.timeScale = 0;

                    LeanTween.moveLocalY(pauseMenuObjects[0], 0.0f, 0.4f).setEaseInOutCirc().setIgnoreTimeScale(true);
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
        GameManager.instance.isPaused = false;
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
    }
}