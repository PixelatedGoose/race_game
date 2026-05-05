using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Song
{
    public string name;
    public AudioSource baseTrack;
    public AudioSource driftTrack;
    public AudioSource turboTrack;
    public float defaultVolume;
}
public class MusicManager : MonoBehaviour
{
    [SerializeField] private List<Song> songs;
    private Song currentSong;
    [SerializeField] private AudioSource[] currentSongTracks;
    
    private enum CarMusicState {Main, Drift, Turbo};
    private CarMusicState CurrentMusState = CarMusicState.Main;
    private CarMusicState LatestMusState = CarMusicState.Main;
    private int[] activeTweenIDs;

    //uniikkeja yksittäisiä biisejä, siksi en laita näille tageja tai arrayta
    public AudioSource resultsTrack;
    public AudioSource finalLapTrack;

    private PlayerCarController carController;
    CarInputActions Controls;

    void Awake()
    {
        //default; tulevaisuudes tallentaa playerprefsiin jotta muistaa?
        currentSong = songs[0];
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        Controls = new CarInputActions();
        carController = FindAnyObjectByType<PlayerCarController>();

        Controls.CarControls.Drift.started += ctx => DriftCall();
        Controls.CarControls.Drift.canceled += ctx => DriftCanceled();
        Controls.CarControls.turbo.started += ctx => TurboCall();
        Controls.CarControls.turbo.canceled += ctx => TurboCanceled();
    }
    private void OnEnable() => Controls.Enable();
    private void OnDisable() => DisableEventsAndControls();
    private void OnDestroy() => DisableEventsAndControls();
    private void DisableEventsAndControls()
    {
        LeanTween.cancelAll();
        Controls.CarControls.Drift.started -= ctx => DriftCall();
        Controls.CarControls.Drift.canceled -= ctx => DriftCanceled();
        Controls.CarControls.turbo.started -= ctx => TurboCall();
        Controls.CarControls.turbo.canceled -= ctx => TurboCanceled();
        Controls.Disable();
    }

    void Start()
    {
        activeTweenIDs = new int[currentSongTracks.Length];
    }

    void DriftCall()
    {
        CurrentMusState = carController.IsTurboActive ? CarMusicState.Turbo : CarMusicState.Drift;
        FadeTracks();
    }
    void DriftCanceled()
    {
        CurrentMusState = carController.IsTurboActive ? CarMusicState.Turbo : CarMusicState.Main;
        FadeTracks();
    }
    void TurboCall()
    {
        CurrentMusState = CarMusicState.Turbo;
        FadeTracks();
    }
    void TurboCanceled()
    {
        CurrentMusState = carController.IsDrifting ? CarMusicState.Drift : CarMusicState.Main;
        FadeTracks();
    }

    private void FadeTracks()
    {
        if (CurrentMusState == LatestMusState) return;

        int stateIndex = (int)CurrentMusState; //current on oikeesti se viimeisin lol
        int previousStateIndex = (int)LatestMusState;
        AudioSource NextTrack = currentSongTracks[stateIndex];
        AudioSource PreviousTrack = currentSongTracks[previousStateIndex];

        LeanTween.cancel(activeTweenIDs[stateIndex]);
        LeanTween.cancel(activeTweenIDs[previousStateIndex]);
        activeTweenIDs[stateIndex] = LeanTween.value(NextTrack.volume, 0.3f, 0.7f).setOnUpdate(val => NextTrack.volume = val).id;
        activeTweenIDs[previousStateIndex] = LeanTween.value(PreviousTrack.volume, 0.0f, 0.7f).setOnUpdate(val => PreviousTrack.volume = val).id;
        LatestMusState = CurrentMusState;

        //ihan VITUN PASKANEN hackki, joka tarkistaa että onko lopullinen music state ees oikea
        //jostain syystä IsTurboActive ei halua toimia samalla framella music staten kanssa, toisin kuin drift...
        if (CurrentMusState == CarMusicState.Turbo && !Controls.CarControls.turbo.IsPressed())
        {
            if (carController.IsDrifting) CurrentMusState = CarMusicState.Drift;
            else CurrentMusState = CarMusicState.Main;
            FadeTracks();
        }
    }

    public void StartMusicTracks()
    {
        foreach (AudioSource track in currentSongTracks) track.Play();
    }
    public void StartFinalLapTrack()
    {
        Debug.Log("jos näitä logeja on enemmän ku yks jokin on paskana.");
        Controls.CarControls.Drift.started -= ctx => DriftCall();
        Controls.CarControls.Drift.canceled -= ctx => DriftCanceled();
        Controls.CarControls.turbo.started -= ctx => TurboCall();
        Controls.CarControls.turbo.canceled -= ctx => TurboCanceled();
        StopMusicTracks();
        finalLapTrack.Play();
    }
    public void StopMusicTracks(bool endRaceEvent = false, bool stopFinalLap = false)
    {
        foreach (AudioSource track in currentSongTracks) track.Stop();
        if (endRaceEvent || stopFinalLap && finalLapTrack != null) finalLapTrack.Stop();
    }

    //dashboard methodit
    /* public void ChangeSong(string newSongName)
    {
        StopMusicTracks();
        currentSong = songs.First(s => s.name == newSongName);
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
    } */
    public void NextSong()
    {
        StopMusicTracks();
        currentSong = songs[songs.IndexOf(currentSong) + 1] ?? currentSong;
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        StartMusicTracks();
    }
    public void PreviousSong()
    {
        StopMusicTracks();
        currentSong = songs[songs.IndexOf(currentSong) - 1] ?? currentSong;
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        StartMusicTracks();
    }
    public void RandomSong()
    {
        StopMusicTracks();
        StartMusicTracks();
    }

    public void PausedMusicHandler()
    {
        bool isPaused = GameManager.IsPaused;
        foreach (AudioSource track in currentSongTracks)
        {
            if (isPaused) track.Pause();
            else track.UnPause();
        }
    }
}