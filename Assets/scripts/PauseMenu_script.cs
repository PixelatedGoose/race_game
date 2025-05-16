using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject Optionspanel;
    public GameObject[] pauseMenuObjects;

    private bool optionsOpen => Optionspanel != null && Optionspanel.activeSelf;
    private CarInputActions Controls;
    public RacerScript racerScript;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        if (pauseMenuObjects == null || pauseMenuObjects.Length == 0)
            Debug.LogWarning("PauseMenuObjects array is not assigned or empty.");
        if (Optionspanel == null)
            Debug.LogWarning("Optionspanel is not assigned.");
    }

    private void OnEnable() => Controls.Enable();
    private void OnDisable() => Controls.Disable();
    private void OnDestroy() => Controls.Disable();

    void Start()
    {
        racerScript = FindFirstObjectByType<RacerScript>();
    }

    void Update()
    {
        if (Controls.CarControls.pausemenu.triggered && !optionsOpen && !racerScript.raceFinished
        && racerScript.racestarted)
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        if (pauseMenuObjects == null || pauseMenuObjects.Length == 0) return;

        LeanTween.cancel(pauseMenuObjects[0]);
        bool isActive = pauseMenuObjects[0].activeSelf;

        foreach (GameObject obj in pauseMenuObjects)
        {
            obj.SetActive(!isActive);
        }
        GameManager.instance.isPaused = !isActive;

        if (isActive)
        {
            Time.timeScale = 1;
        }
        else
        {
            pauseMenuObjects[0].transform.localPosition = new Vector3(0.0f, 470.0f, 0.0f);
            Time.timeScale = 0;
            LeanTween.moveLocalY(pauseMenuObjects[0], 0.0f, 0.4f).setEaseInOutCirc().setIgnoreTimeScale(true);
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
        GameManager.instance.isPaused = false;
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}