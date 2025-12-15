using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class soundFXControl : MonoBehaviour
{
    CarInputActions Controls;
    //säilyttää KAIKKI äänet, paitsi ne, joita käytetään interactableiden kanssa
    public GameObject[] soundList;
    //kaikki äänet, joita käytetään interactablea käyttäessä
    public GameObject[] soundClickList;
    public GameObject[] soundButtonsList;
    public GameObject[] soundSlidersList;
    public GameObject[] soundTogglesList;
    public RacerScript racerScript;

    abstract class soundFXAttributes<T> where T : Component
    {
        protected AudioSource soundToPlay;
        protected T componentReference;

        public soundFXAttributes(T gameobject, AudioSource sound)
        {
            componentReference = gameobject;
            soundToPlay = sound;
        }
        public abstract void AddSoundToComponent();
    }

    class soundFXButton : soundFXAttributes<Button>
    {
        //base() toimii constructorina ilma toistoa; tässä ne asetetaan tyhjäksi
        public soundFXButton(Button button, AudioSource sound) : base(button, sound) { }
        public override void AddSoundToComponent()
        {
            if (componentReference != null)
            {
                componentReference.onClick.AddListener(() =>
                {
                    soundToPlay.Play();
                });
            }
        }
    }

    class soundFXSlider : soundFXAttributes<Slider>
    {
        public soundFXSlider(Slider slider, AudioSource sound) : base(slider, sound) { }
        public override void AddSoundToComponent()
        {
            if (componentReference != null)
            {
                componentReference.onValueChanged.AddListener((value) =>
                {
                    soundToPlay.Play();
                });
            }
        }
    }

    class soundFXToggle : soundFXAttributes<Toggle>
    {
        public soundFXToggle(Toggle toggle, AudioSource sound) : base(toggle, sound) { }
        public override void AddSoundToComponent()
        {
            if (componentReference != null)
            {
                componentReference.onValueChanged.AddListener((value) =>
                {
                    soundToPlay.Play();
                });
            }
        }
    }



    void Awake()
    {
        Controls = new CarInputActions();
        racerScript = FindFirstObjectByType<RacerScript>();
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

    private void FindSoundGameObjects()
    {
        //eti äänet tässä
        soundList = GameObject.FindGameObjectsWithTag("soundFX");
        soundList = soundList.OrderBy(a => a.name).ToArray();

        soundClickList = GameObject.FindGameObjectsWithTag("soundFXonClick"); //koska array on vitun paska
        soundClickList = soundClickList.OrderBy(a => a.name).ToArray();
    }

    void Start()
    {
        FindSoundGameObjects();

        if (soundClickList.Length == 0)
        {
            Debug.LogError("EI ÄÄNIÄ SOUNDCLICKLISTISSÄ");
        }

        SoundFXHandler("button", soundButtonsList, soundClickList[0].GetComponent<AudioSource>());
        SoundFXHandler("toggle", soundTogglesList, soundClickList[1].GetComponent<AudioSource>());
        SoundFXHandler("slider", soundSlidersList, soundClickList[2].GetComponent<AudioSource>());
    }

    void LateUpdate()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        if (SceneManager.GetActiveScene().name == "Carselectionmenu_VECTORAMA") return;
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
    public void SoundFXHandler(string type, GameObject[] componentsToChange, AudioSource soundToPlay)
    {
        //componentit saa eri muutoksia riippuen tyypistä
        foreach (GameObject component in componentsToChange)
        {    
            switch (type)
            {
                case "button":
                    Button buttonComponent = component.GetComponent<Button>();
                    if (buttonComponent != null)
                    {
                        //soundFXButton (soundFX) toimii setuppina
                        //sama koskee jokasta eri classia
                        var soundFX = new soundFXButton(buttonComponent, soundToPlay);
                        soundFX.AddSoundToComponent();
                    }
                    break;
                case "slider":
                    Slider sliderComponent = component.GetComponent<Slider>();
                    if (sliderComponent != null)
                    {
                        var soundFX = new soundFXSlider(sliderComponent, soundToPlay);
                        soundFX.AddSoundToComponent();
                    }
                    break;
                case "toggle":
                    Toggle toggleComponent = component.GetComponent<Toggle>();
                    if (toggleComponent != null)
                    {
                        var soundFX = new soundFXToggle(toggleComponent, soundToPlay);
                        soundFX.AddSoundToComponent();
                    }
                    break;
                default:
                    Debug.Log("Nope!");
                    break;
            }
        }
    }

    public void PauseStateHandler()
    {
        bool isPaused = GameManager.instance.isPaused;
        if (isPaused)
            soundList[4].GetComponent<AudioSource>().Play();

        foreach (GameObject sound in soundList)
        {
            AudioSource audioSource = sound.GetComponent<AudioSource>();
            if (isPaused)
            {
                Debug.Log(sound + " pysäytetty");
                audioSource.Pause();
            }
            else
            {
                Debug.Log(sound + " ei pysäytetty");
                audioSource.UnPause();
            }
        }
    }
}