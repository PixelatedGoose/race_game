using System.Collections;
using UnityEngine;
using Logitech;
using UnityEngine.UI;

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
        }
    }

    IEnumerator NoCountdown()
    {
        yield return new WaitForSecondsRealtime(0f);
        LogitechLedController.Clear();
        racerScript.StartRace();
    }

    IEnumerator ShowS1AfterDelay()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        // 3 - LEDs at 33% (lowest amount of lights)
        s3.SetActive(true);
        LogitechLedController.SetNormalized(0.33f);
        count3.Play();
        LeanTween.value(s3.GetComponent<RawImage>().color.a, 0.0f, 0.9f)
        .setOnUpdate((float val) =>
        {
            var img = s3.GetComponent<RawImage>();
            Color c = img.color;
            c.a = val;
            img.color = c;
        })
        .setIgnoreTimeScale(true)
        .setEaseLinear();
        yield return new WaitForSecondsRealtime(1.0f);

        // 2 - LEDs at 66% (more lights)
        s2.SetActive(true);
        LogitechLedController.SetNormalized(0.66f);
        count2.Play();
        LeanTween.value(s2.GetComponent<RawImage>().color.a, 0.0f, 0.9f)
        .setOnUpdate((float val) =>
        {
            var img = s2.GetComponent<RawImage>();
            Color c = img.color;
            c.a = val;
            img.color = c;
        })
        .setIgnoreTimeScale(true)
        .setEaseLinear();
        yield return new WaitForSecondsRealtime(1.0f);

        // 1 - LEDs at 100% (all lights)
        s1.SetActive(true);
        LogitechLedController.SetNormalized(1.0f);
        count1.Play();
        LeanTween.value(s1.GetComponent<RawImage>().color.a, 0.0f, 0.9f)
        .setOnUpdate((float val) =>
        {
            var img = s1.GetComponent<RawImage>();
            Color c = img.color;
            c.a = val;
            img.color = c;
        })
        .setIgnoreTimeScale(true)
        .setEaseLinear();
        yield return new WaitForSecondsRealtime(1.0f);

        // GO!
        go.SetActive(true);
        countGo.Play();
        LogitechLedController.Clear();
        Time.timeScale = 1f;
        racerScript.StartRace();

        LeanTween.value(go, go.GetComponent<RawImage>().color.a, 0.0f, 2f)
        .setOnUpdate((float val) =>
        {
            var img = go.GetComponent<RawImage>();
            Color c = img.color;
            c.a = val;
            img.color = c;
        })
        .setEaseLinear();
        yield return new WaitForSecondsRealtime(2f);
        s3.SetActive(false);
        s2.SetActive(false);
        s1.SetActive(false);
        go.SetActive(false);
    }
}