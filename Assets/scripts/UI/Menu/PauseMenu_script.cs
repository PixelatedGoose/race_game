using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private GameObject optionsPanel;
    private Selectable firstSelected;

    private CarInputActions Controls;
    private RacerScript racerScript;
    private GameObject fullMenu;
    private MusicManager musicMngr;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.CarControls.pausemenu.performed += ctx => PauseMenuCheck();

        fullMenu = transform.Find("menuCanvas").gameObject;
        optionsPanel = GetComponentInChildren<Options>().gameObject;
        firstSelected = EventSystem.current.firstSelectedGameObject.GetComponent<Selectable>();
        musicMngr = FindFirstObjectByType<MusicManager>();
    }

    private void OnEnable() => Controls.Enable();
    private void OnDisable()
    {
        Controls.CarControls.pausemenu.performed -= ctx => PauseMenuCheck();
        Controls.Disable();
    }
    private void OnDestroy()
    {
        Controls.CarControls.pausemenu.performed -= ctx => PauseMenuCheck();
        Controls.Disable();
    }

    void Start()
    {
        fullMenu.SetActive(false);
        optionsPanel.SetActive(false);
        racerScript = FindFirstObjectByType<RacerScript>();  
    }

    void PauseMenuCheck()
    {
        if (!optionsPanel.activeSelf && !racerScript.raceFinished && racerScript.racestarted) TogglePauseMenu();
    }

    public void TogglePauseMenu()
    {
        fullMenu.SetActive(!fullMenu.activeSelf);
        LeanTween.cancel(fullMenu);
        SFXManager SFXMngr = FindFirstObjectByType<SFXManager>();
        Time.timeScale = fullMenu.activeSelf ? 0 : 1;
        if (musicMngr != null) musicMngr.PausedMusicHandler();
        if (SFXMngr != null && racerScript.racestarted) SFXMngr.PauseStateHandler();

        if (!fullMenu.activeSelf) return;

        fullMenu.transform.localPosition = new(-400.0f, 0.0f, 0.0f);
        LeanTween.moveLocalX(fullMenu, 0.0f, 0.4f).setEaseInOutCirc().setIgnoreTimeScale(true);
        firstSelected.Select();
    }

    public void QuitGame()
    {
        Destroy(musicMngr); //sillä emme pidä ongelmista
        SceneManager.LoadSceneAsync("MainMenu");
    }
    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}
