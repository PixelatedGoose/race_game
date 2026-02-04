using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class SelectionMenuNewestComboDoubleTroubleExtraSauce : MonoBehaviour
{
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

    [SerializeField] private int selectionIndex = 0;
    [SerializeField] private GameObject[] selectionMenus;
    [SerializeField] private GameObject detailsPanel;



    void Awake()
    {
        selectionDetails = Resources.Load<TextAsset>("selectionDetails");
        //i'm dictionarying my dictionary
        details = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(selectionDetails.text);
        /* selectText = GameObject.Find("SelectYoMap").GetComponent<Text>();
        //pitää ettiä tekstit jollai array tavalla
        scoreText = GameObject.Find("ScoreOnThaAuto").GetComponent<Text>();
        loadObjects = GameObject.Find("loadObjects");
        msObjectsList = GameObject.FindGameObjectsWithTag("msObj");
        loadingLoop = GameObject.Find("loadingLoop").GetComponent<AudioSource>(); */

        selectionMenus = GameObject.FindGameObjectsWithTag("selectionMenu")
        .OrderBy(go => go.name).ToArray();
        selectionMenus[1].SetActive(false);
        selectionMenus[2].SetActive(false);

        //detailsPanel = GameObject.Find("detailsPanel");
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

    //selection > singleplayer, ai botit, multiplayer
    //ai botit on sama ku singleplayer + ai bottien säätö selection (mihi se menee?)
    //tätä ekaa valintaa käytetään määrittämään arrayn koko
    //(paitsi multiplayer koska se on eri scene)

    //map > car > ai options (?) > options > gaming singleplayeris
    //uus scene: lobby > map > car > options > gaming multiplayeris

    //helpottaa asioit ja se on coroutine jo valmiiksi
    //poistan turhan methodin myöhemmin
    public void MapFallAnim()
    {
        MapFallAnimFunc();
    }

    private IEnumerator MapButtonPress()
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

    private void MapFallAnimFunc()
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
    }

    private void Update()
    {
        //tän kaiken pitää executtaa AINOASTAAN kun se vaihtuu jos se on helppoa
        GameObject current = EventSystem.current.currentSelectedGameObject;
        //Debug.LogWarning(current);

        if (current != null)
        {
            //vuoden indeksoinnit siitä
            if (details[selectionMenus[selectionIndex].name].ContainsKey(current.name))
                detailsPanelText.text = details[selectionMenus[selectionIndex].name][current.name];
            //säilytä edellinen teksti details ruudus jos dropdown on valittuna
            else if (current.name.StartsWith("Item"))
                return;
            else
                detailsPanelText.text = "";
        }
    }

    /* 1. map, 2. car, 3. settings, 4. loading :))))
    vaihtaa sit indeksien mukaan tota
    HUOM. ylemmän buttonin teksti vois vaihtaa "go!" kun on viimeses osas */
    public void Next()
    {
        selectionIndex++;
        selectionMenus[selectionIndex].SetActive(true);
        selectionMenus[selectionIndex - 1].SetActive(false);
    }
    public void Back()
    {
        //normal back
        if (selectionIndex != 0)
        {
            selectionIndex--;
            selectionMenus[selectionIndex].SetActive(true);
            selectionMenus[selectionIndex + 1].SetActive(false);
            //jos index == 0 mentyään takasin
            if (selectionIndex == 0)
            {
                detailsPanel.SetActive(false);
            }
        }
        else
        {
            SceneManager.LoadSceneAsync("MainMenu");
        }
    }

    /// <summary>
    /// WIP
    /// </summary>
    /// <param name="selecta">WIP</param>
    private void SetMapToLoad(string selecta)
    {
        string selectedMap;
        if (AItoggle.isOn)
            selectedMap = $"ai_{selecta}";
        else
            selectedMap = selecta;
        //if (dropdown == night)
            //selectedmap += $"_night"
        //else
            //nothing; valmiiks laitettu jo

        PlayerPrefs.SetString("SelectedMap", selectedMap);
        PlayerPrefs.Save();
        Debug.Log($"onnittelut, voitat lomamatkan kohteeseen: {selectedMap}");
    }
}
