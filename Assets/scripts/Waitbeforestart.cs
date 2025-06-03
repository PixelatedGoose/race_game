using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Waitbeforestart : MonoBehaviour
{
    public GameObject s1;
    public GameObject s2;
    public GameObject s3;
    public GameObject go;
    public RacerScript racerScript; // Assign in Inspector!

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
        
        s2.SetActive(false);
        s1.SetActive(false);
        go.SetActive(false);

        if (GameManager.instance.sceneSelected != "tutorial")
        {
            count1 = GameObject.Find("count1").GetComponent<AudioSource>();
            count2 = GameObject.Find("count2").GetComponent<AudioSource>();
            count3 = GameObject.Find("count3").GetComponent<AudioSource>();
            countGo = GameObject.Find("countGo").GetComponent<AudioSource>();

            StartCoroutine(ShowS1AfterDelay());
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

    //tutorial map ei tykänny alottaa kisaa ilman delayta jostai syystä
    IEnumerator NoCountdown()
    {
        yield return new WaitForSecondsRealtime(0f);

        racerScript.StartRace(); // Start the race!
    }

    IEnumerator ShowS1AfterDelay()
    {
        Time.timeScale = 0f;

        count3.Play();
        yield return new WaitForSecondsRealtime(1.0f);
        s3.SetActive(false);

        s2.SetActive(true);
        count2.Play();
        yield return new WaitForSecondsRealtime(1.0f);
        s2.SetActive(false);

        s1.SetActive(true);
        count1.Play();
        yield return new WaitForSecondsRealtime(1.0f);
        s1.SetActive(false);

        go.SetActive(true);
        countGo.Play();
        Time.timeScale = 1f;
        racerScript.StartRace(); // Start the race!

        LeanTween.alphaText(go.GetComponent<RectTransform>(), 0.0f, 2f).setEaseLinear();
        yield return new WaitForSecondsRealtime(2f);
        go.SetActive(false);
    }
}