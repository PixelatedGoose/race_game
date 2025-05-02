using System.Collections;
using UnityEngine;

public class Waitbeforestart : MonoBehaviour
{
    public GameObject s1;
    public GameObject s2;
    public GameObject s3;
    public GameObject go;
    public RacerScript racerScript; // Assign in Inspector!

    void Start()
    {
        if (GameManager.instance.sceneSelected != "tutorial")
        {
            StartCoroutine(ShowS1AfterDelay());
        }
        else
        {
            StartCoroutine(NoCountdown());
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

        s3.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        s3.SetActive(false);

        s2.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        s2.SetActive(false);

        s1.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        s1.SetActive(false);

        go.SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);
        go.SetActive(false);
        Time.timeScale = 1f;
        racerScript.StartRace(); // Start the race!
    }
}