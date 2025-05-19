using System.Collections;
using UnityEngine;

public class loadArea : MonoBehaviour
{
    public new Collider collider;
    public string prefix;
    public instructionHandler instructionHandler; //jotta tää on paljon helpompaa



    void Awake()
    {
        prefix = gameObject.name.Substring(0, 2);
        collider = GetComponent<Collider>();
        instructionHandler = GameObject.Find("instructionHandler").GetComponent<instructionHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hello! My name is Gustavo, but you can call me Gus");
        switch (prefix)
        {
            case "01":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                break;
            case "02":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 2);
                break;
            case "03":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 3);
                break;
            case "11":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                StartCoroutine(FadeDeath(1.0f));
                Debug.Log("mf really died");
                break;
            default:
                Debug.Log($"prefix {prefix} not defined");
                break;
        }
    }

    /// <summary>
    /// tuhoaa objektin määritetyn ajan jälkee
    /// </summary>
    /// <param name="seconds">sekunnit</param>
    private IEnumerator FadeDeath(float seconds)
    {
        LeanTween.alpha(gameObject, 0f, seconds).setEaseLinear();
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
} 