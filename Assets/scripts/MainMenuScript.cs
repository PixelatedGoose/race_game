using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    int randomChance;
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;
    private AudioSource menuMusic;
    public VideoPlayer videoPlayer;

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
    }

    void OnEnable()
    {
        Application.targetFrameRate = (int)PlayerPrefs.GetFloat("framerate_value") * 10;

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
            LeanTween.moveLocalY(fullMenu, 0.0f, 1.5f).setEase(LeanTweenType.easeOutBounce);
            menuMusic.Play();
        }
    }

    // changed: show the play-confirm UI instead of immediately loading
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
    }

    // called by the "Play Game" button on the playConfirmPanel
    public void ConfirmPlay()
    {
        SceneManager.LoadSceneAsync(7); //menee ny carselectioniin suoraan
        DatapersistenceManager.instance.LoadGame();
    }

    // optional: called by a "Back" button on the playConfirmPanel
    public void CancelPlay()
    {
        if (playConfirmPanel != null)
            playConfirmPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void PlayTutorial()
    {
        SceneManager.LoadSceneAsync(5);
        DatapersistenceManager.instance.LoadGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Credits()
    {
        SceneManager.LoadSceneAsync(10);
    }
}