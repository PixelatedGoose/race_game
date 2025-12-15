using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject Optionspanel;
    public GameObject[] pauseMenuObjects;

    private bool optionsOpen => Optionspanel != null && Optionspanel.activeSelf;
    private CarInputActions Controls;
    public RacerScript racerScript;
    public GameObject fullMenu;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
        Controls.CarControls.pausemenu.performed += PauseMenuCheck;

        if (pauseMenuObjects == null || pauseMenuObjects.Length == 0)
            Debug.LogWarning("PauseMenuObjects array is not assigned or empty.");
        if (Optionspanel == null)
            Debug.LogWarning("Optionspanel is not assigned.");

        fullMenu = GameObject.Find("menuCanvas");
    }

    private void OnEnable() => Controls.Enable();
    private void OnDisable() => Controls.Disable();
    private void OnDestroy() => Controls.Disable();

    void Start()
    {
        fullMenu.SetActive(false);
        Optionspanel.SetActive(false);
        racerScript = FindFirstObjectByType<RacerScript>();
    }

    void PauseMenuCheck(InputAction.CallbackContext context)
    {
        if (!optionsOpen && !racerScript.raceFinished && racerScript.racestarted)
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
            GameManager.instance.StopAddingPoints();
            Time.timeScale = 0;
            LeanTween.moveLocalY(pauseMenuObjects[0], 0.0f, 0.4f).setEaseInOutCirc().setIgnoreTimeScale(true);
        }
    }

    //ronnyinen funktio tääl näi
    public void SetPausedState(bool paused)
    {
        Time.timeScale = paused ? 0 : 1;
        GameManager.instance.isPaused = paused;
    }

    public void ContinueGame()
    {
        foreach (GameObject obj in pauseMenuObjects)
        {
            obj.SetActive(false);
        }
        SetPausedState(false);
    }
    public void QuitGame()
    {
        SetPausedState(false);
        SceneManager.LoadSceneAsync(0);
    }
    public void RestartGame()
    {
        SetPausedState(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
