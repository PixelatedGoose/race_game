using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logitech;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class NewDoublefunszechuansauceWithAsideofNuggets : BaseCarController
{
    public CarInputActions Controls { get; protected set; }

    LogitechMovement LGM;
    PlayerInput PlayerInput;
    
    string CurrentControlScheme;

    // Logitech input timing (copied from PlayerCarController)
    internal float LastNonWheelInputTime = 0f;
    internal float LastWheelInputTime = 0f;

    [Header("Visuals")]
    [SerializeField] private GameObject carLights;
    [SerializeField] private Material PixelCount;

    [Header("Steering")]
    [SerializeField] private float steerDeadzone = 0.15f;

    [Header(" Drift")]
    [SerializeField] private float driftTurnStrength = 12f;
    [SerializeField] private float driftSidewaysDamping = 0.95f;
    [SerializeField] private float driftForwardBoost = 1.01f;
    [SerializeField] private float driftRotationSpeed = 10f;
    [SerializeField] private float driftOutwardForce = 10f;
    [SerializeField] private float counterSteerStrength = 0.6f;
    [SerializeField] private float driftSteerMultiplier = 0.22f;
    [SerializeField] private float driftMinSpeed = 8f;
    [SerializeField] private float driftTightenTurnBoost = 1.15f;
    [SerializeField] private float driftTightenDamping = 0.9f;
    [SerializeField] private float driftWidenDamping = 0.98f;
    [SerializeField] private float driftHighSpeedTurnBoost = 1.25f;
    [SerializeField] private float driftHighSpeedOutwardBoost = 1.15f;

    [Header("Grounding")]
    [SerializeField] private int minGroundedWheelsForDrive = 2;

    float basePixel;
    float minPixel = 32f;

    float recoverTime = 2f;

    Coroutine PixelRecovery;

    float driftDirectionLocked;

    protected override void Awake()
    {
        Controls = new CarInputActions();

        CarRb = GetComponent<Rigidbody>();

        TurbeBar = GameManager.instance.CarUI
            .transform.Find("TurbeDisplay")
            .GetComponentInChildren<Image>();

        TryGetComponent(out LGM);

        Controls.Enable();

        base.Awake();

        CarRb.centerOfMass = _CenterofMass;

        if (turbo != null)
        {
            Controls.CarControls.turbo.started += _ => turbo.Activate();
            Controls.CarControls.turbo.canceled += _ => turbo.Stop();
        }
    }

    protected override void Start()
    {
        TryGetComponent(out LGM);
        if (LGM != null) LGM.InitializeLogitechWheel(); 
        base.Start();

        basePixel = PixelCount.GetFloat("_pixelcount");
    }

    private void OnEnable()
    {
        Controls.Enable();

        PlayerInput = GetComponent<PlayerInput>();

        PlayerInput.onControlsChanged += OnControlsChanged;

        // mirror PlayerCarController: track any input action so wheel auto-enable and FF can be managed
        Controls.CarControls.Get().actionTriggered += OnAnyActionTriggered;

        Controls.CarControls.Move.performed += OnMovePerformed;
        Controls.CarControls.Move.canceled += OnMoveCanceled;

        Controls.CarControls.Drift.performed += OnDriftPerformed;
        Controls.CarControls.Drift.canceled += OnDriftCanceled;

        Controls.CarControls.Brake.performed += OnBrakePerformed;
        Controls.CarControls.Brake.canceled += OnBrakeCanceled;
    }

    private void OnDisable()
    {
        Controls.Disable();

        if (PlayerInput != null)
            PlayerInput.onControlsChanged -= OnControlsChanged;

        Controls.CarControls.Move.performed -= OnMovePerformed;
        Controls.CarControls.Move.canceled -= OnMoveCanceled;

        Controls.CarControls.Drift.performed -= OnDriftPerformed;
        Controls.CarControls.Drift.canceled -= OnDriftCanceled;

        Controls.CarControls.Brake.performed -= OnBrakePerformed;
        Controls.CarControls.Brake.canceled -= OnBrakeCanceled;

        Controls.CarControls.Get().actionTriggered -= OnAnyActionTriggered;

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

        var device = control.device;
        if (device is Keyboard || device is Mouse)
            CurrentControlScheme = "Keyboard";
        else if (device is Gamepad)
            CurrentControlScheme = "Gamepad";


        if (LGM != null)
        {
            LGM.allowAutoEnable = true;
            LGM.StopAllForceFeedback();
        }
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        MovementInputs = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        MovementInputs = Vector2.zero;
    }

    protected void Update()
    {
        MovementInputs = Controls.CarControls.Move.ReadValue<Vector2>();

        MovementInputs.x = ApplySteerDeadzone(MovementInputs.x);

        Animatewheels();

        Steer();

        if (GetGroundedWheelCount() >= minGroundedWheelsForDrive)
        {
            CarMovement();
        }

        ApplySpeedLimit();

        Decelerate();

        if (LGM != null && LGM.useLogitechWheel)
        {
            LGM.allowAutoEnable = true;
            LogitechGSDK.LogiUpdate();
            LGM.GetLogitechInputs();
            LGM.ApplyForceFeedback();  
        }
    }

    protected override void FixedUpdate()
    {
        TurnSensitivity = CarRb.linearVelocity.magnitude / MaxSpeed * turnSensitivityRange + MaxTurnSensitivity;

        if (IsDrifting)
        {
            DriftPhysics();
        }

        base.FixedUpdate();
    }

    int GetGroundedWheelCount()
    {
        int grounded = 0;

        foreach (Wheel wheel in Wheels)
        {
            if (wheel?.WheelCollider == null)
                continue;

            if (wheel.IsGrounded())
                grounded++;
        }

        return grounded;
    }


    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            if (LGM != null && LGM.logitechInitialized && LogitechGSDK.LogiIsConnected(0))
            {
                LogitechGSDK.LogiUpdate();
            }
        }
    }

    protected void CarMovement()
    {
        Vector3 forward = transform.forward;

        float currentSign = Mathf.Abs(Vector3.Dot(CarRb.linearVelocity, forward)) > 0.1f ? Mathf.Sign(Vector3.Dot(CarRb.linearVelocity, forward)) : Mathf.Sign(MovementInputs.y);
        float signedSpeed = CarRb.linearVelocity.magnitude * currentSign;
        float forwardSpeed = Mathf.MoveTowards(
            signedSpeed,
            MaxSpeed * MovementInputs.y,
            Acceleration * Time.deltaTime
        );

        Vector3 horiz = forward * forwardSpeed;
            if (IsDrifting)
            {
                if (!Mathf.Approximately(driftDirectionLocked, 0f))
                {
                    float absInput = Mathf.Abs(MovementInputs.x);
                    if (absInput > steerDeadzone)
                    {
                        bool counterSteering = Mathf.Sign(MovementInputs.x) != driftDirectionLocked;
                        float scale = counterSteering ? counterSteerStrength : 1f;
                        MovementInputs.x = driftDirectionLocked * absInput * scale;
                    }
                }

                MovementInputs.x *= driftSteerMultiplier;
            }
        CarRb.linearVelocity = new Vector3(horiz.x, CarRb.linearVelocity.y, horiz.z);
    }





    void DriftPhysics()
    {
        Vector3 flatVelocity = Vector3.ProjectOnPlane( CarRb.linearVelocity, Vector3.up);

        if (flatVelocity.magnitude < driftMinSpeed) return;

        if (Mathf.Approximately(driftDirectionLocked, 0f))
        {
            if (Mathf.Abs(MovementInputs.x) <= steerDeadzone)
                return;

            driftDirectionLocked = Mathf.Sign(MovementInputs.x);
        }

        float driftDir = driftDirectionLocked;
        bool counterSteering = Mathf.Abs(MovementInputs.x) > steerDeadzone && Mathf.Sign(MovementInputs.x) != driftDir;

        Vector3 forward = transform.forward * Vector3.Dot(flatVelocity, transform.forward);

        float damping = driftSidewaysDamping;
        if (counterSteering)
            damping = Mathf.Lerp(driftSidewaysDamping, driftWidenDamping, 0.75f);
        else
            damping = Mathf.Lerp(driftSidewaysDamping, driftTightenDamping, 0.75f);

        Vector3 sideways = transform.right * Vector3.Dot(flatVelocity, transform.right) * damping;

        CarRb.linearVelocity = new Vector3(forward.x + sideways.x,  CarRb.linearVelocity.y, forward.z + sideways.z) * driftForwardBoost;

        float speed01 = Mathf.Clamp01(flatVelocity.magnitude / Mathf.Max(0.1f, MaxSpeed));
        float speedTurnBoost = Mathf.Lerp(1f, driftHighSpeedTurnBoost, speed01);
        float speedOutwardBoost = Mathf.Lerp(1f, driftHighSpeedOutwardBoost, speed01);

        float turnScale = counterSteering ? counterSteerStrength : driftTightenTurnBoost;
        float turnForce = driftTurnStrength * turnScale * speedTurnBoost;

        CarRb.AddTorque(Vector3.up * driftDir * turnForce, ForceMode.Acceleration);

        float outwardScale = counterSteering ? counterSteerStrength : 1f;
        CarRb.AddForce(transform.right * driftDir * driftOutwardForce * outwardScale * speedOutwardBoost, ForceMode.Acceleration);

        flatVelocity = Vector3.ProjectOnPlane(CarRb.linearVelocity, Vector3.up);

        if (flatVelocity.sqrMagnitude < 0.5f) return;

        CarRb.MoveRotation(Quaternion.Slerp(CarRb.rotation, Quaternion.LookRotation(flatVelocity.normalized), driftRotationSpeed * Time.fixedDeltaTime));
    }

    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        if (IsDrifting)
            return;

        if (CarRb.linearVelocity.magnitude < driftMinSpeed)
            return;

        IsDrifting = true;

        driftDirectionLocked = 0f;

        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        EndDrift();
    }

    void EndDrift()
    {
        IsDrifting = false;

        driftDirectionLocked = 0f;

        WheelEffects(false);
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

    float ApplySteerDeadzone(float steer)
    {
        return Mathf.Abs(steer) < steerDeadzone ? 0f : steer;
    }

    public float GetDriftSharpness()
    {
        if (!IsDrifting)
            return 0f;

        Vector3 flatVelocity = CarRb.linearVelocity;
        flatVelocity.y = 0f;

        if (flatVelocity.sqrMagnitude < 0.1f)
            return 0f;

        return Vector3.Angle(transform.forward, flatVelocity.normalized);
    }

    // void OnCollisionEnter(Collision collision)
    // {
    //     if (PixelCount == null  || collision.impulse.magnitude < 0.1f ) 
    //         return;

    //     float impact = Mathf.Clamp01(collision.relativeVelocity.magnitude / Mathf.Max(MpsMaxSpeed, 0.01f));

    //     impact = Mathf.SmoothStep(0f, 1f, impact);

    //     if (PixelRecovery != null)
    //     {
    //         StopCoroutine(PixelRecovery);
    //     }

    //     PixelRecovery = StartCoroutine(PixelRecover(Mathf.Lerp(basePixel, minPixel, impact),Mathf.Max(0.1f, recoverTime * impact)
    //         )
    //     );
    // }

    // IEnumerator PixelRecover(float hitPixel, float recover)
    // {
    //     float elapsed = 0f;

    //     while (elapsed < recover)
    //     {
    //         elapsed += Time.deltaTime;

    //         PixelCount.SetFloat("_pixelcount",Mathf.Lerp(hitPixel, basePixel, elapsed / recover));

    //         yield return null;
    //     }

    //     PixelCount.SetFloat("_pixelcount", basePixel);

    //     PixelRecovery = null;
    // }
}