using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    float FirstDriftDirection = 0f;
    float DriftDirectionSteer = 0f;

    float RightDriftAngle = 9f;
    float LeftDriftAngle = -9f;

    private float driftRearSidewaysStiffness = 2.0f;
    private float driftSteerMultiplierMin = 0f;
    private float driftSteerMultiplierMax = 2f;
    [SerializeField] private float driftSameSteerBoost = 0.7f;
    [SerializeField] private float driftOppositeSteerBoost = 0.35f;
    [SerializeField] private float driftInitialSteerMultiplier = 0.6f;

    private readonly Dictionary<WheelCollider, float> rearSidewaysStiffnessCache = new();
  

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

    float recoverTime = 2f;

    Coroutine PixelRecovery;

    float baseAngularDamping;
    float baseLinearDamping;
    float driftLockTimestamp = -1f;


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
        CacheRearSidewaysStiffness();
    }

    //movement or anykind of input related will go here
    protected void Update()
    {
        MovementInputs = Controls.CarControls.Move.ReadValue<Vector2>();
        MovementInputs.x = ApplySteerDeadzone(MovementInputs.x);
        if (IsDrifting && !Controls.CarControls.Drift.IsPressed()) EndDrift();
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
        Vector3 forward = transform.forward;
        float currentSign = Mathf.Abs(Vector3.Dot(CarRb.linearVelocity, forward)) > 0.1f ? Mathf.Sign(Vector3.Dot(CarRb.linearVelocity, forward)) : Mathf.Sign(MovementInputs.y);
        float signedSpeed = CarRb.linearVelocity.magnitude * currentSign;
        float forwardSpeed = Mathf.MoveTowards(
            signedSpeed,
            MaxSpeed * MovementInputs.y,
            Acceleration * Time.deltaTime
        );

        Vector3 newVelocity = forward * forwardSpeed;
        if (IsDrifting)
        {
            Vector3 right = transform.right;
            newVelocity += right * Vector3.Dot(CarRb.linearVelocity, right);
            Vector3 horizontal = new Vector3(newVelocity.x, 0f, newVelocity.z);
            if (horizontal.magnitude > 0.01f && Mathf.Abs(forwardSpeed) > 0.01f)
            {
                horizontal = horizontal / horizontal.magnitude * Mathf.Abs(forwardSpeed);
                newVelocity.x = horizontal.x;
                newVelocity.z = horizontal.z;
            }
        }

        newVelocity.y = CarRb.linearVelocity.y;
        CarRb.linearVelocity = newVelocity;
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

        float inputSign = GetSteerSign(MovementInputs.x);
        if (Mathf.Approximately(FirstDriftDirection, 0f))
        {
            if (Mathf.Approximately(inputSign, 0f)) return 0f;
            FirstDriftDirection = inputSign;
            DriftDirectionSteer = FirstDriftDirection;
            driftLockTimestamp = Time.time;
        }

        float steerDirection = FirstDriftDirection;
        float driftMultiplier = RemapSteerToDriftMultiplier(MovementInputs.x, FirstDriftDirection);
        float initialFactor = 1f;
        if (driftLockTimestamp > 0f && driftLockTime > 0f)
        {
            float t = Mathf.Clamp01((Time.time - driftLockTimestamp) / driftLockTime);
            initialFactor = Mathf.Lerp(driftInitialSteerMultiplier, 1f, t);
        }
        driftMultiplier *= driftSameSteerBoost * initialFactor;
        float lerpSpeed = driftDirectionLerp;
        bool isOpposite = false;

        if (!Mathf.Approximately(inputSign, 0f) && inputSign != FirstDriftDirection)
        {
            steerDirection = inputSign;
            driftMultiplier = RemapSteerToDriftMultiplier(MovementInputs.x, inputSign) * driftOppositeSteerBoost;
            isOpposite = true;
        }

        if (isOpposite) lerpSpeed *= 0.5f;
        DriftDirectionSteer = Mathf.Lerp(DriftDirectionSteer, steerDirection, Time.deltaTime * lerpSpeed);
        return DriftDirectionSteer * driftMultiplier;
    }

    float ApplySteerDeadzone(float steer)
    {
        return Mathf.Abs(steer) < SteerDeadzone ? 0f : steer;
    }

    float RemapSteerToDriftMultiplier(float steer, float driftDirection)
    {
        if (Mathf.Approximately(driftDirection, 0f)) return 0f;

        float outMin = driftDirection > 0f ? driftSteerMultiplierMin : driftSteerMultiplierMax;
        float outMax = driftDirection > 0f ? driftSteerMultiplierMax : driftSteerMultiplierMin;
        return Remap(steer, -1f, 1f, outMin, outMax);
    }

    static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (Mathf.Approximately(inMin, inMax)) return outMin;
        return Mathf.Lerp(outMin, outMax, Mathf.InverseLerp(inMin, inMax, value));
    }

    // your car is drifting bro better go and call your insurance company
    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        FirstDriftDirection = 0f;
        driftLockTimestamp = -1f;

        CarRb.angularDamping = 0.02f;
        CarRb.linearDamping = 0.08f;

        if (rearSidewaysStiffnessCache.Count == 0) CacheRearSidewaysStiffness();
        ApplyRearDriftFriction();
        IsDrifting = true;
        WheelEffects(true);
    }

    void AdjustWheelsBackToNormal()
    {
        foreach (Wheel wheel in Wheels)
        {
            if (wheel.Axel != Axel.Rear || wheel.WheelCollider == null) continue;
            if (!rearSidewaysStiffnessCache.TryGetValue(wheel.WheelCollider, out float baseStiffness)) continue;
            WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
            sidewaysFriction.stiffness = baseStiffness;
            wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
        }  
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        EndDrift();
    }

    void EndDrift()
    {
        IsDrifting = false;
        CurrentDriftAngle = 0f;
        FirstDriftDirection = 0f;
        DriftDirectionSteer = 0f;
        driftLockTimestamp = -1f;
        CarRb.angularDamping = baseAngularDamping;
        CarRb.linearDamping = baseLinearDamping;
        AdjustWheelsBackToNormal();
        WheelEffects(false);
    }

    void CacheRearSidewaysStiffness()
    {
        rearSidewaysStiffnessCache.Clear();
        foreach (Wheel wheel in Wheels)
        {
            if (wheel.Axel != Axel.Rear || wheel.WheelCollider == null) continue;
            rearSidewaysStiffnessCache[wheel.WheelCollider] = wheel.WheelCollider.sidewaysFriction.stiffness;
        }
    }

    void ApplyRearDriftFriction()
    {
        foreach (Wheel wheel in Wheels)
        {
            if (wheel.Axel != Axel.Rear || wheel.WheelCollider == null) continue;
            WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
            sidewaysFriction.stiffness = driftRearSidewaysStiffness;
            wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }

}