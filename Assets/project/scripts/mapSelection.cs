using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class mapSelection : MonoBehaviour
{
    public GameObject csObjects;
    public GameObject msObjects;
    public GameObject[] msObjectsList;
    private float schizophrenia;
    private RawImage loadImage;

    void Awake()
    {
        csObjects = GameObject.Find("carSelectionObj");
        msObjects = GameObject.Find("mapSelectionObj");
        loadImage = GameObject.Find("loadImage").GetComponent<RawImage>();
        msObjectsList = GameObject.FindGameObjectsWithTag("msObj");
    }

    public void Back()
    {
        csObjects.SetActive(true);
        msObjects.SetActive(false);
    }

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
        }
    }

    private IEnumerator MapButtonFunc()
    {
        PlayerPrefs.Save();
        GameManager.instance.chosenMap = PlayerPrefs.GetInt("chosenMap");

        schizophrenia = Random.Range(1.0f, 4.0f);
        LeanTween.moveLocalY(loadImage.gameObject, 0, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        foreach (GameObject theobject in msObjectsList)
        {
            LeanTween.moveLocalY(theobject, theobject.transform.position.y + 451, 0.8f).setEase(LeanTweenType.easeInOutCubic);
        }

        Debug.Log("you will now wait for: " + schizophrenia + "seconds");
        yield return new WaitForSeconds(schizophrenia);

        SceneManager.LoadSceneAsync(PlayerPrefs.GetInt("chosenMap"));
    }
}