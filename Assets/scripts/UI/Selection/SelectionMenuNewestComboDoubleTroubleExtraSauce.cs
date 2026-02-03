using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Reflection.Emit;
using Newtonsoft.Json;
using System.Linq;

public class SelectionMenuNewestComboDoubleTroubleExtraSauce : MonoBehaviour
{
    public GameObject carSelectionO;
    public GameObject mapSelectionO;
    public GameObject optionSelection;
    public GameObject[] msObjectsList;
    private float schizophrenia;
    private GameObject loadObjects;
    private AudioSource loadingLoop;
    public GameObject[] maps;
    private RectTransform mapRectTransform;
    private Text selectText;
    private Text scoreText;
    public Toggle AItoggle;

    private TextAsset selectionDetails;
    private Dictionary<string, Dictionary<string, string>> details;
    [SerializeField] private TextMeshProUGUI detailsPanelText;

    void Awake()
    {
        selectionDetails = Resources.Load<TextAsset>("selectionDetails");
        //i'm dictionarying my dictionary
        details = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(selectionDetails.text);
        /* selectText = GameObject.Find("SelectYoMap").GetComponent<Text>();
        //pitää ettiä tekstit jollai array tavalla
        scoreText = GameObject.Find("ScoreOnThaAuto").GetComponent<Text>();
        carSelectionO = GameObject.Find("CarSelectionNew");
        mapSelectionO = GameObject.Find("mapSelectionObj");
        loadObjects = GameObject.Find("loadObjects");
        msObjectsList = GameObject.FindGameObjectsWithTag("msObj");
        loadingLoop = GameObject.Find("loadingLoop").GetComponent<AudioSource>(); */

        //mfw kun pitää siirtää koko maps paska car selection scriptiin
    }

    public void MapFallAnimResetPos()
    {
        foreach (GameObject map in maps)
        {
            LeanTween.cancel(map);
            mapRectTransform = map.GetComponent<RectTransform>();
            mapRectTransform.anchoredPosition = new Vector2(mapRectTransform.anchoredPosition.x, 280.0f);
        }
        LeanTween.cancel(selectText.gameObject);
        LeanTween.cancel(scoreText.gameObject);
        selectText.rectTransform.anchoredPosition = new Vector2(320.0f, selectText.rectTransform.anchoredPosition.y);
        scoreText.rectTransform.anchoredPosition = new Vector2(520.0f, scoreText.rectTransform.anchoredPosition.y);
    }

    /// <summary>
    /// käytetään mapin valintaan. ottaa mapin PlayerPrefsistä
    /// </summary>
    /// <param name="selecta">PERUS mappi, jonka haluat ladata</param>
    public void MapButtonPress(string selecta)
    {
        string selectedMapTrue;
        if (AItoggle.isOn)
            selectedMapTrue = $"ai_{selecta}";
        else
            selectedMapTrue = selecta;
        PlayerPrefs.SetString("SelectedMap", selectedMapTrue);
        PlayerPrefs.Save();
        Debug.Log($"onnittelut, voitit lomamatkan kohteeseen: {selectedMapTrue}");

        StartCoroutine(MapButtonFunc());
    }

    //helpottaa asioit ja se on coroutine jo valmiiksi
    public void MapFallAnim()
    {
        StartCoroutine(MapFallAnimFunc());
    }

    private IEnumerator MapButtonFunc()
    {
        loadingLoop.Play();

        schizophrenia = Random.Range(3.5f, 6.5f);
        LeanTween.moveLocalY(loadObjects.gameObject, -0.5f, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        foreach (GameObject theobject in msObjectsList)
        {
            LeanTween.moveLocalY(theobject, theobject.transform.position.y + 451, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        }

        Debug.Log("you will now wait for: " + schizophrenia + " seconds");
        yield return new WaitForSeconds(schizophrenia);
        
        SceneManager.LoadSceneAsync(PlayerPrefs.GetString("SelectedMap"));
    }

    private IEnumerator MapFallAnimFunc()
    {
        LeanTween.value(selectText.gameObject, selectText.rectTransform.anchoredPosition.x, -20.0f, 2f)
            .setEase(LeanTweenType.easeOutExpo)
            .setOnUpdate((float val) =>
            {
            selectText.rectTransform.anchoredPosition = new Vector2(val, selectText.rectTransform.anchoredPosition.y);
            });

        LeanTween.value(scoreText.gameObject, scoreText.rectTransform.anchoredPosition.x, -20.0f, 2f)
            .setEase(LeanTweenType.easeOutExpo)
            .setOnUpdate((float val) =>
            {
            scoreText.rectTransform.anchoredPosition = new Vector2(val, scoreText.rectTransform.anchoredPosition.y);
            });

        foreach (GameObject mapImg in maps)
        {
            RectTransform rectTransform = mapImg.GetComponent<RectTransform>();

            //LeanTween.value on ainoa tapa muuttaa Rect Transformin y:tä
            LeanTween.value(mapImg, rectTransform.anchoredPosition.y, -60.0f, 0.7f)
            .setEase(LeanTweenType.easeInOutExpo)
            .setOnUpdate((float val) =>
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, val);
            });
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void Update()
    {
        //tän kaiken pitää executtaa AINOASTAAN kun se vaihtuu jos se on helppoa
        GameObject current = EventSystem.current.currentSelectedGameObject;
        Debug.LogWarning(current);
        
        if (current != null)
        {
            if (details["options"].ContainsKey(current.name))
                detailsPanelText.text = details["options"][current.name];
            else if (current.name.StartsWith("Item"))
                Debug.Log("do nothing lol");
            else
                detailsPanelText.text = "";
        }
    }

    //tää pitää muuttaa arrayta tai vastaavaa käyttäväks, joka sisältää:
    /* 1. map, 2. car, 3. settings, 4. loading :))))
    vaihtaa sit indeksien mukaan tota

    HUOM. alemman buttonin teksti pitää vaihtaa "exit" kun on ekassa osassa
    ja ylemmän buttonin teksti pitää vaihtaa "go!" tai jtn samanlaista */
    public void ActivateMapSelection()
    {
        carSelectionO.SetActive(false);
        mapSelectionO.SetActive(true);
    }

    /* public void Back()
    {
        MapFallAnimResetPos();
        carSelectionO.SetActive(true);
        mapSelectionO.SetActive(false);
    } */

    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
