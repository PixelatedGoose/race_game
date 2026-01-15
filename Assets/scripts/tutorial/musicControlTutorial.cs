using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class musicControlTutorial : MonoBehaviour
{
    CarInputActions Controls;
    public AudioSource[] musicListSources;
    public AudioSource mainTrack;
    public AudioSource driftTrack;
    public AudioSource turboTrack;
    public AudioSource[] variants;

    private enum CarMusicState {Main, Drift, Turbo};
    private CarMusicState CurrentMusState = CarMusicState.Main;
    private CarMusicState LatestMusState = CarMusicState.Main;
    private int[] activeTweenIDs;

    private CarController carController;

    void OnEnable()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        carController = FindAnyObjectByType<CarController>();
        Controls.CarControls.pausemenu.performed += ctx => PausedMusicHandler();
    }
    private void OnDisable()
    {
        LeanTween.cancelAll();
        Controls.CarControls.Drift.performed -= DriftCall;
        Controls.CarControls.Drift.canceled -= DriftCanceled;
        Controls.CarControls.turbo.performed -= TurboCall;
        Controls.CarControls.turbo.canceled -= TurboCanceled;
        Controls.Disable();
    }
    private void OnDestroy() => Controls.Disable();

    public void EnableDriftFunctions()
    {
        Controls.CarControls.Drift.performed += DriftCall;
        Controls.CarControls.Drift.canceled += DriftCanceled;
    }
    public void EnableTurboFunctions()
    {
        Controls.CarControls.turbo.performed += TurboCall;
        Controls.CarControls.turbo.canceled += TurboCanceled;
    }

    //kaikki tarpeelline on täs
    void DriftCall(InputAction.CallbackContext context)
    {
        CurrentMusState = carController.isTurboActive ? CarMusicState.Turbo : CarMusicState.Drift;
        FadeLayerTracks();
    }
    void DriftCanceled(InputAction.CallbackContext context)
    {
        CurrentMusState = carController.isTurboActive ? CarMusicState.Turbo : CarMusicState.Main;
        FadeLayerTracks();
    }
    void TurboCall(InputAction.CallbackContext context)
    {
        CurrentMusState = CarMusicState.Turbo;
        FadeLayerTracks();
    }
    void TurboCanceled(InputAction.CallbackContext context)
    {
        CurrentMusState = carController.isDrifting ? CarMusicState.Drift : CarMusicState.Main;
        FadeLayerTracks();
    }



    void Start()
    {
        musicListSources = gameObject.GetComponents<AudioSource>()
        .OrderBy(a => a.name).ToArray();
        TrackVariants();

        //debug
        StartNonIntroTracks();
        carController.canDrift = true;
        carController.canUseTurbo = true;
        MusicSections("7_FINAL_TUTORIAL_1main");
        EnableDriftFunctions();
        EnableTurboFunctions();
        //the TRUE death of TrackedTween
        activeTweenIDs = new int[musicListSources.Length];
    }

    public void StartNonIntroTracks()
    {
        foreach (AudioSource musicTrack in musicListSources)
        {
            if (!musicTrack.isPlaying)
            {
                musicTrack.Play();
            }
        }
    }
    public void StopNonIntroTracks()
    {
        foreach (AudioSource musicTrack in musicListSources)
        {
            musicTrack.Stop();
        }
    }
    void TrackVariants(bool set = false)
    {
        string clipName = mainTrack.clip.name;

        // Get the prefix (e.g. first two characters)
        string prefix = clipName.Substring(0, 1);

        //updated the fucker jotta se käyttää suoraan soossia eikä gameobjectei
        variants = musicListSources
            .Select(go => go)
            .Where(a => a.name.StartsWith(prefix))
            .OrderBy(a => a.name)
            .ToArray();
            
        if (set)
        {
            mainTrack = variants.Length > 0 ? variants[0] : null;
            driftTrack = variants.Length > 1 ? variants[1] : null;
            turboTrack = variants.Length > 2 ? variants[2] : null;
        }
        else if (!set)
        {
            Debug.Log("assuming track has no variants", mainTrack);
        }
    }

    void Update()
    {
        Debug.Log(mainTrack);
    }

    void ChangeTrack(string selectedAudio)
    {
        mainTrack = GameObject.Find(selectedAudio).GetComponent<AudioSource>();
        TrackVariants(true);
    }

    /// <summary>
    /// vaihtaa musiikkiraidat trackNamen mukaan
    /// </summary>
    /// <param name="trackName">koko tiedostonimi, ilman .wav päätettä</param>
    public void MusicSections(string trackName, string mode = "instant") //lisään myöhemmi oikeet fade outit ja transitionit
    {
        float volSet = 0.28f;

        switch (mode)
        {
            case "instant":
                mainTrack.Stop();
                if (driftTrack != null)
                    driftTrack.Stop();
                if (turboTrack != null)
                    turboTrack.Stop();

                ChangeTrack(trackName);
                mainTrack.volume = volSet;

                mainTrack.Play();
                if (driftTrack != null)
                    driftTrack.Play();
                if (turboTrack != null)
                    turboTrack.Play();
                break;
        }
    }

    private void FadeLayerTracks()
    {
        Debug.Log("begin function");
        //tarkistaa staten ku funktio alkaa, ei tarvi muualla
        if (CurrentMusState == LatestMusState) return;
        Debug.Log("currentmusstate is not latestmusstate");



        int stateIndex = (int)CurrentMusState; //current on oikeesti se viimeisin lol
        int previousStateIndex = (int)LatestMusState;
        //tein uniikin arrayn näitten säilytykselle
        AudioSource NextTrack = variants[stateIndex];
        AudioSource PreviousTrack = variants[previousStateIndex];
        Debug.Log($"{NextTrack.name} {PreviousTrack.name}");

        // Cancel any existing tweens on these tracks
        if (activeTweenIDs[stateIndex] != -1)
            LeanTween.cancel(activeTweenIDs[stateIndex]);
        if (activeTweenIDs[previousStateIndex] != -1)
            LeanTween.cancel(activeTweenIDs[previousStateIndex]);

        // Start new tweens and store their IDs
        activeTweenIDs[stateIndex] =
        LeanTween.value(NextTrack.volume, 0.3f, 1.0f)
            .setOnUpdate(val => NextTrack.volume = val)
            .id;
        activeTweenIDs[previousStateIndex] =
        LeanTween.value(PreviousTrack.volume, 0.0f, 1.0f)
            .setOnUpdate(val => PreviousTrack.volume = val)
            .id;
        
        LatestMusState = CurrentMusState;
    }

    public void PausedMusicHandler()
    {
        bool isPaused = GameManager.instance.isPaused;
        foreach (AudioSource musicTrack in musicListSources)
        {
            if (isPaused)
                musicTrack.Pause();
            else
                musicTrack.UnPause();
        }
    }

    //jotta instructionCheck.cs ei callaa tätä scriptii 5 kertaa
    public void BeginDriftSection()
    {
        mainTrack.volume = 0f;
        StopNonIntroTracks();
        MusicSections("6_FINAL_TUTORIAL_1main");
        StartNonIntroTracks();
    }
}