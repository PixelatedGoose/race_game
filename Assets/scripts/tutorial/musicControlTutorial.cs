using UnityEngine;
using System.Linq;
using System.Collections;

public class musicControlTutorial : MonoBehaviour
{
    public GameObject[] musicList;
    public AudioSource[] musicListSources;
    public AudioSource mainTrack;
    public AudioSource driftTrack;
    public AudioSource turboTrack;

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
    void TrackVariants(bool set = false)
    {
        string clipName = mainTrack.clip.name;

        // Get the prefix (e.g. first two characters)
        string prefix = clipName.Substring(0, 1);

        // Find all AudioSources with the same prefix
        var variants = musicList
            .Select(go => go)
            .Where(a => a.name.StartsWith(prefix))
            .ToList();

        if (variants.Count <= 1)
        {
            Debug.LogWarning("no variants found; ignore if intended");
            if (set)
            {
                mainTrack = variants[0].GetComponent<AudioSource>();
            }
        }
        else
        {
            Debug.Log($"Found {variants.Count} variants for prefix {prefix}");
            if (set)
            {
                mainTrack = variants[0].GetComponent<AudioSource>();
                driftTrack = variants[1].GetComponent<AudioSource>();
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
        float volSet = mainTrack.volume;

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

        LeanTween.value(mainTrack.volume, 0.28f, 1.0f).setOnUpdate((float val) =>
        {mainTrack.volume = val;});
        LeanTween.value(previousMain.volume, 0f, 1.0f).setOnUpdate((float val) =>
        {previousMain.volume = val;});

        if (driftTrack != null && previousDrift != null)
        {
            LeanTween.value(driftTrack.volume, 0.28f, 1.0f).setOnUpdate((float val) =>
            {driftTrack.volume = val;});
            LeanTween.value(previousDrift.volume, 0f, 1.0f).setOnUpdate((float val) =>
            {previousDrift.volume = val;});
        }
        if (turboTrack != null && previousTurbo != null)
        {
            LeanTween.value(turboTrack.volume, 0.28f, 1.0f).setOnUpdate((float val) =>
            {turboTrack.volume = val;});
            LeanTween.value(previousTurbo.volume, 0f, 1.0f).setOnUpdate((float val) =>
            {previousTurbo.volume = val;});
        }
    }

    void Update()
    {
        if (GameManager.instance.turbeActive)
        {
            if (turboTrack != null)
            {
                if (turboTrack.volume <= 0.390f)
                {
                    turboTrack.volume = Mathf.MoveTowards(turboTrack.volume, 0.5f, 1.0f * Time.deltaTime);
                    driftTrack.volume = Mathf.MoveTowards(driftTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                    mainTrack.volume = Mathf.MoveTowards(mainTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                }
            }
        }
        else
        {
            if (driftTrack != null)
            {
                if (GameManager.instance.isAddingPoints)
                {
                    if (driftTrack.volume <= 0.390f)
                    {
                        driftTrack.volume = Mathf.MoveTowards(driftTrack.volume, 0.5f, 1.0f * Time.deltaTime);
                        mainTrack.volume = Mathf.MoveTowards(mainTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                    }
                }
                else
                {
                    if (driftTrack.volume > 0.000f)
                    {
                        driftTrack.volume = Mathf.MoveTowards(driftTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                        mainTrack.volume = Mathf.MoveTowards(mainTrack.volume, 0.5f, 1.0f * Time.deltaTime);
                    }
                }
            }

            if (turboTrack != null)
            {
                if (turboTrack.volume > 0.000f)
                {
                    turboTrack.volume = Mathf.MoveTowards(turboTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                    mainTrack.volume = Mathf.MoveTowards(mainTrack.volume, 0.5f, 1.0f * Time.deltaTime);
                }
            }
        }



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
}