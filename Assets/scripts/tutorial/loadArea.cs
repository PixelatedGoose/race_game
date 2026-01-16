using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadArea : MonoBehaviour
{
    public new Collider collider;
    public string prefix;
    private instructionHandler instructionHandler; //jotta tää on paljon helpompaa
    private musicControlTutorial musicControlTutorial;


    
    void Awake()
    {
        prefix = gameObject.name.Substring(0, 2);
        collider = GetComponent<Collider>();
        instructionHandler = GameObject.Find("instructionHandler").GetComponent<instructionHandler>();
        musicControlTutorial = GameObject.Find("music").GetComponent<musicControlTutorial>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hello! My name is Gustavo, but you can call me Gus");
        switch (prefix)
        {
            case "01":
                musicControlTutorial.StartNonIntroTracks();
                musicControlTutorial.MusicSections("2_FINAL_TUTORIAL_main");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "02":
                instructionHandler.ShowInstruction
                (instructionHandler.GetInstruction("driving", 3)
                , 1);

                StartCoroutine(FadeDeath(1.0f));
                break;
            case "03":
                musicControlTutorial.MusicSections("3_FINAL_TUTORIAL_main", "fade");
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "04":
                musicControlTutorial.MusicSections("4_FINAL_TUTORIAL_main", "fade");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "05":
                musicControlTutorial.MusicSections("5_FINAL_TUTORIAL_main", "fade");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            //drift
            case "DD":
                CarController carController = FindAnyObjectByType<CarController>();
                carController.canDrift = true;
                
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            //uskon että on unused, pidän varmuuden vuoksi
            case "06":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            //after drift
            case "07":
                instructionHandler.index = 1;
                instructionHandler.ShowInstruction(
                    instructionHandler.GetInstruction(
                        instructionHandler.curCategory,
                        instructionHandler.index));
                StartCoroutine(FadeDeath(1.0f));
                break;
            //turbe
            case "TT":
                CarController carController3 = FindAnyObjectByType<CarController>();
                carController3.canUseTurbo = true;

                musicControlTutorial.EnableTurboFunctions();
                musicControlTutorial.MusicSections("7_FINAL_TUTORIAL_1main", "fade");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "08":
                musicControlTutorial.StopNonIntroTracks();
                musicControlTutorial.turboTrack.Stop();
                //ig just in case?
                musicControlTutorial.turboTrack.volume = 0f;
                musicControlTutorial.driftTrack.Stop();
                musicControlTutorial.driftTrack.volume = 0f;
                musicControlTutorial.MusicSections("8_FINAL_TUTORIAL_outro");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "EX":
                RaceResultCollector collector = FindFirstObjectByType<RaceResultCollector>();
                collector.SaveRaceResult("JoonasKallio");

                //alkuperäsesti oli musicControlTutorialissa (mitä vittua)
                SceneManager.LoadSceneAsync(0);
                break;
            case "RE":
                RacerScript racerScript = FindFirstObjectByType<RacerScript>();
                //ensi kerralla korjaamme nää miljoona reset juttua :)
                racerScript.RespawnAtLastCheckpoint();
                break;
            default:
                Debug.LogError($"prefix {prefix} not defined");
                break;
        }
    }

    /// <summary>
    /// tekee pikku fade outin ja tuhoaa objektin määritetyn ajan jälkee
    /// </summary>
    /// <param name="seconds">sekunnit</param>
    private IEnumerator FadeDeath(float seconds)
    {
        LeanTween.alpha(gameObject, 0f, seconds).setEaseLinear();
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
} 