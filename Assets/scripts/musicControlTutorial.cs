using UnityEngine;
using System.Linq;

public class musicControlTutorial : MonoBehaviour
{
    public GameObject[] musicList;
    public AudioSource mainTrack;
    public AudioSource driftTrack;
    public AudioSource turboTrack;

    void Start()
    {
        musicList = GameObject.FindGameObjectsWithTag("musicTrack");
        TrackVariants();

        foreach (GameObject musicTrack in musicList)
        {
            musicTrack.GetComponent<AudioSource>().Play();
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

        if (variants.Count == 0)
        {
            Debug.LogWarning("no variants found; ignore if intended");
        }
        else
        {
            Debug.Log($"Found {variants.Count} variants for prefix {prefix}");
        }

        if (set == true)
        {
            driftTrack = variants[0].GetComponent<AudioSource>();
            turboTrack = variants[1].GetComponent<AudioSource>();
        }
        else
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

    public void MusicSections(string trackName) //lisään myöhemmi oikeet fade outit ja transitionit
    {
        mainTrack.Stop();
        ChangeTrack(trackName);
        mainTrack.Play();
    }

    void Update()
    {
        if (GameManager.instance.turbeActive)
        {
            if (turboTrack != null)
                if (turboTrack.volume <= 0.390f)
                {
                    turboTrack.volume = Mathf.MoveTowards(turboTrack.volume, 0.5f, 1.0f * Time.deltaTime);
                    driftTrack.volume = Mathf.MoveTowards(driftTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                    mainTrack.volume = Mathf.MoveTowards(mainTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                }
        }
        else
        {
            if (driftTrack != null)
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
            
            if (turboTrack != null)
                if (turboTrack.volume > 0.000f)
                {
                    turboTrack.volume = Mathf.MoveTowards(turboTrack.volume, 0.0f, 1.0f * Time.deltaTime);
                    mainTrack.volume = Mathf.MoveTowards(mainTrack.volume, 0.5f, 1.0f * Time.deltaTime);
                }
        }


        if (GameManager.instance.isPaused == true)
        {
            foreach (GameObject musicTrack in musicList)
            {
                musicTrack.GetComponent<AudioSource>().Pause();
            }
        }
        else if (GameManager.instance.isPaused == false && mainTrack.isPlaying == false)
        {
            foreach (GameObject musicTrack in musicList)
            {
                musicTrack.GetComponent<AudioSource>().UnPause();
            }
        }
    }
}