using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class soundFXControl : MonoBehaviour
{
    //the pain is inbound
    class soundFXAttributes
    {
        public GameObject objectForSound;
        public GameObject[] allSoundsList;
    }

    class soundFXButton : soundFXAttributes
    {
        public Button buttonComponent;
        public void AddSoundToComponent()
        {
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() =>
                {
                    this.allSoundsList[0].GetComponent<AudioSource>().Play();
                });
            }
        }
    }
    class soundFXSlider : soundFXAttributes
    {
        public Slider sliderComponent;
    }
    class soundFXToggle : soundFXAttributes
    {
        public Toggle toggleComponent;
    }



    CarInputActions Controls;
    public GameObject[] soundList;
    public GameObject[] soundClickList;
    public GameObject[] soundButtonsList;
    public GameObject[] soundSlidersList;
    public GameObject[] soundTogglesList;

    public RacerScript racerScript;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
        racerScript = FindFirstObjectByType<RacerScript>();
    }

    void Start()
    {
        //eti äänet tässä
        soundList = GameObject.FindGameObjectsWithTag("soundFX");
        soundList = soundList.OrderBy(a => a.name).ToArray();

        soundClickList = GameObject.FindGameObjectsWithTag("soundFXonClick"); //koska array on vitun paska
        soundClickList = soundClickList.OrderBy(a => a.name).ToArray();

        if (GameManager.instance.sceneSelected != "tutorial")
            soundList[1].GetComponent<AudioSource>().Play();

        //hell
        //paska koodi, rewrite myöhemmi

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
    }

    void LateUpdate()
    {
        if (Controls.CarControls.pausemenu.triggered && racerScript.racestarted == true)
        {
            PauseStateHandler();
        }
    }

    /// <summary>
    /// liittää jokaseen määritettyyn nappiin, slideriin ja toggleen niitten omat äänet.
    /// korjaa sen ikivanhan kolmen foreachin koodin
    /// </summary>
    /// <param name="type">selittää itse itsensä</param>
    
    //sais inherittaa classin ja säätää sen switch-case statementis
    public void SoundFXHandler(string type)
    {
        switch (type)
        {
            case "button":
            case "slider":
            case "toggle":
            default:
                Debug.Log("Nope!");
                break;
        }
        
        //placeholder
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
    }

    public void PauseStateHandler()
    {
        if (GameManager.instance.isPaused == true)
        {
            soundList[2].GetComponent<AudioSource>().volume = 1.0f;
            foreach (GameObject sound in soundList)
            {
                if (sound.GetComponent<AudioSource>().name != "pausedTrack")
                {
                    Debug.Log(sound + " pysäytetty");
                    sound.GetComponent<AudioSource>().Pause();
                }
            }
        }
        else if (GameManager.instance.isPaused == false)
        {
            soundList[2].GetComponent<AudioSource>().volume = 0.0f;
            foreach (GameObject sound in soundList)
            {
                if (sound.GetComponent<AudioSource>().name != "pausedTrack")
                {
                    Debug.Log(sound + " ei pysäytetty");
                    sound.GetComponent<AudioSource>().UnPause();
                }
            }
        }
    }
}