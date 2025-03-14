using UnityEngine;
using UnityEngine.InputSystem;

public class musicVolumeControl : MonoBehaviour
{
    public AudioSource cirno;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    CarInputActions Controls;



    private void Onable()
    {
        Controls.Enable();
    }

    private void Disable()
    {
        Controls.Disable();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cirno = this.GetComponent<AudioSource>();
        cirno.volume = 0.0f;
    }

    private bool isDrifting = false;

    void Update()
    {
        if (isDrifting)
        {
            if (cirno.volume <= 1.0f)
            {
                cirno.volume = Mathf.MoveTowards(cirno.volume, 1.0f, 1.0f * Time.deltaTime);
            }
        }
        else
        {
            if (cirno.volume > 0.0f)
            {
                cirno.volume = Mathf.MoveTowards(cirno.volume, 0.0f, 1.0f * Time.deltaTime);
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
