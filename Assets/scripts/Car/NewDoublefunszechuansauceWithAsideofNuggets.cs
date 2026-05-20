using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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


    float SteerDeadzone = 0.2f;

    float FirstDriftDirection = 0f;
    float DriftDirectionSteer = 0f;

    [SerializeField] private float driftRearSidewaysStiffness = 1.6f;


    [SerializeField] private float counterSteerGripMultiplier = 1.35f;
    [SerializeField] private float driftYawAssist = 3.5f;
    [SerializeField] private float counterYawMultiplier = 0.4f;
    [SerializeField] private float driftAngularDrag = 0.5f;
    [SerializeField] private float driftLinearDrag = 0.05f;
    [SerializeField] private float steerDeadzone = 0.15f;
    [SerializeField] private float driftforwardStabilize;

    private float firstDriftDirection;
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
    [SerializeField] private float driftInputBiasLowSpeed = 0.2f;
    [SerializeField] private float driftAssistMinSpeed = 3f;
    [SerializeField] private float driftAssistMaxSpeed = 18f;


    float basePixel;
    float minPixel = 32f;

    float recoverTime = 2f;

    Coroutine PixelRecovery;

    float baseAngularDamping;
    float baseLinearDamping;




  
      
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
            Controls.CarControls.turbo.canceled += context => { turbo.Stop(); };
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
        if (IsDrifting){
            MovementInputs.x = GetDriftSteer();
        }
        Steer();
        if (IsAnyWheelGrounded())
        {
            CarMovement();
        }
        ApplySpeedLimit();
        Decelerate();
    }

    //physics related will go here
    override protected void FixedUpdate()
    {
        TurnSensitivity = CarRb.linearVelocity.magnitude / MaxSpeed * turnSensitivityRange + MaxTurnSensitivity;
        if (IsDrifting) {
            ApplyRearDriftFriction();
            ApplyDriftAssist();
        }
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
        Vector3 right = transform.right;

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
            float lateral = Vector3.Dot(CarRb.linearVelocity, right);
            horiz += right * lateral;

            float target = Mathf.Abs(forwardSpeed);
            float mag = new Vector3(horiz.x, 0f, horiz.z).magnitude;
            if (mag > target && target > 0.01f)
                horiz *= target / mag;
        }

        CarRb.linearVelocity = new Vector3(horiz.x, CarRb.linearVelocity.y, horiz.z);
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
        float input = MovementInputs.x;
        float absInput = Mathf.Abs(input);

        if (Mathf.Approximately(firstDriftDirection, 0f))
        {
            if (absInput < steerDeadzone)
                return 0f;

            firstDriftDirection = Mathf.Sign(input);
        }

        bool counterSteering =
            absInput > steerDeadzone &&
            Mathf.Sign(input) != firstDriftDirection;

        float locked = counterSteering
            ? firstDriftDirection * 0.4f
            : firstDriftDirection;

        locked *= Mathf.Clamp01(absInput);

        float speedFactor = GetDriftSpeedFactor();
        float bias = Mathf.Lerp(driftInputBiasLowSpeed, driftInputBias, speedFactor);

        return Mathf.Lerp(input, locked, bias);
    }

    void ApplyDriftAssist(){
        bool counterSteering = Mathf.Abs(MovementInputs.x) > steerDeadzone && math.sign(MovementInputs.x) != firstDriftDirection;

        if (Mathf.Approximately(firstDriftDirection, 0f)) return;

        float yawAssistBase = counterSteering ? driftYawAssist * counterYawMultiplier : driftYawAssist;
        float yawAssist = yawAssistBase * GetDriftSpeedFactor();

        CarRb.AddTorque(Vector3.up * firstDriftDirection * yawAssist, ForceMode.Acceleration);

    }

    float GetDriftSpeedFactor()
    {
        Vector3 planarVel = CarRb.linearVelocity;
        planarVel.y = 0f;
        float speed = planarVel.magnitude;
        return Mathf.Clamp01(Mathf.InverseLerp(driftAssistMinSpeed, driftAssistMaxSpeed, speed));
    }

    float ApplySteerDeadzone(float steer)
    {
        return Mathf.Abs(steer) < SteerDeadzone ? 0f : steer;
    }

    bool IsAnyWheelGrounded()
    {
        foreach (Wheel wheel in Wheels)
        {
            if (wheel?.WheelCollider == null) continue;
            if (wheel.IsGrounded()) return true;
        }
        return false;
    }

    


    //this is to remap the normal left to right values from -1 - 1 to -1, 0, 1, 2. for better feeling for this drift 
    static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (Mathf.Approximately(inMin, inMax)) return outMin;
        return Mathf.Lerp(outMin, outMax, Mathf.InverseLerp(inMin, inMax, value));
    }

    // your car is drifting bro better go and call your insurance company
    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        IsDrifting = true;

        firstDriftDirection = 0f;

        CarRb.angularDamping = driftAngularDrag;
        CarRb.linearDamping = driftLinearDrag;

        if (rearSidewaysStiffnessCache.Count == 0)
            CacheRearSidewaysStiffness();

        ApplyRearDriftFriction();

        WheelEffects(true);
    }

    void AdjustWheelsBackToNormal()
    {
        foreach (Wheel wheel in Wheels)
        {
            if (wheel.Axel != Axel.Rear || wheel.WheelCollider == null) continue;
            if (!rearSidewaysStiffnessCache.TryGetValue(wheel.WheelCollider,out float baseStiffness)) continue;

            WheelFrictionCurve friction = wheel.WheelCollider.sidewaysFriction;
            friction.stiffness = baseStiffness;
            wheel.WheelCollider.sidewaysFriction = friction;
        }
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        EndDrift();
    }

    void EndDrift()
    {
        IsDrifting = false;

        firstDriftDirection = 0f;

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
        bool counterSteering =
            Mathf.Abs(MovementInputs.x) > steerDeadzone &&
            Mathf.Sign(MovementInputs.x) != firstDriftDirection;

        float stiffness =
            counterSteering
            ? driftRearSidewaysStiffness * counterSteerGripMultiplier
            : driftRearSidewaysStiffness;

        foreach (Wheel wheel in Wheels)
        {
            if (wheel.Axel != Axel.Rear || wheel.WheelCollider == null)
                continue;

            WheelFrictionCurve friction =
                wheel.WheelCollider.sidewaysFriction;

            friction.stiffness = stiffness;

            wheel.WheelCollider.sidewaysFriction = friction;
        }
    }

}