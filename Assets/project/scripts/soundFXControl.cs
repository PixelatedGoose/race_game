using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class soundFXControl : MonoBehaviour
{
    CarInputActions Controls;
    private GameObject[] soundList;
    private GameObject[] soundClickListPRE;
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

        soundClickListPRE = GameObject.FindGameObjectsWithTag("soundFXonClick"); //koska array on vitun paska
        soundClickList = soundClickListPRE.OrderBy(a => a.name).ToArray();

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

    private void Onable()
    {
        Controls.Enable();
    }

    private void Disable()
    {
        Controls.Disable();
    }

    void Update()
    {
        if (GameManager.instance.isPaused == false)
        {
            soundList[0].GetComponent<AudioSource>().volume = 0.0f;
        }
        else if (GameManager.instance.isPaused == true)
        {
            soundList[0].GetComponent<AudioSource>().volume = 1.0f;
        }
    }
}