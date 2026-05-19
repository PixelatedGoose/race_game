using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Song
{
    public string name;
    public AudioSource baseTrack;
    public AudioSource driftTrack;
    public AudioSource turboTrack;
}
public class MusicManager : MonoBehaviour
{
    [SerializeField] private List<Song> songs;
    private Song currentSong;
    [SerializeField] private AudioSource[] currentSongTracks;
    
    private enum CarMusicState {Main, Drift, Turbo};
    private CarMusicState CurrentMusState = CarMusicState.Main;
    private CarMusicState LatestMusState = CarMusicState.Main;
    public bool shuffleSong;
    public bool loopSong;
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

    //TODO: playlist alkaa eikä vaa yks biisi joka sitte aina vaihetaan
    public IEnumerator BeginSongPlaylist()
    {
        StartMusicTracks();
        
        //tää silti pitää saaha toimimaan...
        while (!currentSong.baseTrack.isPlaying && !GameManager.IsPaused)
        {
            Debug.Log("song ended! switching to next track...");
            NextSong();
            yield return null;
        }
    }

    public void StartMusicTracks()
    {
        if (shuffleSong) currentSong = songs[UnityEngine.Random.Range(0, songs.Count)] ?? currentSong;
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };

        foreach (AudioSource track in currentSongTracks)
        {
            track.loop = loopSong;
            track.Play();
        }
    }
    public void StopMusicTracks(bool endRaceEvent = false)
    {
        foreach (AudioSource track in currentSongTracks) track.Stop();
        if (endRaceEvent && finalLapTrack != null) finalLapTrack.Stop();
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

    /* public void ChangeSong(string newSongName)
    {
        StopMusicTracks();
        currentSong = songs.First(s => s.name == newSongName);
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
    } */
    /* public void ChangeSongByIndex(int change)
    {
        int newSongIndex = songs.IndexOf(currentSong);
        StopMusicTracks();
        //jos biisi on listan lengthin sisällä, siirry siihen
        //jos se on ulkopuolella (viimesestä ensimmäiseen tai vice versa), mene joko alkuun tai loppuun
        //also btw tää ei ees oo tehty loppuun; ethän käytä thanks
        currentSong = songs[currentSongIndex >= 0 && currentSongIndex < songs.Count ? currentSongIndex + change : (currentSongIndex + change)] ?? currentSong;
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        StartMusicTracks();
    } */
    //dashboard methodit
    public void NextSong()
    {
        int newSongIndex = (songs.IndexOf(currentSong) + 1) < songs.Count ? songs.IndexOf(currentSong) + 1 : 0;
        StopMusicTracks();
        currentSong = songs[newSongIndex] ?? currentSong;
        StartMusicTracks();
        Debug.Log($"changed to song: {currentSong.name}");
    }
    public void PreviousSong()
    {
        int newSongIndex = (songs.IndexOf(currentSong) - 1) >= 0 ? songs.IndexOf(currentSong) - 1 : songs.Count - 1;
        StopMusicTracks();
        currentSong = songs[newSongIndex] ?? currentSong;
        StartMusicTracks();
        Debug.Log($"changed to song: {currentSong.name}");
    }
    /* public void RandomSong()
    {
        StopMusicTracks();
        currentSong = songs[UnityEngine.Random.Range(0, songs.Count)] ?? currentSong;
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        StartMusicTracks();
        Debug.Log($"changed to song: {currentSong.name}");
    } */
    public void SetLoop(bool loop)
    {
        loopSong = loop;
        foreach (AudioSource a in currentSongTracks) a.loop = loop;
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