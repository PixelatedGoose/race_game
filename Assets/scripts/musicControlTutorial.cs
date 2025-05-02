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
        musicList = GameObject.FindGameObjectsWithTag("thisisasound");
        GetTrackVariants();

        foreach (GameObject musicTrack in musicList)
        {
            /* musicTrack.GetComponent<AudioSource>().Play(); */
        }
    }

    void GetTrackVariants()
    {
        string clipName = mainTrack.clip.name;

        // Get the prefix (e.g. first two characters)
        string prefix = clipName.Substring(0, 1);

        // Find all AudioSources with the same prefix
        var variants = musicList
            .Select(go => go)
            .Where(a => a.name.StartsWith(prefix))
            .ToList();

        Debug.Log($"Found {variants.Count} variants for prefix {prefix}");
    }

    void ChangeTrack(string selectedAudio)
    {
        mainTrack = GameObject.Find(selectedAudio).GetComponent<AudioSource>();
        GetTrackVariants();
    }
    
    // part 1:
        // ChangeTrack("1_intro_MAINloop")
    // part 2:
        // ChangeTrack("2_driving_MAINloop")

    // jne...

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