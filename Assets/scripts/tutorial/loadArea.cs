using System.Collections;
using UnityEngine;

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
            //eri animaatiot
            case "01":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "02":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 2);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "03":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 3);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "11":
                musicControlTutorial.MusicSections("2_FINAL_TUTORIAL_main");
                musicControlTutorial.StartNonIntroTracks();            
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "12":
                StartCoroutine(ChangeAnimOverrides("driving:3", 1)); //manuaalisesti koska fuck this shit
                instructionHandler.index = 3;

                instructionHandler.ShowInstruction
                (instructionHandler.GetInstruction("driving", 3)
                , 1);

                StartCoroutine(FadeDeath(1.0f));
                break;                
            case "13":
                musicControlTutorial.MusicSections("3_FINAL_TUTORIAL_main", "fade");
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "14":
                musicControlTutorial.MusicSections("4_FINAL_TUTORIAL_main", "fade");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "15":
                musicControlTutorial.MusicSections("5_FINAL_TUTORIAL_main", "fade");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "26":
                CarController carController = GameObject.FindAnyObjectByType<CarController>();
                carController.canDrift = true;
                break;
            case "16":
                musicControlTutorial.MusicSections("6_FINAL_TUTORIAL_main");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "17":
                CarController carController3 = GameObject.FindAnyObjectByType<CarController>();
                carController3.canUseTurbo = true;
                musicControlTutorial.MusicSections("7_FINAL_TUTORIAL_main", "fade");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "18":
                musicControlTutorial.MusicSections("8_FINAL_TUTORIAL_main");
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "50":
                //AIKA VITTU LOPPUU JA KELLO ON 2 yöllä EI SAATANA EI PERKELE EI VITTEJFSFKJGERGDREDOK
                instructionHandler.ShowInstruction("You've passed a checkpoint. They change where you respawn upon reset (press R [keyboard] / D-Pad Left [controller] to test).");
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "51":
                CameraFollow cameraFollowRead = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
                cameraFollowRead.rotOffset = new Vector3(cameraFollowRead.rotOffset.x, 3.0f);
                break;
            case "52":
                CameraFollow cameraFollow3 = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
                cameraFollow3.rotOffset = new Vector3(cameraFollow3.rotOffset.x, 1.7f);
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

    private IEnumerator ChangeAnimOverrides(string instruction, int value)
    {
        if (instruction == null)
        {
            Debug.LogError("NO INSTRUCTION WITH NAME: " + instruction, gameObject);
            yield break;
        }

        if (instructionHandler.instructionAnimOverrides.ContainsKey(instruction))
        {
            instructionHandler.instructionAnimOverrides[instruction] = value;
            Debug.Log("success! modified: " + instruction + ", " + value);
        }
        else
        {
            instructionHandler.instructionAnimOverrides.Add(instruction, value);
            Debug.Log("success! added: " + instruction + ", " + value);
        }

        yield break;
    }
} 