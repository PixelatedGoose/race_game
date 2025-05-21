using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] optionScript OptionScript;
    public GameObject fullMenu;
    private AudioSource menuMusic;

    private GameObject textScroll;
    private RectTransform rectTransform;
    private Text headline;
    public string[] headlines = new string[]
    {
        "voi vitun paska saatana perkele mitä vittua",
        "kenen vitun idea oli tää paskanen paska",
        "Joonas Kallio! Joonas Kallio! Joonas Kallio!",
        "jdfgkhfkdgjjghubbjlreghwefwovjergubroewöwefwG",
        "Tämä on esimerkki viestistä, joka voi näkyä. Iso pässi.",
        "Vuoden peli 1997",
        "Uusi tutkimus: alkoholi on vaarallista auton sisällä",
        "Nyt puhutaan asiaa. Aivan pelkkää faktaa. Mitä vittua",
        "Ootko kuullu semmosesta kaverista ku Kari?",
        "kolmen tähen kaveri arvosteli; ei kiinnosta paskaakaa",
        "vittu sun kanssa. kyllä red bull on parempaa ku nocco"
    };

    void Awake()
    {
        menuMusic = GameObject.Find("menuLoop").GetComponent<AudioSource>();
        textScroll = GameObject.Find("textScroll");
        headline = textScroll.GetComponent<Text>();
        rectTransform = textScroll.GetComponent<RectTransform>();
        fullMenu = GameObject.Find("menuCanvas");
        OptionScript = GameObject.Find("Optionspanel").GetComponent<optionScript>();
        GameObject.Find("Optionspanel").SetActive(false);
    }

    void OnEnable()
    {
        setHeadline();
    }

    void Start()
    {
        LeanTween.moveLocalY(fullMenu, 0.0f, 1.5f).setEase(LeanTweenType.easeOutBounce);
        menuMusic.Play();
        
        scrollText();
    }

    public void Playgame()
    {
        SceneManager.LoadSceneAsync(3); //menee ny carselectioniin suoraan
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