using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class musicControlTutorial : MonoBehaviour
{
    CarInputActions Controls;
    public GameObject[] musicList;
    public AudioSource[] musicListSources;
    public AudioSource mainTrack;
    public AudioSource driftTrack;
    public AudioSource turboTrack;
    private List<GameObject> variants;

    private List<int> tweenIds = new List<int>();
    private enum MusicState { Main, Drift, Turbo }
    private MusicState currentState = MusicState.Main;

    private CarController carController;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        carController = FindAnyObjectByType<CarController>();
    }
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

    void DriftCall(InputAction.CallbackContext context)
    {
        if (currentState == MusicState.Turbo || variants.Count <= 1) //VOI VITTU IHAN OIKEASTI
            return;

        CancelTweens();
        if (GameManager.instance.isAddingPoints)
        {
            TrackedTween_Start(mainTrack.volume, 0.0f, 0.6f, val => mainTrack.volume = val);
            TrackedTween_Start(driftTrack.volume, 0.28f, 0.6f, val => driftTrack.volume = val);

            if (turboTrack != null)
                TrackedTween_Start(turboTrack.volume, 0.0f, 0.6f, val => turboTrack.volume = val);

            currentState = MusicState.Drift;
        }
    }
    void DriftCanceled(InputAction.CallbackContext context)
    {
        if (currentState == MusicState.Turbo || variants.Count <= 1)
            return;

        if (!GameManager.instance.isAddingPoints) //turha mut nyt ei oteta riskejä lol
        {
            TrackedTween_Start(mainTrack.volume, 0.28f, 0.6f, val => mainTrack.volume = val);
            TrackedTween_Start(driftTrack.volume, 0.0f, 0.6f, val => driftTrack.volume = val);

            if (turboTrack != null)
                TrackedTween_Start(turboTrack.volume, 0.0f, 0.6f, val => turboTrack.volume = val);

            currentState = MusicState.Main;
        }
    }

    void TurboCall(InputAction.CallbackContext context)
    {
        if (variants.Count < 3) //koska näit voi olla ainoastaa kolme kun voi käyttää turboa
            return;

        CancelTweens();
        TrackedTween_Start(turboTrack.volume, 0.28f, 0.6f, val => turboTrack.volume = val);
        TrackedTween_Start(driftTrack.volume, 0.0f, 0.6f, val => driftTrack.volume = val);
        TrackedTween_Start(mainTrack.volume, 0.0f, 0.6f, val => mainTrack.volume = val);

        currentState = MusicState.Turbo;
    }
    void TurboCanceled(InputAction.CallbackContext context)
    {
        if (variants.Count < 3)
            return;

        if (GameManager.instance.isAddingPoints)
        {
            TrackedTween_Start(driftTrack.volume, 0.28f, 0.6f, val => driftTrack.volume = val);
            TrackedTween_Start(mainTrack.volume, 0.0f, 0.6f, val => mainTrack.volume = val);
            TrackedTween_Start(turboTrack.volume, 0.0f, 0.6f, val => turboTrack.volume = val);

            currentState = MusicState.Drift;
        }
        else
        {
            TrackedTween_Start(mainTrack.volume, 0.28f, 0.6f, val => mainTrack.volume = val);
            TrackedTween_Start(driftTrack.volume, 0.0f, 0.6f, val => driftTrack.volume = val);
            TrackedTween_Start(turboTrack.volume, 0.0f, 0.6f, val => turboTrack.volume = val);

            currentState = MusicState.Main;
        }
    }
    void OnDisable()
    {
        Controls.Disable();
        Controls.CarControls.Drift.performed -= DriftCall;
        Controls.CarControls.turbo.performed -= TurboCall;
        Controls.CarControls.Drift.canceled -= DriftCanceled;
        Controls.CarControls.turbo.canceled -= TurboCanceled;
    }

    void Start()
    {
        musicList = GameObject.FindGameObjectsWithTag("musicTrack");
        musicListSources = musicList.Select(go => go.GetComponent<AudioSource>()).ToArray();
        TrackVariants();
    }

    //"track 2" eli se joka alkaa, ku menee ekasta triggeristä läpi
    //on se, mistä kaikkien muitten pitäs referoida alkukohta.
    //joudun tän lisäksi tekee tän vielä uudestaa driftin alotustrackkia varten,
    //mut se sit joudutaa tekee just ja just enne vikoja päiviä. helppo juttu lol
    //(famous last words)
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

        // Find all AudioSources with the same prefix
        variants = musicList
            .Select(go => go)
            .Where(a => a.name.StartsWith(prefix))
            .OrderBy(a => a.name)
            .ToList();

        if (variants.Count == 1)
        {
            Debug.LogWarning("no variants found; ignore if intended");
            if (set)
            {
                mainTrack = variants[0].GetComponent<AudioSource>();
            }
        }
        //lazy
        else if (variants.Count == 2)
        {
            Debug.Log($"Found {variants.Count} variants for prefix {prefix}");
            if (set)
            {
                mainTrack = variants[0].GetComponent<AudioSource>();
                if (variants[1] != null)
                    driftTrack = variants[1].GetComponent<AudioSource>();
            }
        }
        else if (variants.Count == 3)
        {
            Debug.Log($"Found {variants.Count} variants for prefix {prefix}");
            if (set)
            {
                mainTrack = variants[0].GetComponent<AudioSource>();
                if (variants[1] != null)
                    driftTrack = variants[1].GetComponent<AudioSource>();
                if (variants[2] != null)
                    turboTrack = variants[2].GetComponent<AudioSource>();
            }
        }

        if (set == false)
        {
            Debug.Log("assuming track has no variants; removing", mainTrack);
            driftTrack = null;
            turboTrack = null;
        }
    }

    void CancelTweens()
    {
        foreach (var tweenId in tweenIds)
        {
            LeanTween.cancel(tweenId);
        }
        tweenIds.Clear();
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
        CancelTweens();

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
            case "fade":
                FadeSections(trackName);
                break;
        }
    }

    private void FadeSections(string trackName)
    {
        AudioSource previousMain = mainTrack;
        AudioSource previousDrift = driftTrack;
        AudioSource previousTurbo = turboTrack;

        ChangeTrack(trackName);

        TrackedTween_Start(mainTrack.volume, 0.28f, 0.6f, val => mainTrack.volume = val);
        TrackedTween_Start(previousMain.volume, 0.0f, 0.6f, val => previousMain.volume = val);

        //TODO: rewrite tälle jotta tätä PASKAA ei tarvita.
        //liian monimutkanen, varsinki verrattuna nykyseen musiikkisysteemii
        if (driftTrack != null && carController.isDrifting)
            TrackedTween_Start(driftTrack.volume, 0.28f, 0.6f, val => driftTrack.volume = val);
        if (previousDrift != null)
            TrackedTween_Start(previousDrift.volume, 0.0f, 0.6f, val => previousDrift.volume = val);
        if (turboTrack != null && GameManager.instance.turbeActive)
            TrackedTween_Start(turboTrack.volume, 0.28f, 0.6f, val => turboTrack.volume = val);
        if (previousTurbo != null)
            TrackedTween_Start(previousTurbo.volume, 0.0f, 0.6f, val => previousTurbo.volume = val);
    }



    void Update()
    {
        if (GameManager.instance.isPaused == true)
        {
            foreach (AudioSource musicTrack in musicListSources)
            {
                musicTrack.Pause();
            }
        }
        else if (GameManager.instance.isPaused == false && mainTrack.isPlaying == false)
        {
            foreach (AudioSource musicTrack in musicListSources)
            {
                musicTrack.UnPause();
            }
        }
    }

    public int TrackedTween_Start(float from, float to, float time, System.Action<float> onUpdate, bool yeah = false)
    {
        int tweenId;

        if (yeah)
        {
            tweenId = LeanTween.value(from, to, time).setOnUpdate(onUpdate)
            .setOnComplete(() =>
            {
                mainTrack.volume = 0f;
            }).uniqueId;
        }
        else
        {
            tweenId = LeanTween.value(from, to, time).setOnUpdate(onUpdate).uniqueId;
        }

        tweenIds.Add(tweenId);
        return tweenId;
    }

    //jotta fuckshitter.cs ei callaa tätä scriptii 5 kertaa
    public void BeginDriftSection()
    {
        mainTrack.volume = 0f;
        StopNonIntroTracks();
        MusicSections("6_FINAL_TUTORIAL_1main");
        StartNonIntroTracks();
    }

    public IEnumerator End()
    {
        SceneManager.LoadSceneAsync(0);
        yield return null;
    }
}