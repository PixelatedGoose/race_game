using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    int fucker;
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;
    private AudioSource menuMusic;

    private GameObject textScroll;
    private RectTransform rectTransform;
    private Text headline;
    public string[] headlines = new string[]
    {
        "voi vitun paska saatana perkele mitä vittua",
        "Niin justiinsa, sinä sen sanoit",
        "Joonas Kallio! Joonas Kallio! Joonas Kallio!",
        "jdfgkhfkdgjjghubbjlreghwefwovjergubroewöwefwG",
        "vittu nää oot pässi.",
        "Vuoden peli 1997",
        "Uusi tutkimus: alkoholi on vaarallista auton sisällä",
        "Nyt puhutaan asiaa. Aivan pelkkää faktaa. Mitä vittua",
        "Ootko kuullu semmosesta kaverista ku Kari?",
        "kolmen tähen kaveri arvosteli; ei kiinnosta paskaakaa",
        "vittu sun kanssa. kyllä red bull on parempaa ku nocco",
        "miks pelaat tätä paskaa? mene ulos",
        "Sponsored by no one ............................ yet",
        "haastattelimme Karia. hän ei kertonut autoista mitään",
        "Tämä peli on tehty rakkaudella ja viinalla"
    };

    public VideoPlayer videoPlayer;

    void Awake()
    {
        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();
        textScroll = GameObject.Find("textScroll");
        headline = textScroll.GetComponent<Text>();
        rectTransform = textScroll.GetComponent<RectTransform>();
        fullMenu = GameObject.Find("menuCanvas");
        OptionScript = GameObject.Find("Optionspanel").GetComponent<optionScript>();
        GameObject.Find("Optionspanel").SetActive(false);

        fucker = Random.Range(1, 3334);
    }

    void OnEnable()
    {
        Application.targetFrameRate = (int)PlayerPrefs.GetFloat("framerate_value") * 10;
        setHeadline();
        
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
        scrollText();
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
            
            scrollText();
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

    public void scrollText()
    {
        LeanTween.value(textScroll, rectTransform.anchoredPosition.x, -1288.0f, 7f)
        .setEase(LeanTweenType.linear)
        .setOnUpdate((float val) =>
        {
            rectTransform.anchoredPosition = new Vector2(val, rectTransform.anchoredPosition.y);
        }).setOnComplete(() =>
        {
            rectTransform.anchoredPosition = new Vector2(1288.0f, rectTransform.anchoredPosition.y);
            scrollText();
            setHeadline();
        }).setIgnoreTimeScale(true);
    }

    public void setHeadline()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        headline.text = headlines[Random.Range(0, headlines.Length)];
    }
}