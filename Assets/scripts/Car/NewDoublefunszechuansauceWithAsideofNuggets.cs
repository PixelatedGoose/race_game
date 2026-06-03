using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Logitech;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class NewDoublefunszechuansauceWithAsideofNuggets : BaseCarController
{
    public CarInputActions Controls { get; protected set; } //new CarInputActions();

    LogitechMovement LGM;
    PlayerInput PlayerInput;

    string CurrentControlScheme;

    [SerializeField] private GameObject carLights;
    [SerializeField] private Material PixelCount;

    [SerializeField] private float steerDeadzone = 0.15f;

    [SerializeField] private int minGroundedWheelsForDrive = 2;

    float basePixel;
    float minPixel = 32f;
    float recoverTime = 2f;
    private bool isBraking = false;
    Coroutine PixelRecovery;
    private MultCounter multCounter;
    float steerSmoothedForce;
    float steerSmoothed;
    float sideVelSmoothed;

    float driftExitBlend = 1f;

    protected override void Awake()
    {
        CarRb = GetComponent<Rigidbody>();
        TryGetComponent(out LGM);
        multCounter = GameManager.instance.CarUI.GetComponentInChildren<MultCounter>();
        
        Controls = new CarInputActions();

        PlayerInput = GetComponent<PlayerInput>();
        LGM = FindFirstObjectByType<LogitechMovement>();

        CarRb = GetComponent<Rigidbody>();

        // TurbeBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();

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
        if (LGM != null) LGM.InitializeLogitechWheel(); 
        base.Start();

        basePixel = PixelCount.GetFloat("_pixelcount");
    }

    private void OnEnable()
    {
        Controls.Enable();

        if (PlayerInput != null)
            PlayerInput.onControlsChanged += OnControlsChanged;

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

        if (LGM == null)
            return;

        if (CurrentControlScheme == "Keyboard")
        {
            LGM.useLogitechWheel = false;
            LGM.allowAutoEnable = true;
        }
        else if (CurrentControlScheme == "Gamepad")
        {
            LGM.allowAutoEnable = true;
        }
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        MovementInputs = ctx.ReadValue<Vector2>();
        Steer();

        if (!isBraking) Wheels.MotorTorque = MovementInputs.y * Acceleration;
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (LGM != null && LGM.useLogitechWheel) return;
        MovementInputs = Vector2.zero;
        Steer();
        Wheels.MotorTorque = 0;
    }

    protected void Update()
    {
        Animatewheels();
        Steer();

        if (LGM != null && LGM.useLogitechWheel)
        {
            LGM.allowAutoEnable = true;
            LogitechGSDK.LogiUpdate();
            LGM.ApplyForceFeedback();  
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (GetGroundedWheelCount() >= minGroundedWheelsForDrive)
        {


            if (IsDrifting){
                DriftPhysics();
            }
            Decelerate();
        }
    }

    int GetGroundedWheelCount()
    {
        int grounded = 0;

        foreach (Wheel wheel in Wheels)
        {
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

    //before you even fucking ask lamelemon, YES i used AI a lot bcs the deadline is too close
    void DriftPhysics()
    {
        ApplyDriftForce();
        ApplyDriftRotation();
    }

    void ApplyDriftForce()
    {
        Vector3 vel = CarRb.linearVelocity;
        Vector3 up = transform.up;

        Vector3 planar = Vector3.ProjectOnPlane(vel, up);

        float speed = planar.magnitude;
        if (speed < 0.1f) return;

        float steer = MovementInputs.x;

        float forwardDot = Vector3.Dot(planar.normalized, transform.forward);

        float reverseFactor = Mathf.Lerp(1f, 0.25f, Mathf.Clamp01(-forwardDot));

        float turnStrength =
            steer *
            Mathf.Lerp(60f, 140f, speed / MaxSpeed) *
            reverseFactor *
            Time.fixedDeltaTime;

        Vector3 dir = Quaternion.AngleAxis(turnStrength, up) * planar.normalized;

        planar = Vector3.Slerp(planar, dir * planar.magnitude, 0.75f);

        Vector3 targetVel = planar + Vector3.Project(vel, up);

        CarRb.linearVelocity = Vector3.Lerp(
            CarRb.linearVelocity,
            targetVel,
            driftExitBlend
        );
    }

    void ApplyDriftRotation()
    {
        Vector3 planar = Vector3.ProjectOnPlane(CarRb.linearVelocity, transform.up);

        if (planar.sqrMagnitude < 0.01f)
            return;

        float speed01 = Mathf.Clamp01(planar.magnitude / MaxSpeed);
        float steer = MovementInputs.x;

        Vector3 velDir = planar.normalized;

        Vector3 targetDir = Vector3.Slerp(
            transform.forward,
            velDir + transform.right * steer * 0.8f,
            Mathf.Lerp(0.2f, 0.45f, speed01)
        );

        Quaternion rot = Quaternion.LookRotation(targetDir, transform.up);

        CarRb.MoveRotation(
            Quaternion.Slerp(CarRb.rotation, rot, 12f * Time.fixedDeltaTime)
        );
    }

    void SetDriftFriction(bool drifting)
    {
        float side = drifting ? .3f : 5f;
        float forward = drifting ? .6f : 5f;

        foreach (Wheel wheel in Wheels)
        {
            var sidewaysfriction = wheel.collider.sidewaysFriction;
            var forwardfriction = wheel.collider.forwardFriction;

            sidewaysfriction.stiffness = side;
            forwardfriction.stiffness = forward;

            wheel.collider.sidewaysFriction = sidewaysfriction;
            wheel.collider.forwardFriction = forwardfriction;
        }
    }

    void OnDriftPerformed(InputAction.CallbackContext _)
    {
        if (IsDrifting || MovementInputs.y < 0f)
            return;

        IsDrifting = true;

        SetDriftFriction(true);
        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext _) => EndDrift();

    void EndDrift()
    {
        IsDrifting = false;

        SetDriftFriction(false);
        WheelEffects(false);
    }

    void OnBrakePerformed(InputAction.CallbackContext ctx)
    {
        Wheels.BrakeTorque = BrakeAcceleration;
        Wheels.MotorTorque = 0;
        isBraking = true;
    }
    void OnBrakeCanceled(InputAction.CallbackContext ctx)
    {
        Wheels.BrakeTorque = 0;
        Wheels.MotorTorque = MovementInputs.y * Acceleration;
        isBraking = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == grassLayer.value)
        {
            multCounter.ResetMultiplier();
        }

        if (PixelCount == null  || collision.impulse.sqrMagnitude < 0.1f ) 
            return;

        if (IsDrifting)
            EndDrift();

        float impact = Mathf.Clamp01(
            collision.relativeVelocity.magnitude / Mathf.Max(MpsMaxSpeed, 0.01f)
        );

        if (impact > 0.9f)
            impact = 1f;

        impact = Mathf.SmoothStep(0f, 1f, impact);

        if (PixelRecovery != null)
        {
            StopCoroutine(PixelRecovery);
        }

        PixelRecovery = StartCoroutine(
            PixelRecover(
                Mathf.Lerp(basePixel, minPixel, impact),
                Mathf.Max(0.1f, recoverTime * impact)
            )
        );
    }

    IEnumerator PixelRecover(float hitPixel, float recover)
    {
        float elapsed = 0f;

        while (elapsed < recover)
        {
            elapsed += Time.deltaTime;

            PixelCount.SetFloat(
                "_pixelcount",
                Mathf.Lerp(hitPixel, basePixel, elapsed / recover)
            );

            yield return null;
        }

        PixelCount.SetFloat("_pixelcount", basePixel);
        PixelRecovery = null;
    }
}