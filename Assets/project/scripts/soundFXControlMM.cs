using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class soundFXControlMM : MonoBehaviour
{
    public GameObject[] soundClickList;
    public GameObject[] soundButtonsList;
    public GameObject[] soundSlidersList;
    public GameObject[] soundTogglesList;

    void Start()
    {
        //eti äänet tässä
        soundClickList = GameObject.FindGameObjectsWithTag("soundFXonClick"); //koska array on vitun paska
        soundClickList = soundClickList.OrderBy(a => a.name).ToArray();



        //hell
        //REFACTOROIN TÄN MYÖHEMMIN REGARDS LA CREATURA

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

        foreach (GameObject soundSlider in soundSlidersList)
        {
            Slider soundSliderComponent = soundSlider.GetComponent<Slider>();

            if (soundSliderComponent != null)
            {
                soundSliderComponent.onValueChanged.AddListener(value =>
                {
                    soundClickList[2].GetComponent<AudioSource>().Play();
                });
            }
        }

        foreach (GameObject soundToggle in soundTogglesList)
        {
            Toggle soundToggleComponent = soundToggle.GetComponent<Toggle>();

            if (soundToggleComponent != null)
            {
                soundToggleComponent.onValueChanged.AddListener(value =>
                {
                    soundClickList[1].GetComponent<AudioSource>().Play();
                });
            }
        }

        if (soundClickList.Length == 0)
        {
            Debug.LogError("no sounds");
        }
    }
}