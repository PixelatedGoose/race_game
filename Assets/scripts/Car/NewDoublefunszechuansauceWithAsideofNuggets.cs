using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logitech;
using System;
using NUnit.Framework;

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
    float rawSteerInput;
    float smoothedDriftAngle;

    [SerializeField] private int minGroundedWheelsForDrive = 2;
    float smoothedSteer;

    float basePixel;
    float minPixel = 32f;
    float recoverTime = 2f;
    Coroutine PixelRecovery;


    protected override void Awake()
    {
        CarRb = GetComponent<Rigidbody>();
        TryGetComponent(out LGM);
        
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
        if (LGM != null && LGM.useLogitechWheel)
            return;

        MovementInputs = ctx.ReadValue<Vector2>();
        Steer();
        MovementInputs.x = ApplySteerDeadzone(MovementInputs.x);
        rawSteerInput = MovementInputs.x;
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (LGM != null && LGM.useLogitechWheel)
            return;
        MovementInputs = Vector2.zero;
        Steer();
        rawSteerInput = 0f;
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
        TurnSensitivity = CarRb.linearVelocity.magnitude / MaxSpeed * turnSensitivityRange + MaxTurnSensitivity;
        
        if (GetGroundedWheelCount() >= minGroundedWheelsForDrive)
        {
            CarMovement();

            if (IsDrifting){
                DriftPhysics();
            }
            Decelerate();
        }
        base.FixedUpdate();
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

    protected void CarMovement()
    {
        Vector3 moveDir =
        IsDrifting
            ? Vector3.ProjectOnPlane(CarRb.linearVelocity, transform.up).normalized
            : transform.forward;

        float forwardDot = Vector3.Dot(CarRb.linearVelocity, moveDir);

        float currentSign = Mathf.Abs(forwardDot) > 0.1f ? Mathf.Sign(forwardDot) : Mathf.Sign(MovementInputs.y);
        float signedSpeed = CarRb.linearVelocity.magnitude * currentSign;
        float accelMultiplier = IsDrifting ? 0.25f : 1f;
        float targetSpeed = MaxSpeed * MovementInputs.y;

        if (IsDrifting)
        {
            Vector3 slopeVel = Vector3.ProjectOnPlane(CarRb.linearVelocity, transform.up);
            targetSpeed = Mathf.Max(Mathf.Abs(signedSpeed), Mathf.Abs(targetSpeed));
            if (slopeVel.sqrMagnitude > 1f) moveDir = slopeVel.normalized;
        }

        float forwardSpeed = Mathf.MoveTowards(
            signedSpeed,
            targetSpeed,
            Acceleration * accelMultiplier * Time.fixedDeltaTime
        );

        Vector3 horiz = moveDir * forwardSpeed;
        CarRb.linearVelocity = new Vector3(horiz.x, Mathf.Min(CarRb.linearVelocity.y, horiz.y), horiz.z);
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

        if (speed < 0.2f) return;

        Vector3 forward = planar / speed;
        Vector3 right = Vector3.Cross(up, forward);

        float steer = rawSteerInput;

        smoothedSteer = Mathf.Lerp(smoothedSteer, steer, 8f * Time.fixedDeltaTime);

        float speed01 = Mathf.Clamp01(speed / MaxSpeed);

        float driftStrength = Mathf.Lerp(45f, 10f, speed01);

        float steerResponse = Mathf.SmoothStep(0f, 1f, Mathf.Abs(smoothedSteer));

        Vector3 sideForce =
            right * smoothedSteer * driftStrength * steerResponse;

        Vector3 lateralVelocity = Vector3.Project(planar, right);

        Vector3 damping =
            -lateralVelocity * 0.01f; 

        CarRb.linearVelocity += (sideForce + damping) * Time.fixedDeltaTime;
    }

    void ApplyDriftRotation()
    {
        Vector3 vel = CarRb.linearVelocity;
        Vector3 up = transform.up;

        Vector3 planar = Vector3.ProjectOnPlane(vel, up);

        if (planar.sqrMagnitude < 0.01f) return;

        Vector3 velocityDir = planar.normalized;

        float steer = rawSteerInput;

        Vector3 targetDir =
            Vector3.RotateTowards(
                transform.forward,
                velocityDir + transform.right * steer * 0.8f,
                6f * Time.fixedDeltaTime,
                0f
            );

        Quaternion targetRot = Quaternion.LookRotation(targetDir, up);

        CarRb.MoveRotation(
            Quaternion.Slerp(CarRb.rotation, targetRot, 9f * Time.fixedDeltaTime)
        );
    }

    void SetDriftFriction(bool drifting)
    {
        foreach (Wheel wheel in Wheels)
        {
            WheelFrictionCurve sideways = wheel.collider.sidewaysFriction;
            WheelFrictionCurve forward = wheel.collider.forwardFriction;

            if (drifting){
                sideways.stiffness = 0.3f;
                forward.stiffness = 0.6f;
            }
            else{
                sideways.stiffness = 5f;
                forward.stiffness = 5f;
            }

            wheel.collider.sidewaysFriction = sideways;
            wheel.collider.forwardFriction = forward;
        }
    }

    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {

        IsDrifting = true;
        

        SetDriftFriction(true);

        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx) => EndDrift();

    void EndDrift()
    {
        IsDrifting = false;
        smoothedDriftAngle = 0f;
        SetDriftFriction(false);

        WheelEffects(false);
    }


    void OnBrakePerformed(InputAction.CallbackContext ctx) => Wheels.BrakeTorque = BrakeAcceleration;
    void OnBrakeCanceled(InputAction.CallbackContext ctx) => Wheels.MotorTorque = TargetTorque;

    float ApplySteerDeadzone(float steer)
    {
        return Mathf.Abs(steer) < steerDeadzone ? 0f : steer;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (PixelCount == null  || collision.impulse.sqrMagnitude < 0.1f ) 
            return;

        float impact = Mathf.Clamp01(collision.relativeVelocity.magnitude / Mathf.Max(MpsMaxSpeed, 0.01f));

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