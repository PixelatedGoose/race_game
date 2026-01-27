using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
    int randomChance;
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;
    private AudioSource menuMusic;
    public VideoPlayer videoPlayer;
    private int musictweenIDstart = -1;
    private int musictweenIDend = -1;

    // new: assign in inspector (or the script will try to find "PlayPanel")
    [SerializeField] private GameObject playConfirmPanel;
    [SerializeField] private GameObject mainMenuPanel; // assign in Inspector (fallback to Find below)

    void Awake()
    {
        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();

        fullMenu = GameObject.Find("menuCanvas");
        OptionScript = GameObject.Find("Optionspanel").GetComponent<optionScript>();
        GameObject.Find("Optionspanel").SetActive(false);

        if (mainMenuPanel == null)
            mainMenuPanel = GameObject.Find("MainMenu");
        if (playConfirmPanel == null)
            playConfirmPanel = GameObject.Find("PlayPanel");
        if (playConfirmPanel != null)
            playConfirmPanel.SetActive(false);

        randomChance = Random.Range(1, 3334);

        //hei nimeni on main menu ja tykkään vittuilla koodaajille
        OptionScript.CacheUIElements();
        OptionScript.InitializeSliderValues();
        OptionScript.InitializeToggleValues();
    }

    void OnEnable()
    {
        if (randomChance <= 2)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }
    }
    void OnVideoFinished(VideoPlayer vp)
    {
        videoPlayer.Stop();
        Destroy(videoPlayer);

        LeanTween.moveLocalY(fullMenu, 0.0f, 1.5f).setEase(LeanTweenType.easeOutBounce);
        menuMusic.Play();
    }
    void OnDisable()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    void Start()
    {
        if (randomChance <= 2)
        {
            return;
        }
        else
        {
            LeanTween.moveLocalY(fullMenu, 0.0f, 1.5f)
            .setEase(LeanTweenType.easeOutBounce)
            .setOnStart(() =>
            {
                menuMusic.Play();
            });
        }
    }

    // changed: show the play-confirm UI instead of immediately loading
    // new: events for camera animation (assign in Inspector)
    [SerializeField] private UnityEvent onShowPlayMenu;
    [SerializeField] private UnityEvent onHidePlayMenu;

    public void Playgame()
    {
        if (playConfirmPanel == null)
        {
            // fallback to old behaviour if no UI panel found
            ConfirmPlay();
            return;
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        playConfirmPanel.SetActive(true);

        // signal camera to move forward on Play
        onShowPlayMenu?.Invoke();
    }

    // called by the "Play Game" button on the playConfirmPanel
    public void ConfirmPlay()
    {
        SceneManager.LoadSceneAsync("Carselectionmenu_VECTORAMA"); //menee ny carselectioniin suoraan
        DatapersistenceManager.instance.LoadGame();
    }

    // optional: called by a "Back" button on the playConfirmPanel
    public void CancelPlay()
    {
        if (playConfirmPanel != null)
            playConfirmPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // signal camera to move back to start
        onHidePlayMenu?.Invoke();
    }

    public void MainMenuMusic(bool active)
    {
        //jotta ei oo pelkästää truena tai falsena olemassa
        switch (active)
        {
            case true:
                if (musictweenIDend != -1)
                    LeanTween.cancel(musictweenIDend);

                musictweenIDstart = LeanTween.value(menuMusic.volume, 0.27f, 0.9f)
                .setOnUpdate(val => menuMusic.volume = val).id;
                break;
            case false:
                if (musictweenIDstart != -1)
                    LeanTween.cancel(musictweenIDstart);
                musictweenIDend = LeanTween.value(menuMusic.volume, 0.0f, 0.9f)
                .setOnUpdate(val => menuMusic.volume = val).id;
                break;
        }
    }

    public void PlayTutorial()
    {
        SceneManager.LoadSceneAsync("tutorial");
        DatapersistenceManager.instance.LoadGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}