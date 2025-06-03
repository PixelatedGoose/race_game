using System.Collections;
using UnityEngine;

//tän scriptin ainoa tarkotus on tehä jotai kun se halutaan instructionCatInHand.cs:ssä
//en tosiaan haluais pidentää sitä enää lol
public class fuckshitter : MonoBehaviour
{
    private GameObject beginwall;
    private AudioSource wallMovement_Lower;
    private AudioSource wallMovement_End;

    private musicControlTutorial musicControlTutorial;

    public void SetupFuckShit()
    {
        beginwall = GameObject.Find("TERRAIN/walls_ground/beginwall");
        wallMovement_Lower = GameObject.Find("wallMovement_Lower").GetComponent<AudioSource>();
        wallMovement_End = GameObject.Find("wallMovement_End").GetComponent<AudioSource>();
        musicControlTutorial = GameObject.FindAnyObjectByType<musicControlTutorial>();
    }

    public void DoSomeFuckShit(string value)
    {
        switch (value)
        {
            case "begin":
                LeanTween.moveY(beginwall, -5.5f, 2.5f).setEaseLinear().setOnComplete(() =>
                {
                    wallMovement_End.Play();
                    wallMovement_Lower.Stop();
                });
                wallMovement_Lower.Play();
                return;
            case "predriftfadeout":
                LeanTween.value(musicControlTutorial.mainTrack.volume, 0.0f, 5.0f)
                .setOnUpdate((float val) => {
                    musicControlTutorial.mainTrack.volume = val;
                });
                return;
            default:
                Debug.LogWarning($"case {value} not found");
                return;
        }
    }
}