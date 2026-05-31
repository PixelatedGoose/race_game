using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logitech;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class NewDoublefunszechuansauceWithAsideofNuggets : BaseCarController
{
    public CarInputActions Controls { get; protected set; } = new CarInputActions();
    RacerScript racerScript;
    LogitechMovement LGM;
    PlayerInput PlayerInput;
    string CurrentControlScheme;

    [Header("Visuals")]
    [SerializeField] private GameObject carLights;
    [SerializeField] private Material PixelCount;

    [Header("Steering")]
    [SerializeField] private float steerDeadzone = 0.15f;

    [Header(" Drift")]
    [SerializeField] float normalAngularDrag = 1.2f;
    [SerializeField] float driftAngularDrag = 0.05f;

    float rawSteerInput;



    [Header("Grounding")]
    [SerializeField] private int minGroundedWheelsForDrive = 2;

    float basePixel;
    float minPixel = 32f;

    float recoverTime = 2f;

    Coroutine PixelRecovery;

    protected override void Awake()
    {
        CarRb = GetComponent<Rigidbody>();
        racerScript = GetComponent<RacerScript>();
        TryGetComponent(out LGM);
        
        Controls = new CarInputActions();

        PlayerInput = GetComponent<PlayerInput>();
        LGM = FindFirstObjectByType<LogitechMovement>();
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
        CarMovement();



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
            if (LGM == null || !LGM.useLogitechWheel)
            {
                CarMovement();
            }
            if (IsDrifting){
                DriftPhysics();
            }
        }
        base.FixedUpdate();
    }

    int GetGroundedWheelCount()
    {
        int grounded = 0;

        foreach (Wheel wheel in Wheels)
        {
            if (wheel.collider == null)
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
        Vector3 moveDir = transform.forward;
        if (IsDrifting)
        {
            // Project velocity onto the car's local plane so it works on hills
            Vector3 slopeVel = Vector3.ProjectOnPlane(CarRb.linearVelocity, transform.up);
            if (slopeVel.sqrMagnitude > 1f) moveDir = slopeVel.normalized;
        }

        float forwardDot = Vector3.Dot(CarRb.linearVelocity, moveDir);

        float currentSign = Mathf.Abs(forwardDot) > 0.1f ? Mathf.Sign(forwardDot) : Mathf.Sign(MovementInputs.y);
        float signedSpeed = CarRb.linearVelocity.magnitude * currentSign;
        float forwardSpeed = Mathf.MoveTowards(signedSpeed, MaxSpeed * MovementInputs.y, Acceleration * Time.fixedDeltaTime);

        Vector3 horiz = moveDir * forwardSpeed;


        CarRb.linearVelocity = new Vector3(horiz.x, Mathf.Min(CarRb.linearVelocity.y, horiz.y), horiz.z);

        foreach (var wheel in Wheels)
        {

        }
    }

    void DriftPhysics()
    {

        Vector3 velocity = CarRb.linearVelocity;
        
        // Use the car's up vector (hill slope) instead of pure world up
        // otherwise drifting on slopes makes the car fly instantly
        Vector3 groundNormal = transform.up;

        velocity = Vector3.ProjectOnPlane(velocity, groundNormal);

        float speed = velocity.magnitude;
        if (speed < 0.1f) return;

        Vector3 velocityDir = velocity / speed;

        Vector3 right = Vector3.Cross(groundNormal, velocityDir).normalized;

        float steer = rawSteerInput;

        float speed01 = Mathf.Clamp01(speed / MaxSpeed);
        float speedSteerMultiplier = Mathf.Pow(1f - speed01, 2.2f);

        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        float slopeDamp = Mathf.Lerp(1f, 0.55f, slopeAngle / 45f);

        float steerStrength = 0.1421f * speedSteerMultiplier * slopeDamp;

        float inertia = Mathf.Lerp(6f, 2.2f, speed01);

        Vector3 desiredDirection = velocityDir + right * steer * steerStrength;

        desiredDirection = Vector3.ProjectOnPlane(desiredDirection, groundNormal).normalized;

        Vector3 targetVel = desiredDirection * speed;

        Vector3 verticalVel = Vector3.Project(CarRb.linearVelocity, groundNormal);
        Vector3 horizontalVel = Vector3.ProjectOnPlane(CarRb.linearVelocity, groundNormal);

        float time = Mathf.Clamp01(inertia * Time.fixedDeltaTime);

        Vector3 dir = Vector3.Slerp(horizontalVel.normalized, targetVel.normalized, time);
        horizontalVel = dir * horizontalVel.magnitude;

        CarRb.linearVelocity = horizontalVel + verticalVel;
        
        float visualSteerStrength = 0.55f * slopeDamp;
        Vector3 visualDesiredDirection = velocityDir + right * steer * visualSteerStrength;
        
        Vector3 visualDirection = Vector3.Slerp(velocityDir, visualDesiredDirection, Mathf.Abs(steer));
        visualDirection = Vector3.ProjectOnPlane(visualDirection, groundNormal).normalized;

        Quaternion targetRot = Quaternion.LookRotation(visualDirection, groundNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 4.5f * Time.fixedDeltaTime);
    
    }
        
    void SetDriftFriction(bool drifting)
    {
        foreach (Wheel wheel in Wheels)
        {
            WheelFrictionCurve sideways = wheel.collider.sidewaysFriction;
            WheelFrictionCurve forward = wheel.collider.forwardFriction;

            if (drifting){
                sideways.stiffness = 0.5f;
                forward.stiffness = 1.0f;
            }
            else{
                sideways.stiffness = 5.0f;
                forward.stiffness = 5.0f;
            }

            wheel.collider.sidewaysFriction = sideways;
            wheel.collider.forwardFriction = forward;
        }
    }

    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {

        IsDrifting = true;

        // 🔥 THIS is your WheelCollider "loose drift feel"
        CarRb.angularDamping = driftAngularDrag;

        foreach (Wheel wheel in Wheels)
        {
            WheelFrictionCurve sideways = wheel.collider.sidewaysFriction;
            sideways.stiffness = 2.0f;
            wheel.collider.sidewaysFriction = sideways;
        }

        AdjustWheelsForDrift();
        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx) => EndDrift();

    void EndDrift()
    {
        IsDrifting = false;

        SetDriftFriction(false);

        CarRb.angularDamping = normalAngularDrag;

        foreach (Wheel wheel in Wheels)
        {
            WheelFrictionCurve sideways = wheel.collider.sidewaysFriction;
            sideways.stiffness = 5.0f;
            wheel.collider.sidewaysFriction = sideways;
        }

        WheelEffects(false);
    }


    void OnBrakePerformed(InputAction.CallbackContext ctx) => Wheels.BrakeTorque = BrakeAcceleration;
    void OnBrakeCanceled(InputAction.CallbackContext ctx) => Wheels.MotorTorque = TargetTorque;

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

    void OnCollisionEnter(Collision collision)
    {
        if (PixelCount == null  || collision.impulse.magnitude < 0.1f ) 
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