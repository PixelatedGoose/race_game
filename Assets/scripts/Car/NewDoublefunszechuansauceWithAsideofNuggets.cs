using System;
using System.Collections;
using Unity.Splines.Examples;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class NewDoublefunszechuansauceWithAsideofNuggets : BaseCarController
{
    public CarInputActions Controls { get; protected set; }
    LogitechMovement LGM;
    private string CurrentControlScheme;
    PlayerInput PlayerInput;
    [SerializeField] private GameObject carLights;
    private Material carLightsMaterial;
    public Material PixelCount;

    float DriftTurnSpeed = 3.5f;
    float DriftTurn = 1.2f;
    float SteerDeadzone = 0.2f;
    float CurrentDriftAngle = 0f;

    float DriftDirection = 0f;

    float RightDriftAngle = 9f;
    float LeftDriftAngle = -9f;
  

    public float driftLockTime = 0.22f;
    public float steerSmooth = 8f;
    public float driftBiasSet = 5.5f;
    public float driftLateralSpeedFactor = 1.0f;
    public float driftLateralControl = 1.0f;
    public float driftLateralDamping = 1.0f;
    public float driftForceMultiplier = 4.0f;
    public float driftInputBias = 0.6f;
    public float driftDirectionLerp = 8f;


    float basePixel;
    float minPixel = 32f;

    float recoverTime = 4f;

    Coroutine PixelRecovery;

    float baseAngularDamping;
    float baseLinearDamping;


    float GetSteerSign(float steer) => Mathf.Abs(steer) > SteerDeadzone ? Mathf.Sign(steer) : 0f;

  
      
    override protected void Awake()
    {
        Controls = new CarInputActions();
        CarRb = GetComponent<Rigidbody>();
        baseAngularDamping = CarRb.angularDamping;
        baseLinearDamping = CarRb.linearDamping;
        TurbeBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();
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

        Controls.CarControls.Drift.performed   += OnDriftPerformed;
        Controls.CarControls.Drift.canceled    += OnDriftCanceled;
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
        MovementInputs = Vector2.zero;
    }

    override protected void Start()
    {
        base.Start();

        basePixel = PixelCount.GetFloat("_pixelcount");
    }

    //movement or anykind of input related will go here
    protected void Update()
    {
        MovementInputs = Controls.CarControls.Move.ReadValue<Vector2>();
        Animatewheels();
        MovementInputs.x = GetDriftSteer();
        Steer();
        CarMovement();
        ApplySpeedLimit();
        Decelerate();
    }

    //physics related will go here
    override protected void FixedUpdate()
    {
        TurnSensitivity = CarRb.linearVelocity.magnitude / MaxSpeed * turnSensitivityRange + MaxTurnSensitivity;
        base.FixedUpdate();
        ApplyDriftForce();
        ApplyDriftTurn();
        // HandleTurbo();
    }

    //que
//    protected void HandleTurbo()
//     {
//         if (!CanUseTurbo) return;
//         Turbe.TURBO(this);
//         TurbeMeter();
//     }





    //Arcade car style movement
    protected void CarMovement()
    {
        Vector3 velocity = CarRb.linearVelocity;
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);
        float targetForward = Mathf.MoveTowards(
            forwardSpeed,
            MaxSpeed * MovementInputs.y,
            Acceleration * Time.deltaTime
        );

        Vector3 finalVelocity = transform.forward * targetForward;

        if (IsDrifting)
        {
            finalVelocity += transform.right * Vector3.Dot(velocity, transform.right);
        }
        else
        {
            finalVelocity += transform.right * Mathf.MoveTowards(Vector3.Dot(velocity, transform.right), 0f, driftLateralDamping * Time.deltaTime);
        }

        finalVelocity.y = velocity.y;
        CarRb.linearVelocity = finalVelocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (PixelCount == null || collision.impulse.magnitude < 0.1f) return;


        float impact = Mathf.Clamp01(collision.relativeVelocity.magnitude / Mathf.Max(MpsMaxSpeed, 0.01f));
        impact = Mathf.SmoothStep(0f, 1f, impact);

        if (PixelRecovery != null) StopCoroutine(PixelRecovery);
        PixelRecovery = StartCoroutine(PixelRecover(Mathf.Lerp(basePixel, minPixel, impact), Mathf.Max(0.1f, recoverTime * impact)));
    }

    IEnumerator PixelRecover(float hitPixel, float recover)
    {
        float elapsed = 0f;
        while (elapsed < recover)
        {
            elapsed += Time.deltaTime;
            PixelCount.SetFloat("_pixelcount", Mathf.Lerp(hitPixel, basePixel, elapsed / recover));
            yield return null;
        }
        PixelCount.SetFloat("_pixelcount", basePixel);
        PixelRecovery = null;
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
    
    float GetDriftSteer()
    {
        if (!IsDrifting) return MovementInputs.x;

        if (Mathf.Approximately(DriftDirection, 0f)) DriftDirection = GetSteerSign(MovementInputs.x);

        return DriftDirection;
    }

    void ApplyDriftTurn()
    {
        if (!IsDrifting)
            return;
        CarRb.AddTorque(Vector3.up * DriftDirection * Mathf.Lerp(DriftTurnSpeed, 2f, Mathf.Clamp01(CarRb.linearVelocity.magnitude / Mathf.Max(MaxSpeed, 0.01f))), ForceMode.Acceleration);
    }

    void ApplyDriftForce()
    {
        if (!IsDrifting)
            return;

        if (!Mathf.Approximately(GetSteerSign(MovementInputs.x), 0f))
        {
            DriftDirection = Mathf.MoveTowards(DriftDirection, Mathf.Lerp(DriftDirection, GetSteerSign(MovementInputs.x), driftInputBias), driftDirectionLerp * Time.fixedDeltaTime);
        }

        if (Mathf.Approximately(DriftDirection, 0f))
            return;

        Vector3 velocity = CarRb.linearVelocity;
        float delta = ( DriftDirection * MaxSpeed * driftLateralSpeedFactor * Mathf.Clamp01( Mathf.Abs(Vector3.Dot(velocity, transform.forward))/ Mathf.Max(MaxSpeed, 0.01f)) - Vector3.Dot(velocity, transform.right)) * driftLateralControl;
        Vector3 lateralForce = transform.right * (delta * driftForceMultiplier);

        CarRb.AddForce(lateralForce, ForceMode.VelocityChange);
    }


    // your car is drifting bro better go and call your insurance company
    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        float inputSign = GetSteerSign(MovementInputs.x);
        if (Mathf.Approximately(inputSign, 0f))
            inputSign = Mathf.Sign(Vector3.Dot(CarRb.linearVelocity, transform.right));
        DriftDirection = inputSign;

        CarRb.angularDamping = 0.02f;
        CarRb.linearDamping = 0.08f;

        AdjustWheelsForDrift();
        IsDrifting = true;
        WheelEffects(true);
    }

    void AdjustWheelsBackToNormal()
    {
        foreach (Wheel wheel in Wheels)
        {
            WheelFrictionCurve forwwardfriction = wheel.WheelCollider.forwardFriction;
            forwwardfriction.asymptoteValue = 1f; forwwardfriction.asymptoteSlip = 0.8f;
            forwwardfriction.extremumValue = 1f; forwwardfriction.extremumSlip = 0.6f;
            wheel.WheelCollider.forwardFriction = forwwardfriction;

            if (wheel.Axel == Axel.Front)
            {
                WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = 6f; wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }  
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        IsDrifting = false;
        CurrentDriftAngle = 0f;
        DriftDirection = 0f;
        CarRb.angularDamping = baseAngularDamping;
        CarRb.linearDamping = baseLinearDamping;
        AdjustSuspension();
        AdjustWheelsBackToNormal();
        WheelEffects(false);
    }

}