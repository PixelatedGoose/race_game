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

    [Header("Visuals")]
    [SerializeField] private GameObject carLights;
    [SerializeField] private Material PixelCount;

    [Header("Steering")]
    [SerializeField] private float steerDeadzone = 0.15f;

    [Header(" Drift")]
    [SerializeField] float normalAngularDrag = 1.2f;
    [SerializeField] float driftAngularDrag = 0.05f;
    float AllowedDriftAngle = 45f;

    float rawSteerInput;



    [Header("Grounding")]
    [SerializeField] private int minGroundedWheelsForDrive = 2;

    float basePixel;
    float minPixel = 32f;

    float recoverTime = 2f;

    Coroutine PixelRecovery;

    protected override void Awake()
    {
        Controls = new CarInputActions();

        PlayerInput = GetComponent<PlayerInput>();
        LGM = FindFirstObjectByType<LogitechMovement>();

        CarRb = GetComponent<Rigidbody>();

        TurbeBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();

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
        MovementInputs.x = ApplySteerDeadzone(MovementInputs.x);
        rawSteerInput = MovementInputs.x;
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (LGM != null && LGM.useLogitechWheel)
            return;
        MovementInputs = Vector2.zero;
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
            ApplySpeedLimit();
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
        Vector3 moveDir = transform.forward;
        if (IsDrifting)
        {
            Vector3 slopeVel = Vector3.ProjectOnPlane(CarRb.linearVelocity, transform.up);
            if (slopeVel.sqrMagnitude > 1f) moveDir = slopeVel.normalized;
        }

        float forwardDot = Vector3.Dot(CarRb.linearVelocity, moveDir);

        float currentSign = Mathf.Abs(forwardDot) > 0.1f ? Mathf.Sign(forwardDot) : Mathf.Sign(MovementInputs.y);
        float signedSpeed = CarRb.linearVelocity.magnitude * currentSign;
        float forwardSpeed = Mathf.MoveTowards(signedSpeed, MaxSpeed * MovementInputs.y, Acceleration * Time.fixedDeltaTime);

        Vector3 horiz = moveDir * forwardSpeed;


        CarRb.linearVelocity = new Vector3(horiz.x, Mathf.Min(CarRb.linearVelocity.y, horiz.y), horiz.z);

    }

    void DriftPhysics()
    {
        Vector3 velocity = CarRb.linearVelocity;
        
        Vector3 groundNormal = transform.up;

        velocity = Vector3.ProjectOnPlane(velocity, groundNormal);
        if (velocity.magnitude < 0.1f) return;

        Vector3 velocityDirection = velocity / velocity.magnitude;

        Vector3 rightorleft = Vector3.Cross(groundNormal, velocityDirection).normalized;

        float steer = rawSteerInput;

        float steerStrength = 
            0.1421f 
            * Mathf.Pow(1f - Mathf.Clamp01(velocity.magnitude / MaxSpeed), 2.2f)
            * Mathf.Lerp(
                1f, 
                0.55f, 
                Vector3.Angle(groundNormal, Vector3.up) / AllowedDriftAngle
            );

        Vector3 desiredDirection = velocityDirection + rightorleft * steer * steerStrength;

        desiredDirection = Vector3.ProjectOnPlane(desiredDirection, groundNormal).normalized;

        Vector3 targetVelocity = desiredDirection * velocity.magnitude;

        Vector3 horizontalVel = Vector3.ProjectOnPlane(CarRb.linearVelocity, groundNormal);

        float time = Mathf.Clamp01(
            Mathf.Lerp(
                6f, 
                2.2f, 
                Mathf.Clamp01(velocity.sqrMagnitude / (MaxSpeed*MaxSpeed))
            ) * Time.fixedDeltaTime
        );

        Vector3 direction = Vector3.Slerp(horizontalVel.normalized, targetVelocity.normalized, time);
        horizontalVel = direction * horizontalVel.magnitude;

        CarRb.linearVelocity = horizontalVel + 
            Vector3.Project(CarRb.linearVelocity, groundNormal);
                    
        Vector3 visualDirection = Vector3.Slerp(
            velocityDirection, 
            velocityDirection + 0.55f * Mathf.Lerp(
                1f, 
                0.55f, 
                Vector3.Angle(groundNormal, Vector3.up) / AllowedDriftAngle
            ) * steer * rightorleft, 
            Mathf.Abs(steer)
        );

        visualDirection = Vector3.ProjectOnPlane(visualDirection, groundNormal).normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(visualDirection, groundNormal),
            4.5f * Time.fixedDeltaTime
        );
    }
        
    void SetDriftFriction(bool drifting)
    {
        foreach (Wheel wheel in Wheels)
        {
            WheelFrictionCurve sideways = wheel.WheelCollider.sidewaysFriction;
            WheelFrictionCurve forward = wheel.WheelCollider.forwardFriction;

            if (drifting){
                sideways.stiffness = 0.5f;
                forward.stiffness = 1.0f;
            }
            else{
                sideways.stiffness = 5.0f;
                forward.stiffness = 5.0f;
            }

            wheel.WheelCollider.sidewaysFriction = sideways;
            wheel.WheelCollider.forwardFriction = forward;
        }
    }

    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {

        IsDrifting = true;

        CarRb.angularDamping = driftAngularDrag;

        foreach (var wheel in Wheels)
        {
            if (wheel.WheelCollider == null) continue;

            WheelFrictionCurve sideways = wheel.WheelCollider.sidewaysFriction;
            sideways.stiffness = 2.0f;
            wheel.WheelCollider.sidewaysFriction = sideways;
        }

        AdjustWheelsForDrift();
        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        EndDrift();
    }

    void EndDrift()
    {
        IsDrifting = false;

        SetDriftFriction(false);

        CarRb.angularDamping = normalAngularDrag;

        foreach (var wheel in Wheels)
        {
            if (wheel.WheelCollider == null) continue;

            WheelFrictionCurve sideways = wheel.WheelCollider.sidewaysFriction;
            sideways.stiffness = 5.0f;
            wheel.WheelCollider.sidewaysFriction = sideways;
    }


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
        PixelRecovery = StartCoroutine(PixelRecover(Mathf.Lerp(basePixel, minPixel, impact),Mathf.Max(0.1f, recoverTime * impact)
            )
        );
    }

    IEnumerator PixelRecover(float hitPixel, float recover)
    {
        float elapsed = 0f;

        while (elapsed < recover)
        {
            elapsed += Time.deltaTime;
            PixelCount.SetFloat("_pixelcount",Mathf.Lerp(hitPixel, basePixel, elapsed / recover));
            yield return null;
        }

        PixelCount.SetFloat("_pixelcount", basePixel);
        PixelRecovery = null;
    }
}