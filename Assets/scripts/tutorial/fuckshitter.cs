using System.Collections;
using UnityEngine;

//tän scriptin ainoa tarkotus on tehä jotai kun se halutaan instructionCatInHand.cs:ssä
//en tosiaan haluais pidentää sitä enää lol
public class fuckshitter : MonoBehaviour
{
    private GameObject beginwall;
    private AudioSource wallMovement_Lower;

    public void SetupFuckShit()
    {
        beginwall = GameObject.Find("TERRAIN/walls_ground/beginwall");
        wallMovement_Lower = GameObject.Find("wallMovement_Lower").GetComponent<AudioSource>();
    }

    public void DoSomeFuckShit(string value)
    {
        switch (value)
        {
            case "begin":
                LeanTween.moveY(beginwall, -2f, 2.5f).setEaseInOutSine();
                wallMovement_Lower.Play();
                return;
            case "oh_no":
                Debug.Log("uhh");
                return;
            default:
                Debug.LogWarning($"case {value} not found");
                return;
        }
    }
}