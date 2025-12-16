using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class musicControl : MonoBehaviour
{
    public GameObject[] musicObjects;
    public AudioSource[] musicTracks;
    private enum CarMusicState {Main, Drift, Turbo};
    private CarMusicState CurrentMusState = CarMusicState.Main;
    private CarMusicState LatestMusState = CarMusicState.Main;
    private int[] activeTweenIDs;

    private CarController carController;
    CarInputActions Controls;

    void Awake()
    {
        Controls = new CarInputActions();
        carController = FindAnyObjectByType<CarController>();

        Controls.CarControls.Drift.performed += DriftCall;
        Controls.CarControls.Drift.canceled += DriftCanceled;
        Controls.CarControls.turbo.performed += TurboCall;
        Controls.CarControls.turbo.canceled += TurboCanceled;
        Controls.CarControls.pausemenu.performed += ImPauseMenuingIt;
    }

    private void OnEnable() => Controls.Enable();
    private void OnDisable()
    {
        LeanTween.cancelAll();
        Controls.Disable();
    }
    private void OnDestroy() => Controls.Disable();

    void Start()
    {
        //Ouchies! Double ouchies! Triple? Yes!
        musicObjects = GameObject.FindGameObjectsWithTag("thisisasound");
        musicObjects = musicObjects.OrderBy(go => go.name).ToArray();
        musicTracks = musicObjects.Select(go => go.GetComponent<AudioSource>()).ToArray();
        //the death of TrackedTween
        activeTweenIDs = new int[musicTracks.Length];
    }

    //kaikki tarpeelline on täs
    void DriftCall(InputAction.CallbackContext context)
    {
        //condition ? true : false
        CurrentMusState = carController.isTurboActive ? CarMusicState.Turbo : CarMusicState.Drift;
        FadeTracks();
    }
    void DriftCanceled(InputAction.CallbackContext context)
    {
        CurrentMusState = carController.isTurboActive ? CarMusicState.Turbo : CarMusicState.Main;
        FadeTracks();
    }
    void TurboCall(InputAction.CallbackContext context)
    {
        CurrentMusState = CarMusicState.Turbo;
        FadeTracks();
    }
    void TurboCanceled(InputAction.CallbackContext context)
    {
        CurrentMusState = carController.isDrifting ? CarMusicState.Drift : CarMusicState.Main;
        FadeTracks();
    }

    private void FadeTracks()
    {
        //tarkistaa staten ku funktio alkaa, ei tarvi muualla
        if (CurrentMusState == LatestMusState) return;



        int stateIndex = (int)CurrentMusState; //current on oikeesti se viimeisin lol
        int previousStateIndex = (int)LatestMusState;
        AudioSource NextTrack = musicTracks[stateIndex];
        AudioSource PreviousTrack = musicTracks[previousStateIndex];

        // Cancel any existing tweens on these tracks
        LeanTween.cancel(activeTweenIDs[stateIndex]);
        LeanTween.cancel(activeTweenIDs[previousStateIndex]);

        // Start new tweens and store their IDs
        activeTweenIDs[stateIndex] =
        LeanTween.value(NextTrack.volume, 0.34f, 1.0f)
            .setOnUpdate(val => NextTrack.volume = val)
            .id;
        activeTweenIDs[previousStateIndex] =
        LeanTween.value(PreviousTrack.volume, 0.0f, 1.0f)
            .setOnUpdate(val => PreviousTrack.volume = val)
            .id;
        
        LatestMusState = CurrentMusState;
    }

    public void StartMusicTracks()
    {
        foreach (AudioSource track in musicTracks)
        {
            track.Play();
        }
    }

    public void ImPauseMenuingIt(InputAction.CallbackContext context)
    {
        //tämä se vasta on erittäin ronny
        PausedMusicHandler();
    }
    public void PausedMusicHandler()
    {
        foreach (AudioSource track in musicTracks)
        {
            //selvä, käytetään YKSI tämmöne if-else. vähä ronny mut se toimii paremmi ku updates
            if (GameManager.instance.isPaused)
                track.Pause();
            else
                track.UnPause();
        }
    }
}