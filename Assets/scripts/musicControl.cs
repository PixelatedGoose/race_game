using UnityEngine;

public class musicControl : MonoBehaviour
{
    public GameObject[] musicList;
    public AudioSource cirno;
    public AudioSource cirnodrift;
    public AudioSource cirnoturbo;

    //find the cirnos
    void Start()
    {
        musicList = GameObject.FindGameObjectsWithTag("thisisasound");

        cirnoturbo = GameObject.Find("cirnoturbo").GetComponent<AudioSource>();
        cirnodrift = GameObject.Find("cirnodrift").GetComponent<AudioSource>();
        cirno = GameObject.Find("cirno").GetComponent<AudioSource>();

        foreach (GameObject musicTrack in musicList)
        {
            musicTrack.GetComponent<AudioSource>().Play();
        }
    }

    void Update()
    {
        if (GameManager.instance.turbeActive)
        {
            if (cirnoturbo.volume <= 0.390f)
            {
                cirnoturbo.volume = Mathf.MoveTowards(cirnoturbo.volume, 0.5f, 1.0f * Time.deltaTime);
                cirnodrift.volume = Mathf.MoveTowards(cirnodrift.volume, 0.0f, 1.0f * Time.deltaTime);
                cirno.volume = Mathf.MoveTowards(cirno.volume, 0.0f, 1.0f * Time.deltaTime);
            }
        }
        else
        {
            if (GameManager.instance.isAddingPoints)
            {
                if (cirnodrift.volume <= 0.390f)
                {
                    cirnodrift.volume = Mathf.MoveTowards(cirnodrift.volume, 0.5f, 1.0f * Time.deltaTime);
                    cirno.volume = Mathf.MoveTowards(cirno.volume, 0.0f, 1.0f * Time.deltaTime);
                }
            }
            else
            {
                if (cirnodrift.volume > 0.000f)
                {
                    cirnodrift.volume = Mathf.MoveTowards(cirnodrift.volume, 0.0f, 1.0f * Time.deltaTime);
                    cirno.volume = Mathf.MoveTowards(cirno.volume, 0.5f, 1.0f * Time.deltaTime);
                }
            }
            
            if (cirnoturbo.volume > 0.000f)
            {
                cirnoturbo.volume = Mathf.MoveTowards(cirnoturbo.volume, 0.0f, 1.0f * Time.deltaTime);
                cirno.volume = Mathf.MoveTowards(cirno.volume, 0.5f, 1.0f * Time.deltaTime);
            }
        }


        if (GameManager.instance.isPaused == true)
        {
            foreach (GameObject musicTrack in musicList)
            {
                musicTrack.GetComponent<AudioSource>().Pause();
            }
        }
        else if (GameManager.instance.isPaused == false && cirno.isPlaying == false)
        {
            foreach (GameObject musicTrack in musicList)
            {
                musicTrack.GetComponent<AudioSource>().UnPause();
            }
        }
    }
}