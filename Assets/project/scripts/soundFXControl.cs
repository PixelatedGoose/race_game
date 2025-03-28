using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class soundFXControl : MonoBehaviour
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

        soundList[1].GetComponent<AudioSource>().Play();

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
        if (Controls.CarControls.pausemenu.triggered)
        {
            if (GameManager.instance.isPaused == false)
            {
                soundList[2].GetComponent<AudioSource>().volume = 0.0f;
                foreach (GameObject sound in soundList)
                {
                    if (sound.GetComponent<AudioSource>().name != "pausedTrack")
                    {
                        sound.GetComponent<AudioSource>().UnPause();
                    }
                }
            }
            else if (GameManager.instance.isPaused == true)
            {
                soundList[2].GetComponent<AudioSource>().volume = 1.0f;
                foreach (GameObject sound in soundList)
                {
                    if (sound.GetComponent<AudioSource>().name != "pausedTrack")
                    {
                        sound.GetComponent<AudioSource>().Pause();
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (Mathf.Floor(GameManager.instance.carSpeed) == 0)
        {
            Debug.Log("HJTRSGOIERGERNFEWAKJFNWAGIUERAGESRJGÖOERBFDJHESERWBJHGFDRGVERGTERNJG");
            soundList[1].GetComponent<AudioSource>().volume = 0.0f;
        }
        
        if (GameManager.instance.carSpeed > 0) //kesken
        {
            soundList[1].GetComponent<AudioSource>().pitch = GameManager.instance.carSpeed / 40;
            soundList[1].GetComponent<AudioSource>().volume = GameManager.instance.carSpeed / 80;
        }
    }

    public static float Floor(float aValue, int aDigits) //aValue = pyöristettävä, aDigits = desimaalit - jotta voi pyöristää tasan 0.7:ään
    {
        float m = Mathf.Pow(10,aDigits);
        aValue *= m;
        aValue = Mathf.Floor(aValue);
        return aValue / m;
    }
}