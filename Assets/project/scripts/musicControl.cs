using UnityEngine;
using UnityEngine.InputSystem;

public class musicControl : MonoBehaviour
{
    public GameObject[] musicList;
    public AudioSource cirno;
    public AudioSource cirnodrift;

    //drifting control setup
    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    CarInputActions Controls;

    /* private void Onable()
    {
        Controls.Enable();
    }

    private void Disable()
    {
        Controls.Disable();
    } */



    //find the cirnos
    void Start()
    {
        musicList = GameObject.FindGameObjectsWithTag("thisisasound");

        cirnodrift = GameObject.Find("cirnodrift").GetComponent<AudioSource>();
        cirno = GameObject.Find("cirno").GetComponent<AudioSource>();

        foreach (GameObject musicTrack in musicList)
        {
            musicTrack.GetComponent<AudioSource>().Play();
        }
    }



    private bool isDrifting = false;

    void Update()
    {
        if (isDrifting)
        {
            if (cirnodrift.volume <= 1.0f)
            {
                cirnodrift.volume = Mathf.MoveTowards(cirnodrift.volume, 1.0f, 1.0f * Time.deltaTime);
                cirno.volume = Mathf.MoveTowards(cirno.volume, 0.0f, 1.0f * Time.deltaTime);
            }
        }
        else
        {
            if (cirnodrift.volume > 0.0f)
            {
                cirnodrift.volume = Mathf.MoveTowards(cirnodrift.volume, 0.0f, 1.0f * Time.deltaTime);
                cirno.volume = Mathf.MoveTowards(cirno.volume, 1.0f, 1.0f * Time.deltaTime);
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
                musicTrack.GetComponent<AudioSource>().Play();
            }
        }
    }

    void OnEnable()
    {
        Controls.CarControls.Drift.started += OnDriftStarted;
        Controls.CarControls.Drift.canceled += OnDriftCanceled;
    }

    void OnDisable()
    {
        Controls.CarControls.Drift.started -= OnDriftStarted;
        Controls.CarControls.Drift.canceled -= OnDriftCanceled;
    }

    private void OnDriftStarted(InputAction.CallbackContext context)
    {
        isDrifting = true;
    }

    private void OnDriftCanceled(InputAction.CallbackContext context)
    {
        isDrifting = false;
    }
}
