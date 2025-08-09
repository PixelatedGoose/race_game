using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    int fucker;
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;
    private AudioSource menuMusic;
    public VideoPlayer videoPlayer;

    void Awake()
    {
        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();

        fullMenu = GameObject.Find("menuCanvas");
        OptionScript = GameObject.Find("Optionspanel").GetComponent<optionScript>();
        GameObject.Find("Optionspanel").SetActive(false);

        fucker = Random.Range(1, 3334);
    }

    void OnEnable()
    {
        Application.targetFrameRate = (int)PlayerPrefs.GetFloat("framerate_value") * 10;

        if (fucker <= 2)
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
        if (fucker <= 2)
        {
            return;
        }
        else
        {
            LeanTween.moveLocalY(fullMenu, 0.0f, 1.5f).setEase(LeanTweenType.easeOutBounce);
            menuMusic.Play();
        }
    }

    public void Playgame()
    {
        SceneManager.LoadSceneAsync(7); //menee ny carselectioniin suoraan
        DatapersistenceManager.instance.LoadGame();
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