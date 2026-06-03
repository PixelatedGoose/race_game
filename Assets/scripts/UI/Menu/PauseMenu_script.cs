using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private GameObject optionsPanel;
    private Selectable firstSelected;

    private RacerScript racerScript;
    private GameObject fullMenu;
    private MusicManager musicMngr;

    void Awake()
    {
        GameManager.Controls.CarControls.pausemenu.performed += ctx => PauseMenuCheck();

        fullMenu = transform.Find("menuCanvas").gameObject;
        Debug.Log(fullMenu);
        optionsPanel = GetComponentInChildren<Options>().gameObject;
        Debug.Log(optionsPanel);
        firstSelected = EventSystem.current.firstSelectedGameObject.GetComponent<Selectable>();
        Debug.Log(firstSelected);
        musicMngr = FindFirstObjectByType<MusicManager>();
        Debug.Log(musicMngr);
    }

    private void OnEnable()
    {
        GameManager.Controls.Enable();
    }
    private void OnDestroy()
    {
        GameManager.Controls.CarControls.pausemenu.performed -= ctx => PauseMenuCheck();
        GameManager.Controls.Disable();
    }

    void Start()
    {
        fullMenu.SetActive(false);
        racerScript = FindFirstObjectByType<RacerScript>();  
    }

    void PauseMenuCheck()
    {
        if (!optionsPanel.activeSelf && !racerScript.raceFinished && racerScript.racestarted) TogglePauseMenu();
    }

    public void TogglePauseMenu()
    {
        Debug.Log($"pause menu should open!");
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
