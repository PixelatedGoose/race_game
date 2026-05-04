using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NewDoublefunszechuansauceWithAsideofNuggets : BaseCarController
{
    internal CarInputActions Controls;
    public Material pixelCount;
    RacerScript racerScript;
    LogitechMovement LGM;
    private string CurrentControlScheme;
    PlayerInput PlayerInput;
    [SerializeField] private GameObject carLights;
    private Material carLightsMaterial;

    float DriftDirection;
    float DriftTurnSpeed = 5f;
    [SerializeField] float DriftTurnSpeedAtHighSpeed = 2f;
    float driftMultiplier = 1.2f;
    float SteerDeadzone = 0.2f;



    override protected void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
        CarRb = GetComponent<Rigidbody>();
        TurbeBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();
        carLightsMaterial = GetComponentInChildren<Renderer>().materials[1];
        base.Awake();
        
    }

    private void OnControlsChanged(PlayerInput input)
    {
        CurrentControlScheme = input.currentControlScheme;
        if (LGM != null)
            LGM.ReenableFromControlScheme(CurrentControlScheme);
    }
    void OnAnyActionTriggered(InputAction.CallbackContext ctx)
    {
        var control = ctx.action?.activeControl;
        if (control == null)
            return;
    
    }

    private void OnEnable()
    {
        Controls.Enable();
        if (PlayerInput == null)
            PlayerInput = GetComponent<PlayerInput>();

        if (PlayerInput != null)
            PlayerInput.onControlsChanged += OnControlsChanged;

        Controls.CarControls.Get().actionTriggered += OnAnyActionTriggered;

        Controls.CarControls.Move.performed += OnMovePerformed;
        Controls.CarControls.Move.canceled  += OnMoveCanceled;

        Controls.CarControls.Drift.performed += OnDriftPerformed;
        Controls.CarControls.Drift.canceled  += OnDriftCanceled;
        Controls.CarControls.Brake.performed += OnBrakePerformed;
        Controls.CarControls.Brake.canceled  += OnBrakeCanceled;
    }

    private void OnDisable()
    {
        Controls.Disable();
        if (PlayerInput != null)
            PlayerInput.onControlsChanged -= OnControlsChanged;

        Controls.CarControls.Get().actionTriggered -= OnAnyActionTriggered;


        Controls.CarControls.Move.performed -= OnMovePerformed;
        Controls.CarControls.Move.canceled  -= OnMoveCanceled;
        Controls.CarControls.Drift.performed -= OnDriftPerformed;
        Controls.CarControls.Drift.canceled  -= OnDriftCanceled;
        Controls.CarControls.Brake.performed -= OnBrakePerformed;
        Controls.CarControls.Brake.canceled -= OnBrakeCanceled;
        if (LGM != null)
            LGM.StopAllForceFeedback();
    }

    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();

        if (LGM != null)
            LGM.StopAllForceFeedback();
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        SteerInput = ctx.ReadValue<Vector2>().x;
        MoveInput = ctx.ReadValue<Vector2>().y;
    }
    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        SteerInput = 0f;
        MoveInput = 0f;
    }

    override protected void Start()
    {
        racerScript = FindAnyObjectByType<RacerScript>();
        LGM = FindAnyObjectByType<LogitechMovement>();

        CarRb.centerOfMass = _CenterofMass;

        base.Start();
        
    }

    
    protected void Update()
    {
        Animatewheels();
        GetInputs();
        SteerInput = GetDriftSteer();
        Steer();
        CarMovement();
        ApplyDriftTurn();
        WheelEffects(IsDrifting);
    }

    
    override protected void FixedUpdate()
    {
        Applyturnsensitivity(CarRb.linearVelocity.magnitude);
        Decelerate();

        base.FixedUpdate();
    }


    void GetInputs()
    {
        Vector2 move = Controls.CarControls.Move.ReadValue<Vector2>();
        SteerInput = move.x;
        MoveInput = move.y;
    }

    float GetDriftSteer()
    {
        if (!IsDrifting)
            return SteerInput;

        if (Mathf.Approximately(DriftDirection, 0f))
            DriftDirection = GetSteerSign(SteerInput);


        return DriftDirection;
    }

    void ApplyDriftTurn()
    {
        if (!IsDrifting)
            return;
        float speed = CarRb.linearVelocity.magnitude * 3.6f;
        float speedRatio = Mathf.Clamp01(speed / Mathf.Max(MaxSpeed, 0.1f));
        float driftTurnSpeed = Mathf.Lerp(DriftTurnSpeed, DriftTurnSpeedAtHighSpeed, speedRatio);
        float turn = DriftDirection * driftTurnSpeed * driftMultiplier * Time.deltaTime;
        transform.Rotate(0f, turn, 0f);
    }

    //Arcade car style movement
    protected void CarMovement()
    {
        float forwardValue = Mathf.Abs(MoveInput);
        //this might look like slop, but for now i did this way 
        float direction = MoveInput != 0 ? Mathf.Sign(MoveInput) : Mathf.Sign(Vector3.Dot(CarRb.linearVelocity, transform.forward));
       
        float targetSpeed = Mathf.MoveTowards(CarRb.linearVelocity.magnitude, MaxSpeed  * forwardValue, Acceleration * Time.deltaTime);
        Vector3 flatForwardVelocity = transform.forward * targetSpeed * direction;
        CarRb.linearVelocity = new Vector3(flatForwardVelocity.x, CarRb.linearVelocity.y, flatForwardVelocity.z);
    }


    void Applyturnsensitivity(float speed)
    {
        float speedKph = speed * 3.6f;
        TurnSensitivity = Mathf.Lerp(
            TurnSensitivityAtLowSpeed,
            TurnSensitivityAtHighSpeed,
            Mathf.Clamp01(speedKph / Mathf.Max(MaxSpeed, 0.1f)));
    }

    void OnBrakePerformed(InputAction.CallbackContext ctx)
    {
        foreach (var wheel in Wheels)
        {
            wheel.Brake(BrakeAcceleration);
        }
    }

    void OnBrakeCanceled(InputAction.CallbackContext ctx)
    {
        foreach (var wheel in Wheels)
        {
            wheel.MotorTorque(TargetTorque);
        }
    }


    // your car is drifting bro better go and call your insurance company
    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        IsDrifting = true;
        DriftDirection = 0f;
        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        IsDrifting = false;
        DriftDirection = 0f;
        WheelEffects(false);
    }

    float GetSteerSign(float steer) => Mathf.Abs(steer) > SteerDeadzone ? Mathf.Sign(steer) : 0f;
}