using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class musicControl : MonoBehaviour
{
    //ois pitäny olla private jo alusta alkaen...
    private GameObject[] musicObjects;
    private AudioSource[] musicTracks;
    
    private enum CarMusicState {Main, Drift, Turbo};
    private CarMusicState CurrentMusState = CarMusicState.Main;
    private CarMusicState LatestMusState = CarMusicState.Main;
    private int[] activeTweenIDs;

    //uniikkeja yksittäisiä biisejä, siksi en laita näille tageja tai arrayta.
    //vectoraman demossa se tulee valittemaan kolmen eri biisin välillä
    //ja se vaatii sit enemmän paskaa
    public AudioSource resultsTrack;
    public AudioSource finalLapTrack;

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
    //when
    public void StartFinalLapTrack()
    {
        Debug.Log("jos näitä logeja on enemmän ku yks jokin on paskana.");
        Controls.CarControls.Drift.performed -= DriftCall;
        Controls.CarControls.Drift.canceled -= DriftCanceled;
        Controls.CarControls.turbo.performed -= TurboCall;
        Controls.CarControls.turbo.canceled -= TurboCanceled;
        StopMusicTracks();
        finalLapTrack.Play();
    }
    //when 2
    public void StopMusicTracks(bool endRaceEvent = false, bool stopFinalLap = false)
    {
        foreach (AudioSource track in musicTracks)
        {
            track.Stop();
        }

        if (endRaceEvent || stopFinalLap && finalLapTrack != null)
            finalLapTrack.Stop();
        if (endRaceEvent)
            resultsTrack.Play();
    }

    public void PausedMusicHandler()
    {
        bool isPaused = GameManager.instance.isPaused;
        foreach (AudioSource track in musicTracks)
        {
            if (isPaused)
                track.Pause();
            else
                track.UnPause();
        }
    }
}