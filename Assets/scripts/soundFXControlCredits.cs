using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class soundFXControlCredits : MonoBehaviour
{
    public GameObject[] soundClickList;
    public GameObject[] soundButtonsList;

    void Start()
    {
        //eti äänet tässä
        soundClickList = GameObject.FindGameObjectsWithTag("soundFXonClick"); //koska array on vitun paska
        soundClickList = soundClickList.OrderBy(a => a.name).ToArray();

        foreach (GameObject soundButton in soundButtonsList) //jokaselle niistä (jotta niitä voidaan käyttää)
        {
            Button soundButtonComponent = soundButton.GetComponent<Button>(); //eti nappi itessään

            if (soundButtonComponent != null) //jos se on olemas
            {
                soundButtonComponent.onClick.AddListener(() => //lisää listener jokaiseen "Button" componentin "On Click" toimintoon, jotta...
                {
                    soundClickList[0].GetComponent<AudioSource>().Play(); //...ääni voiaan toistaa
                });
            }
        }

        if (soundClickList.Length == 0)
        {
            Debug.LogError("no sounds");
        }
    }
}