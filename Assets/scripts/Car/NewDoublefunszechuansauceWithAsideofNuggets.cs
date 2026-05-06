using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class NewDoublefunszechuansauceWithAsideofNuggets : BaseCarController
{
    public CarInputActions Controls { get; protected set; }
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
    float CurrentDriftAngle = 0f;

    float RightDriftAngle = 45f;
    float LeftDriftAngle = -45f;




    override protected void Awake()
    {
        Controls = new CarInputActions();
        CarRb = GetComponent<Rigidbody>();
        TurbeBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();
        racerScript = GetComponent<RacerScript>();
        TryGetComponent(out LGM);
        
        Controls.Enable();
        base.Awake();

        CarRb.centerOfMass = _CenterofMass;

        if (turbo != null)
        {
            Controls.CarControls.turbo.started += context => { turbo.Activate(); };
            Controls.CarControls.turbo.performed += context => { turbo.Stop(); };
        }
    }

    private void OnControlsChanged(PlayerInput input)
    {
        CurrentControlScheme = input.currentControlScheme;
        if (LGM != null) LGM.ReenableFromControlScheme(CurrentControlScheme);
    }

    void OnAnyActionTriggered(InputAction.CallbackContext ctx)
    {
        var control = ctx.action?.activeControl;
        if (control == null) return;

    }

    private void OnEnable()
    {
        // Add input event listeners
        Controls.Enable();
        PlayerInput = GetComponent<PlayerInput>();

        PlayerInput.onControlsChanged += OnControlsChanged;

        Controls.CarControls.Get().actionTriggered += OnAnyActionTriggered;

        Controls.CarControls.Move.performed += OnMovePerformed;
        Controls.CarControls.Move.canceled  += OnMoveCanceled;

        Controls.CarControls.Drift.performed += OnDriftPerformed;
        Controls.CarControls.Drift.canceled  += OnDriftCanceled;
        Controls.CarControls.Brake.performed += OnBrakePerformed;
        Controls.CarControls.Brake.canceled  += OnBrakeCanceled;

        if (turbo != null)
        {
            Controls.CarControls.turbo.started += context => { turbo.Activate(); };
            Controls.CarControls.turbo.performed += context => { turbo.Stop(); };
        }
    }

    private void OnDisable()
    {
        // Remove input event listeners
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

        if (turbo != null)
        {
            Controls.CarControls.turbo.started -= context => { turbo.Activate(); };
            Controls.CarControls.turbo.performed -= context => { turbo.Stop(); };
        }

        if (LGM != null) LGM.StopAllForceFeedback();
    }

    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();

        if (LGM != null) LGM.StopAllForceFeedback();
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        MovementInputs = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        MovementInputs = Vector3.zero;
    }

    override protected void Start()
    {
        base.Start();
        
    }

    
    protected void Update()
    {
        MovementInputs = Controls.CarControls.Move.ReadValue<Vector2>();
        Animatewheels();

        Steer();
        CarMovement();
        ApplyDriftTurn();
        WheelEffects(IsDrifting);
    }

    //physics related will go here
    override protected void FixedUpdate()
    {
        TurnSensitivity = CarRb.linearVelocity.magnitude / MaxSpeed * turnSensitivityRange + MaxTurnSensitivity;
        base.FixedUpdate();
        // HandleTurbo();
    }

    //que
//    protected void HandleTurbo()
//     {
//         if (!CanUseTurbo) return;
//         Turbe.TURBO(this);
//         TurbeMeter();
//     }

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
        
        float speedRatio = Mathf.Clamp01(CarRb.linearVelocity.magnitude / Mathf.Max(MaxSpeed, 0.01f));
        float turnSpeed = Mathf.Lerp(DriftTurnSpeed, DriftTurnSpeedAtHighSpeed, speedRatio);

        float deltaTurn = DriftDirection * turnSpeed * driftMultiplier * Time.deltaTime;

        float newAngle = Mathf.Clamp(CurrentDriftAngle + deltaTurn, LeftDriftAngle, RightDriftAngle);
        float appliedTurn = newAngle - CurrentDriftAngle;

        CurrentDriftAngle = newAngle;

        transform.Rotate(0f, appliedTurn, 0f);
    }


    //Arcade car style movement
    protected void CarMovement()
    {
        Vector3 flatForwardVelocity = 
        transform.forward * Mathf.MoveTowards(
            CarRb.linearVelocity.magnitude,
            MaxSpeed * Mathf.Abs(MovementInputs.y), 
            Acceleration * Time.deltaTime
        );
        flatForwardVelocity.y = CarRb.linearVelocity.y;
        CarRb.linearVelocity = flatForwardVelocity;
    }

    void OnBrakePerformed(InputAction.CallbackContext ctx)
    {
        foreach (Wheel wheel in Wheels)
        {
            wheel.Brake(BrakeAcceleration);
        }
    }

    void OnBrakeCanceled(InputAction.CallbackContext ctx)
    {
        foreach (Wheel wheel in Wheels)
        {
            wheel.SetTorque(TargetTorque);
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