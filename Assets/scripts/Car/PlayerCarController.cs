using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Logitech;
using System.Collections;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RacerScript))]
public class PlayerCarController : BaseCarController
{
    public CarInputActions Controls { get; protected set; }
    protected RacerScript racerScript;
    protected LogitechMovement LGM;
    private PlayerInput PlayerInput;
    private string CurrentControlScheme = "Keyboard";
    [Header("Turbo Type")]
    internal int turbeChargeAmount = 3;
    internal Coroutine TurbeBoost;
    internal float LastNonWheelInputTime = 0f;
    internal float LastWheelInputTime = 0f;

    [SerializeField] private GameObject carLights;
    private Material carLightsMaterial;

    override protected void Awake()
    {
        Controls = new CarInputActions();
        PlayerInput = GetComponent<PlayerInput>();
        TurbeBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();
        carLightsMaterial = GetComponentInChildren<Renderer>().materials[1];
        CarRb = GetComponent<Rigidbody>();
        racerScript = GetComponent<RacerScript>();
        TryGetComponent(out LGM);

        CarRb.centerOfMass = _CenterofMass;

        Controls.Enable();
        if (LGM != null) LGM.InitializeLogitechWheel(); 
        base.Awake();

        if (turbo != null)
        {
            Controls.CarControls.turbo.started += context => { turbo.Activate(); };
            Controls.CarControls.turbo.performed += context => { turbo.Stop(); };
        }
    }

    override protected void Start()
    {
        base.Start();
    }

    override protected void FixedUpdate()
    {
        float speed = CarRb.linearVelocity.magnitude;
        UpdateDriftSpeed();
        Move();
        Steer();
        Decelerate();
        Applyturnsensitivity(speed);
        WheelEffects(IsDrifting);
        base.FixedUpdate();
    }

    protected void Update()
    {
        //GetInputs();
        if (!Controls.CarControls.Drift.IsPressed()) StopDrifting();
        Animatewheels();
        // detect connection state changes and print once when it changes
        bool currentlyConnected = (LGM != null) && LGM.logitechInitialized && LogitechGSDK.LogiIsConnected(0);
        if (LGM != null && currentlyConnected != LGM.lastLogiConnected)
        {
            LGM.lastLogiConnected = currentlyConnected;
            Debug.Log($"[CarController] Logitech connection status: {(currentlyConnected ? "Connected" : "Disconnected")}");
        }

        if (LGM != null && LGM.useLogitechWheel && LGM.logitechInitialized && LogitechGSDK.LogiIsConnected(0))
        {
            LogitechGSDK.LogiUpdate();
            LGM.ApplyForceFeedback(); 
        }
    }

    // override protected void ApplySpeedLimit()
    // {
    //     MaxSpeed = Mathf.Clamp(MaxSpeed, 0, BaseMaxSpeed);
    //     if (CarRb.linearVelocity.magnitude * 3.6f > Maxspeed) CarRb.linearVelocity = Maxspeed / 3.6f * CarRb.linearVelocity.normalized;
    // }

    private void OnControlsChanged(PlayerInput input)
    {
        CurrentControlScheme = input.currentControlScheme;

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
            LGM.useLogitechWheel = false;
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


    private void OnEnable()
    {
        Controls.Enable();
        if (PlayerInput == null)
            PlayerInput = GetComponent<PlayerInput>();

        if (PlayerInput != null)
            PlayerInput.onControlsChanged += OnControlsChanged;

        Controls.CarControls.Get().actionTriggered += OnAnyActionTriggered;

        // INPUT SUBSCRIPTIONS: KERRAN
        Controls.CarControls.Move.performed += OnMovePerformed;
        Controls.CarControls.Move.canceled  += OnMoveCanceled;

        Controls.CarControls.Drift.performed   += OnDriftPerformed;
        Controls.CarControls.Drift.canceled    += OnDriftCanceled;
    }

    private void OnDisable()
    {
        Controls.Disable();
        if (PlayerInput != null)
            PlayerInput.onControlsChanged -= OnControlsChanged;

        Controls.CarControls.Get().actionTriggered -= OnAnyActionTriggered;

        // UNSUBSCRIBE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        Controls.CarControls.Move.performed -= OnMovePerformed;
        Controls.CarControls.Move.canceled  -= OnMoveCanceled;
        Controls.CarControls.Drift.performed -= OnDriftPerformed;
        Controls.CarControls.Drift.canceled  -= OnDriftCanceled;
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



    void UpdateDriftSpeed()
    {
        if (!IsDrifting) return;

        if (IsTurboActive)
            MaxSpeed = Mathf.Lerp(MaxSpeed, BaseSpeed + Turbesped, Time.deltaTime * 0.5f);
        else
            MaxSpeed = Mathf.Lerp(MaxSpeed, DriftMaxSpeed, Time.deltaTime * 0.1f);

        
        if (Mathf.Abs(MovementInputs.x) > 0.1f)
        {
            CarRb.AddTorque(Vector3.up * Time.deltaTime, ForceMode.Acceleration);
        }
    }




    // void GetInputs()
    // {
    //     //reads inputs and assigns them to values 
    //     // read non-wheel input (keyboard / gamepad) and mark last-non-wheel time when active
    //     float nonWheelMove = Mathf.Abs(MovementInputs.y) + Mathf.Abs(Controls.CarControls.MoveForward.ReadValue<float>()) + Mathf.Abs(Controls.CarControls.MoveBackward.ReadValue<float>());
    //     if (nonWheelMove > 0.001f || Controls.CarControls.Drift.IsPressed() || Controls.CarControls.Brake.IsPressed())
    //     {
    //         if (LGM != null)
    //         {
    //             LGM.useLogitechWheel = false;
    //             LGM.allowAutoEnable = true;
    //             LGM.StopAllForceFeedback();
    //         }
    //     }
        
    //     if (Controls.CarControls.MoveForward.IsPressed())
    //         MovementInputs.y = Controls.CarControls.MoveForward.ReadValue<float>();
    //     else if (Controls.CarControls.MoveBackward.IsPressed())
    //         MovementInputs.y = -Controls.CarControls.MoveBackward.ReadValue<float>();
    //     else
    //         MovementInputs.y = 0f;

    //     if (!Controls.CarControls.Drift.IsPressed())
    //         StopDrifting();
    // }

    void Applyturnsensitivity(float speed)
    {
        TurnSensitivity = Mathf.Lerp(
            MaxTurnSensitivity,
            MinTurnSensitivity,
            Mathf.Clamp01(speed / MaxSpeed));
    }

    // protected void HandleTurbo()
    // {
    //     if (!CanUseTurbo) return;
    //     Turbe.TURBO(this);
    //     TurbeMeter();
    // }



    void Move()
    {
        UpdateTargetTorque();
        AdjustSuspension();
        foreach (var wheel in Wheels)
        {
            if (Controls.CarControls.Brake.IsPressed()) wheel.Brake(BrakeAcceleration);
            else wheel.SetTorque(TargetTorque);
        }
        if (Controls.CarControls.Brake.IsPressed())
        {
            carLights.SetActive(true);
            carLightsMaterial.SetVector("_EmissionColor", new Vector4(1f, 0.0491371f, 0f, 1f) * 2f);
        }
        else if (carLightsMaterial.GetVector("_EmissionColor") != new Vector4(0f, 0f, 0f, 1f) * 2f || !carLights.activeSelf)
        {
            carLights.SetActive(false);
            carLightsMaterial.SetVector("_EmissionColor", new Vector4(0f, 0f, 0f, 1f) * 2f);
        }
    }

    private void UpdateTargetTorque()
    {
        // float inputValue = Mathf.Abs(MovementInputs.y);
        // if (CurrentControlScheme == "Gamepad")
        // {
        //     Vector2 moveVector = Controls.CarControls.Move.ReadValue<Vector2>();
        //     inputValue = Mathf.Max(inputValue, Mathf.Abs(moveVector.y));
        // }

        float steerFactor = Mathf.Clamp01(Mathf.Abs(MovementInputs.x));
        float driftPowerMultiplier = IsDrifting ? Mathf.Lerp(0.65f, 0.85f, steerFactor) : 1.0f;
        float targetMaxAcc = Acceleration * driftPowerMultiplier;

        SmoothedMaxAcceleration = Mathf.MoveTowards(
            SmoothedMaxAcceleration,
            targetMaxAcc,
            Time.deltaTime * 250f
        );

        float rawTorque = MovementInputs.y * SmoothedMaxAcceleration;
        float forwardVel = Vector3.Dot(CarRb.linearVelocity, transform.forward);
        if (IsDrifting && forwardVel > 0.5f && rawTorque < 0f) rawTorque = 0f;

        TargetTorque = rawTorque;

        if (IsDrifting)
        {
            TargetTorque *= Mathf.Lerp(0.5f, 0.7f, steerFactor); 
        }

        if (!IsDrifting)
        {
            MaxSpeed = Mathf.Lerp(MaxSpeed, IsTurboActive ? BaseSpeed + Turbesped : BaseSpeed, Time.deltaTime);
        }
    }



    public float GetDriftSharpness()
    {
        //Checks the drifts sharpness so scoremanager can see how good of a drift you're doing
        if (IsDrifting)
        {
            Vector3 velocity = CarRb.linearVelocity;
            Vector3 forward = transform.forward;
            float angle = Vector3.Angle(forward, velocity);
            return angle;  
        }
        return 0.0f;
    }

    //i hate this so much, its always somewhat broken but for now....... its not broken.
    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        if (IsDrifting || !CanDrift || racerScript.raceFinished) return;

        IsDrifting = true;


        foreach (var wheel in Wheels)
        {
            if (wheel.WheelCollider == null) continue;
            WheelFrictionCurve sideways = wheel.WheelCollider.sidewaysFriction;
            sideways.extremumSlip   = 0.9f;
            sideways.asymptoteSlip  = 1.6f;
            sideways.extremumValue  = 1.0f;
            sideways.asymptoteValue = 1.2f;
            sideways.stiffness      = 2.0f;
            wheel.WheelCollider.sidewaysFriction = sideways;
        }

        CarRb.angularDamping = 0.03f;
        AdjustWheelsForDrift();
        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        StopDrifting();
        OnDriftEndBoostTheCar();
        TargetTorque = BaseTargetTorque;
        WheelEffects(false);
    }

    public void StopDrifting()
    {
        if (IsDrifting)
        {
            IsDrifting = false;
        }
        float DeltaTime = Time.deltaTime * 2.5f;

        CarRb.angularDamping = Mathf.Lerp(CarRb.angularDamping, 0.1f, DeltaTime);
        
        foreach (var wheel in Wheels)
        {
            if (wheel.WheelCollider == null) continue;
            WheelFrictionCurve sideways = wheel.WheelCollider.sidewaysFriction;
            sideways.stiffness = Mathf.Lerp(sideways.stiffness, 5f, DeltaTime);
            sideways.extremumSlip  = Mathf.Lerp(sideways.extremumSlip, 0.15f, DeltaTime);
            sideways.asymptoteSlip = Mathf.Lerp(sideways.asymptoteSlip, 0.1f, DeltaTime);
            wheel.WheelCollider.sidewaysFriction = sideways;
        }
    }



    public void OnDriftEndBoostTheCar()
    {
        float driftmultiplier = ScoreManager.instance.CurrentDriftMultiplier;

        if (driftmultiplier < 6) return;

        float turbe = Mathf.InverseLerp(6f, 10f, driftmultiplier);
        float TurbeStrength = Mathf.Lerp(1f, 3f, turbe);
        float Duration = 3.5f;

        if (TurbeBoost != null)
            StopCoroutine(TurbeBoost);

        TurbeBoost = StartCoroutine(BoostCoroutine(TurbeStrength, Duration));
    }

    protected IEnumerator BoostCoroutine(float turboStrength, float durationOverride = -1f)
    {

        float GetCurrentBaseSpeed() => IsDrifting
            ? (IsTurboActive ? BaseSpeed + Turbesped : DriftMaxSpeed)
            : (IsTurboActive ? BaseSpeed + Turbesped : BaseSpeed);

        float originalSpeed = GetCurrentBaseSpeed();
        float boostedMax = Mathf.Max(BaseSpeed + Turbesped, originalSpeed + turboStrength);


        float duration = durationOverride > 0f
            ? durationOverride
            : Mathf.Lerp(2.5f, 4.5f, Mathf.InverseLerp(2f, 5f, turboStrength));

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0f, 1f, timer / duration);

            float expo = 1f - Mathf.Exp(-12f * timer / duration);
            CarRb.AddForce(transform.forward * turboStrength * 2.5f * expo * Time.deltaTime, ForceMode.VelocityChange);

            MaxSpeed = Mathf.Lerp(MaxSpeed, Mathf.Lerp(boostedMax, GetCurrentBaseSpeed(), smooth), Time.deltaTime * 2f);

            yield return null;
        }
        MaxSpeed = GetCurrentBaseSpeed();
        TurbeBoost = null;
    }
}
