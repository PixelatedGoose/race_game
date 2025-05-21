using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class mapSelection : MonoBehaviour
{
    public GameObject csObjects;
    public GameObject msObjects;
    public GameObject[] msObjectsList;
    private float schizophrenia;
    private GameObject loadObjects;
    private AudioSource loadingLoop;
    public GameObject[] maps;
    private RectTransform mapRectTransform;

    void Awake()
    {
        csObjects = GameObject.Find("carSelectionObj");
        msObjects = GameObject.Find("mapSelectionObj");
        loadObjects = GameObject.Find("loadObjects");
        msObjectsList = GameObject.FindGameObjectsWithTag("msObj");
        loadingLoop = GameObject.Find("loadingLoop").GetComponent<AudioSource>();

        //mfw kun pitää siirtää koko maps paska car selection scriptiin
    }

    public void Back()
    {
        MapFallAnimResetPos();
        csObjects.SetActive(true);
        msObjects.SetActive(false);
    }

    public void MapFallAnimResetPos()
    {
        foreach (GameObject map in maps)
        {
            LeanTween.cancel(map);
            mapRectTransform = map.GetComponent<RectTransform>();
            mapRectTransform.anchoredPosition = new Vector2(mapRectTransform.anchoredPosition.x, 280.0f);
        }
    }

    /// <summary>
    /// käytetään mapin valintaan. ottaa mapin PlayerPrefsistä
    /// </summary>
    /// <param name="selecta">mappi, jonka haluat ladata</param>
    public void MapButtonPress(int selecta)
    {
        switch (selecta)
        {
            case 1:
                PlayerPrefs.SetInt("chosenMap", 1);
                StartCoroutine(MapButtonFunc());

                break;
            case 2:
                PlayerPrefs.SetInt("chosenMap", 2);
                StartCoroutine(MapButtonFunc());

                break;
            case 4:
                PlayerPrefs.SetInt("chosenMap", 4);
                StartCoroutine(MapButtonFunc());

                break;
            case 6:
                PlayerPrefs.SetInt("chosenMap", 6);
                StartCoroutine(MapButtonFunc());

                break;
            case 8:
                PlayerPrefs.SetInt("chosenMap", 8);
                StartCoroutine(MapButtonFunc());

                break;
        }
    }

    //helpottaa asioit ja se on coroutine jo valmiiksi
    public void MapFallAnim()
    {
        StartCoroutine(MapFallAnimFunc());
    }

    private IEnumerator MapButtonFunc()
    {
        loadingLoop.Play();
        PlayerPrefs.Save();
        GameManager.instance.chosenMap = PlayerPrefs.GetInt("chosenMap");

        schizophrenia = Random.Range(3.5f, 6.5f);
        LeanTween.moveLocalY(loadObjects.gameObject, -0.5f, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        foreach (GameObject theobject in msObjectsList)
        {
            LeanTween.moveLocalY(theobject, theobject.transform.position.y + 451, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        }

        Debug.Log("you will now wait for: " + schizophrenia + "seconds");
        yield return new WaitForSeconds(schizophrenia);

        //en tiiä onko performance riski ottaa chosenMap GameManagerista
        SceneManager.LoadSceneAsync(PlayerPrefs.GetInt("chosenMap"));
    }

    private IEnumerator MapFallAnimFunc()
    {
        foreach (GameObject mapImg in maps)
        {
            RectTransform rectTransform = mapImg.GetComponent<RectTransform>();

            //LeanTween.value on ainoa tapa muuttaa Rect Transformin y:tä
            LeanTween.value(mapImg, rectTransform.anchoredPosition.y, (int)-60.0f, 0.5f)
            .setEase(LeanTweenType.easeInOutExpo)
            .setOnUpdate((float val) =>
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, val);
            });
            yield return new WaitForSeconds(0.3f);
        }
    }
}