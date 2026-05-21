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
    private bool musicPlaybackManuallyPaused = false;
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

    public void StartMusicPlayback()
    {
        PlaySong();
        StartCoroutine(SongPlaybackHandler());
    }

    private IEnumerator SongPlaybackHandler()
    {
        Debug.Log($"started playback coroutine");

        //TODO: saaha tää paska toimimaan
        //jos pausettaa pelin, biisi alkaa alusta
        //previous song ei toimi (index out of range)
        //muita mahollisia bugeja liittyen looppaukseen ja shuffleen
        while (currentSong.baseTrack.isPlaying || musicPlaybackManuallyPaused)
        {
            Debug.Log($"song still playing");
            yield return null;
        }

        Debug.Log($"song '{currentSong.name}' ended! switching to next track...");
        NextSong();
        yield return StartCoroutine(SongPlaybackHandler());
        yield break;
    }

    public void PlaySong()
    {
        foreach (AudioSource track in currentSongTracks) track.Play();
        Debug.Log($"started playing song: {currentSong.name}");
        Debug.Log($"tracks for {currentSong.name} are: {currentSong.baseTrack}, {currentSong.driftTrack}, {currentSong.turboTrack}");
    }
    public void PauseSong()
    {
        musicPlaybackManuallyPaused = !musicPlaybackManuallyPaused;
        Debug.Log($"song paused state set to: {musicPlaybackManuallyPaused}");
        foreach (AudioSource track in currentSongTracks)
        {
            if (musicPlaybackManuallyPaused) track.Pause();
            else track.UnPause();
        }
    }
    public void StopSong(bool endRaceEvent = false)
    {
        foreach (AudioSource track in currentSongTracks) track.Stop();
        Debug.Log($"stopped playing song: {currentSong.name}");
        if (endRaceEvent && finalLapTrack != null) finalLapTrack.Stop();
    }
    public void StartFinalLapSong()
    {
        Debug.Log("jos näitä logeja on enemmän ku yks jokin on paskana.");
        Controls.CarControls.Drift.started -= ctx => DriftCall();
        Controls.CarControls.Drift.canceled -= ctx => DriftCanceled();
        Controls.CarControls.turbo.started -= ctx => TurboCall();
        Controls.CarControls.turbo.canceled -= ctx => TurboCanceled();
        StopSong();
        finalLapTrack.Play();
    }

    /* public void ChangeSong(string newSongName)
    {
        StopSong();
        currentSong = songs.First(s => s.name == newSongName);
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        PlaySong();
    } */
    /* public void ChangeSongByIndex(int change)
    {
        int newSongIndex = songs.IndexOf(currentSong);
        StopSong();
        //jos biisi on listan lengthin sisällä, siirry siihen
        //jos se on ulkopuolella (viimesestä ensimmäiseen tai vice versa), mene joko alkuun tai loppuun
        //also btw tää ei ees oo tehty loppuun; ethän käytä thanks
        currentSong = songs[currentSongIndex >= 0 && currentSongIndex < songs.Count ? currentSongIndex + change : (currentSongIndex + change)] ?? currentSong;
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        PlaySong();
    } */
    //dashboard methodit
    public void NextSong()
    {
        int newSongIndex = shuffleSong ? UnityEngine.Random.Range(0, songs.Count) : (songs.IndexOf(currentSong) + 1) % songs.Count;
        currentSong = songs[newSongIndex];
        Debug.Log($"shuffle state: {shuffleSong}");
        SetLoop(loopSong);
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };

        StopSong();
        PlaySong();
        Debug.Log($"to NEXT song: {currentSong.name}; index {newSongIndex}");
    }
    public void PreviousSong()
    {
        int newSongIndex = shuffleSong ? UnityEngine.Random.Range(0, songs.Count) : songs.IndexOf(currentSong) - 1;
        currentSong = songs[newSongIndex < 0 ? songs.Count - 1 : newSongIndex];
        if (newSongIndex < 0) newSongIndex = songs.Count - 1; //jotta newSongIndex ei tee hauskuuksia
        Debug.Log($"shuffle state: {shuffleSong}");
        SetLoop(loopSong);
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };

        StopSong();
        PlaySong();
        Debug.Log($"to PREVIOUS song: {currentSong.name}; index {newSongIndex}");
    }
    /* public void RandomSong()
    {
        StopMusicTracks();
        currentSong = songs[UnityEngine.Random.Range(0, songs.Count)] ?? currentSong;
        currentSongTracks = new AudioSource[] { currentSong.baseTrack, currentSong.driftTrack, currentSong.turboTrack };
        StartMusicTracks();
        Debug.Log($"RANDOM selected song: {currentSong.name}; index {songs[songs.IndexOf(currentSong)]}");
    } */
    public void SetLoop(bool loop)
    {
        loopSong = loop;
        foreach (AudioSource a in currentSongTracks) a.loop = loop;
        Debug.Log($"loop state of current song set to {loop}");
    }

    public void PausedMusicHandler()
    {
        foreach (AudioSource track in currentSongTracks)
        {
            if (GameManager.IsPaused) track.Pause();
            else track.UnPause();
        }
    }
}