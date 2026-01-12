using System.Collections;
using UnityEngine;
using Logitech;

public class Waitbeforestart : MonoBehaviour
{
    public GameObject s1;
    public GameObject s2;
    public GameObject s3;
    public GameObject go;
    public RacerScript racerScript;

    public AudioSource count1;
    public AudioSource count2;
    public AudioSource count3;
    public AudioSource countGo;

    void Start()
    {
        s1 = GameObject.Find("s1");
        s2 = GameObject.Find("s2");
        s3 = GameObject.Find("s3");
        go = GameObject.Find("go");
        
        s3.SetActive(false);
        s2.SetActive(false);
        s1.SetActive(false);
        go.SetActive(false);

        // Clear LEDs at start
        LogitechLedController.Clear();

        if (GameManager.instance.sceneSelected != "tutorial")
        {
            count1 = GameObject.Find("count1").GetComponent<AudioSource>();
            count2 = GameObject.Find("count2").GetComponent<AudioSource>();
            count3 = GameObject.Find("count3").GetComponent<AudioSource>();
            countGo = GameObject.Find("countGo").GetComponent<AudioSource>();

            StartCoroutine(ShowS1AfterDelay());
            Time.timeScale = 0f;
        }
        else
        {
            count1 = null;
            count2 = null;
            count3 = null;
            countGo = null;
            StartCoroutine(NoCountdown());
            s3.SetActive(false);
        }
    }

    void Update()
    {
        LogitechGSDK.LogiUpdate();
    }

    //tutorial map ei tykänny alottaa kisaa ilman delayta jostai syystä
    IEnumerator NoCountdown()
    {
        yield return new WaitForSecondsRealtime(0f);
        LogitechLedController.Clear();
        racerScript.StartRace();
    }

    IEnumerator ShowS1AfterDelay()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        Vector3 tweenEndScale = new(0.0f, 0.0f, 0.0f);

        // 3 - LEDs at 33%
        s3.SetActive(true);
        LogitechLedController.SetNormalized(0.33f);
        count3.Play();
        //ei hyvää päivää
        LeanTween.value(s3, s3.GetComponent<RectTransform>().localScale, tweenEndScale, 1.0f)
        .setOnUpdate((Vector3 val) => s3.GetComponent<RectTransform>().localScale = val)
        .setEaseInCirc()
        .setIgnoreTimeScale(true);
        yield return new WaitForSecondsRealtime(1.0f);

        // 2 - LEDs at 66%
        s2.SetActive(true);
        LogitechLedController.SetNormalized(0.66f);
        count2.Play();
        LeanTween.value(s2, s2.GetComponent<RectTransform>().localScale, tweenEndScale, 1.0f)
        .setOnUpdate((Vector3 val) => s2.GetComponent<RectTransform>().localScale = val)
        .setEaseInCirc()
        .setIgnoreTimeScale(true);
        yield return new WaitForSecondsRealtime(1.0f);

        // 1 - LEDs at 100%
        s1.SetActive(true);
        LogitechLedController.SetMax();
        count1.Play();
        LeanTween.value(s1, s1.GetComponent<RectTransform>().localScale, tweenEndScale, 1.0f)
        .setOnUpdate((Vector3 val) => s1.GetComponent<RectTransform>().localScale = val)
        .setEaseInCirc()
        .setIgnoreTimeScale(true);
        yield return new WaitForSecondsRealtime(1.0f);

        // GO! - Flash LEDs then clear
        go.SetActive(true);
        countGo.Play();
        Time.timeScale = 1f;
        racerScript.StartRace();

        // Flash LEDs leobold times
        StartCoroutine(FlashLeds(8, 0.2f));

        LeanTween.alphaText(go.GetComponent<RectTransform>(), 0.0f, 2f).setEaseLinear();
        yield return new WaitForSecondsRealtime(2f);
        s3.SetActive(false);
        s2.SetActive(false);
        s1.SetActive(false);
        go.SetActive(false);
    }

    IEnumerator FlashLeds(int times, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            LogitechLedController.SetMax();
            yield return new WaitForSecondsRealtime(interval);
            LogitechLedController.Clear();
            yield return new WaitForSecondsRealtime(interval);
        }
    }
}