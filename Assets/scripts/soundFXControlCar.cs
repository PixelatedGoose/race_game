using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class soundFXControlCar : MonoBehaviour
{
    CarInputActions Controls;
    public GameObject[] soundList;
    public GameObject[] soundClickList;
    public GameObject[] soundButtonsList;
    public GameObject[] soundSlidersList;
    public GameObject[] soundTogglesList;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    void Start()
    {
        //eti äänet tässä
        soundList = GameObject.FindGameObjectsWithTag("soundFX");
        soundList = soundList.OrderBy(a => a.name).ToArray();

        soundClickList = GameObject.FindGameObjectsWithTag("soundFXonClick"); //koska array on vitun paska
        soundClickList = soundClickList.OrderBy(a => a.name).ToArray();

        //hell
        //paska koodi



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
                    soundClickList.First(obj => obj.name == "optionSliderTick").GetComponent<AudioSource>().Play();
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
                    soundClickList.First(obj => obj.name == "optionClick").GetComponent<AudioSource>().Play();
                });
            }
        }

        if (soundClickList.Length == 0)
        {
            Debug.LogError("no sounds");
        }
    }

    private void OnEnable()
    {
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }

    private void OnDestroy()
    {
        Controls.Disable();
        //Controls.Dispose();
    }
}