using System.Collections;
using UnityEngine;

public class loadArea : MonoBehaviour
{
    public new Collider collider;
    public string prefix;
    public instructionHandler instructionHandler; //jotta tää on paljon helpompaa
    public musicControlTutorial musicControlTutorial;



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
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "12":
                StartCoroutine(ChangeAnimOverrides("driving:3", 1)); //manuaalisesti koska fuck this shit
                instructionHandler.index = 3;

                instructionHandler.ShowInstruction
                (instructionHandler.GetInstruction("driving", 3)
                , 1);
                
                //StartCoroutine(ChangeAnimOverrides("driving:1", 2));
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "50":
                musicControlTutorial.MusicSections("2_FINAL_TUTORIAL_main");
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "51":
                musicControlTutorial.MusicSections("3_FINAL_TUTORIAL_main");
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "52":
                musicControlTutorial.MusicSections("4_FINAL_TUTORIAL_main");
                StartCoroutine(FadeDeath(1.0f));
                break;
            case "53":
                musicControlTutorial.MusicSections("5_FINAL_TUTORIAL_main");
                StartCoroutine(FadeDeath(1.0f));
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