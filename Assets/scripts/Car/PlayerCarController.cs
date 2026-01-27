using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using Logitech;
using System.Linq;

 
public class PlayerCarController : BaseCarController
{


    RacerScript racerScript;


    private PlayerInput PlayerInput;
    private string CurrentControlScheme = "Keyboard";


    

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
        TurbeMeter = GameObject.Find("turbeFull").GetComponent<Image>();
        AutoAssignWheels();
    }

    void Start()
    {
        PerusMaxAccerelation = MaxAcceleration;
        SmoothedMaxAcceleration = PerusMaxAccerelation;
        PerusTargetTorque = TargetTorque;
        if (CarRb == null)
            CarRb = GetComponent<Rigidbody>();
        CarRb.centerOfMass = _CenterofMass;
        if (GameManager.instance.sceneSelected != "tutorial")
        {
            CanDrift = true;
            CanUseTurbo = true;
        }
        BoostsForEachCar(carsParent: GameObject.Find("cars"));

        racerScript = FindAnyObjectByType<RacerScript>();

        InitializeLogitechWheel(); 
    }

    private void OnControlsChanged(PlayerInput input)
    {
        CurrentControlScheme = input.currentControlScheme;
    }


     // handlers
    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        steerInput = ctx.ReadValue<Vector2>().x;
    }
    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        steerInput = 0f;
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus){
            if (logitechInitialized && LogitechGSDK.LogiIsConnected(0))
            {
                LogitechGSDK.LogiUpdate();
            }
        }
    }


    private void OnEnable()
    {
        Controls.Enable();
        if (PlayerInput != null)
            PlayerInput.onControlsChanged += OnControlsChanged;

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

        // UNSUBSCRIBE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        Controls.CarControls.Move.performed -= OnMovePerformed;
        Controls.CarControls.Move.canceled  -= OnMoveCanceled;
        Controls.CarControls.Drift.performed -= OnDriftPerformed;
        Controls.CarControls.Drift.canceled  -= OnDriftCanceled;
        StopAllForceFeedback();
    }

    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();
        
        StopAllForceFeedback();
    }

    public new float GetSpeed()
    {
        GameManager.instance.carSpeed = CarRb.linearVelocity.magnitude * 3.6f;
        return CarRb.linearVelocity.magnitude * 3.6f;
    }

    public new float GetMaxSpeed()
    {
        return Maxspeed;
    }

    void Update()
    {
        GetInputs();
        Animatewheels();
        // detect connection state changes and print once when it changes
        bool currentlyConnected = logitechInitialized && LogitechGSDK.LogiIsConnected(0);
        if (currentlyConnected != lastLogiConnected)
        {
            lastLogiConnected = currentlyConnected;
            Debug.Log($"[CarController] Logitech connection status: {(currentlyConnected ? "Connected" : "Disconnected")}");
        }

        if (logitechInitialized && LogitechGSDK.LogiIsConnected(0))
        {
            LogitechGSDK.LogiUpdate();
            GetLogitechInputs();
            ApplyForceFeedback(); 
        }
    }


    void FixedUpdate()
    {
        float speed = CarRb.linearVelocity.magnitude * 3.6f;
        isOnGrassCachedValid = false;
        ApplySpeedLimit(speed);

        if (!IsCarActive()) return;
        
        UpdateDriftSpeed();
        StopDriftIfRaceFinished();

        ApplyGravity();
        Move();
        Steer();

        Decelerate();
        Applyturnsensitivity(speed);
        OnGrass();
        HandleTurbo();

        WheelEffects(IsDrifting);
    }

    bool IsCarActive()
    {
        // jos tulee random paskaa kuten peli paussi tai auto kolaroi, LISÄTKÄÄ SE TÄHÄN EI MISSÄN NIMESSÄ FIXEDUPDATEEN!!!!!!!!!!!
        return true;
    }

    void UpdateDriftSpeed()
    {
        if (!IsDrifting) return;


        if (IsTurboActive)
            Maxspeed = Mathf.Lerp(Maxspeed, Turbesped, Time.deltaTime * 0.5f);

        Maxspeed = Mathf.Lerp(Maxspeed, DriftMaxSpeed, Time.deltaTime * 0.1f);
    }

    void StopDriftIfRaceFinished()
    {
        if (racerScript == null) return;
        if (!racerScript.raceFinished) return;
        if (Activedrift <= 0) return;

        StopDrifting();
    }



    void GetInputs()
    {
        // Read steering from Move action (for keyboard/gamepad)
        if (!logitechInitialized || !LogitechGSDK.LogiIsConnected(0))
        {
            steerInput = Controls.CarControls.Move.ReadValue<Vector2>().x;
        }
        
        if (Controls.CarControls.MoveForward.IsPressed())
            moveInput = Controls.CarControls.MoveForward.ReadValue<float>();
        else if (Controls.CarControls.MoveBackward.IsPressed())
            moveInput = -Controls.CarControls.MoveBackward.ReadValue<float>();
        else
            moveInput = 0f;

        if (!Controls.CarControls.Drift.IsPressed())
            StopDrifting();

        //throttlemodifier = Controls.CarControls.ThrottleMod.ReadValue<float>();
    }

    bool IsWheelGrounded(Wheel wheel)
    {
        return Physics.Raycast(wheel.WheelCollider.transform.position, -wheel.WheelCollider.transform.up, out RaycastHit hit, wheel.WheelCollider.radius + wheel.WheelCollider.suspensionDistance);
    }

    bool IsWheelOnGrass(Wheel wheel)
    {
        if (Physics.Raycast(
                wheel.WheelCollider.transform.position,
                -wheel.WheelCollider.transform.up,
                out RaycastHit hit,
                wheel.WheelCollider.radius + wheel.WheelCollider.suspensionDistance))
        {
            // check if hit collider is on a grass layer
            return (Grass.value & (1 << hit.collider.gameObject.layer)) != 0;
        }
        return false;
    }





    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.02f);
    }


    void Applyturnsensitivity(float speed)
    {
        TurnSensitivty = Mathf.Lerp(
            TurnSensitivtyAtLowSpeed,
            TurnSensitivtyAtHighSpeed,
            Mathf.Clamp01(speed / Maxspeed));
    }


    void Move()
    {
        //HandeSteepSlope();
        UpdateTargetTorgue();
        AdjustSpeedForGrass();
        AdjustSuspension();
        foreach (var wheel in Wheels)
        {
            if (Controls.CarControls.Brake.IsPressed())
            {
                Brakes(wheel);
            }
            else
            {
                MotorTorgue(wheel);
            }
        }
    }

    //kommentoin tämän pois koska jos meille tulee se mistä aarre puhu eli autot menee ylös ja alas tiellä niin tämähän estää sen
    // private void HandeSteepSlope()
    // {
    //     if (IsOnSteepSlope())
    //     {
    //         targetTorque *= 0.5f;
    //         carRb.linearVelocity = Vector3.ClampMagnitude(carRb.linearVelocity, maxspeed / 3.6f);
    //     }
    // }

    private void AdjustSuspension()
    {
        foreach (var wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.WheelCollider.suspensionSpring;
            suspensionSpring.spring = 8000.0f;
            suspensionSpring.damper = 5000.0f;
            wheel.WheelCollider.suspensionSpring = suspensionSpring;
        }
    }

    private void AdjustForwardFrictrion()
    {
        foreach (var wheel in Wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.WheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteSlip = 1.0f;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 3.5f;
            wheel.WheelCollider.forwardFriction = forwardFriction;
        }
    }

    private void UpdateTargetTorgue()
    {
        float inputValue = CurrentControlScheme == "Gamepad"
            ? Controls.CarControls.ThrottleMod.ReadValue<float>()
            : Mathf.Abs(moveInput);

        float power = CurrentControlScheme == "Gamepad" ? 0.9f : 0.1f;

        float throttle = Mathf.Pow(inputValue, power);
        
        // Reduce power during drift but don't eliminate it
        float driftPowerMultiplier = IsDrifting ? 0.7f : 1.0f;
        float targetMaxAcc = PerusMaxAccerelation * Mathf.Lerp(0.4f, 1f, throttle) * driftPowerMultiplier;

        SmoothedMaxAcceleration = Mathf.MoveTowards(
            SmoothedMaxAcceleration,
            targetMaxAcc,
            Time.deltaTime * 250f
        );

        if (moveInput > 0f)
            TargetTorque = SmoothedMaxAcceleration;
        else if (moveInput < 0f)
            TargetTorque = -SmoothedMaxAcceleration;
        else
            TargetTorque = 0f;

        if (!IsDrifting)
        {
            float targetMaxSpeed = IsTurboActive ? Turbesped : BaseSpeed;
            Maxspeed = Mathf.Lerp(Maxspeed, targetMaxSpeed, Time.deltaTime);
        }
    }

    private void Brakes(Wheel wheel)
    {
        GameManager.instance.StopAddingPoints();
        wheel.WheelCollider.brakeTorque = BrakeAcceleration * 15f;
    }

    private void MotorTorgue(Wheel wheel)
    {
        wheel.WheelCollider.motorTorque = TargetTorque;
        wheel.WheelCollider.brakeTorque = 0f;
    }

    private void AdjustSpeedForGrass()
    {
        if (IsOnGrassCached() && !IsDrifting)
        {
            TargetTorque *= GrassSpeedMultiplier;
            Maxspeed = Mathf.Lerp(Maxspeed, GrassSpeedMultiplier, Time.deltaTime);
            if (GameManager.instance.carSpeed < 50.0f)
            {
                Maxspeed = 50.0f;
            }
        }
    }

    void Decelerate()
    {
        if (moveInput == 0)
        {
            Vector3 velocity = CarRb.linearVelocity;

            velocity -= velocity.normalized * Deceleration * 2.0f * Time.deltaTime;

            if (velocity.magnitude < 0.1f)
            {
                velocity = Vector3.zero;
            }
            CarRb.linearVelocity = velocity;
        }
    }

    float GetSteeringInput()
    {
        if (CurrentControlScheme == "Keyboard" && Mathf.Abs(steerInput) > 0f)
            return Mathf.Sign(steerInput) * Mathf.Pow(Mathf.Abs(steerInput), 0.8f);
        return steerInput;
    }


    
    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            CarRb.AddForce(Vector3.down * GravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }

    }




    public new float GetDriftSharpness()
    {
        if (IsDrifting)
        {
            Vector3 velocity = CarRb.linearVelocity;
            Vector3 forward = transform.forward;
            float angle = Vector3.Angle(forward, velocity);
            return angle;  
        }
        //checks the angle between the car's forward direction and its velocity vector constantly while drifting
        return 0.0f;
    }

    private void AdjustWheelsForDrift()
    {
        foreach (var wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.WheelCollider.suspensionSpring;
            suspensionSpring.spring = 4000.0f;
            suspensionSpring.damper = 1000.0f;
            wheel.WheelCollider.suspensionSpring = suspensionSpring;
        }

        foreach (var wheel in Wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.WheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.asymptoteSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 3f;
            wheel.WheelCollider.forwardFriction = forwardFriction;
            
            if (wheel.Axel == Axel.Front)
            {
                WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = 2.0f;
                wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }
    }

    void StopDrifting()
    {
        Activedrift = 0;
        IsDrifting = false;
        MaxAcceleration = PerusMaxAccerelation;
        CarRb.angularDamping = 0.05f; 

        if (racerScript != null &&
            (racerScript.raceFinished || GameManager.instance.carSpeed < 20.0f))
        {
            GameManager.instance.StopAddingPoints();
            return;
        }
        GameManager.instance.StopAddingPoints();

        foreach (var wheel in Wheels)
        {
            if (wheel.WheelCollider == null) continue;

            WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.asymptoteSlip = 0.4f;
            sidewaysFriction.extremumValue = 1.0f;
            sidewaysFriction.asymptoteValue = 1f;
            sidewaysFriction.stiffness = 5f;
            wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }



    void BoostsForEachCar(GameObject carsParent)
    {
        int childCount = carsParent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject car = carsParent.transform.GetChild(i).gameObject;
            if (car.activeInHierarchy)
            {
                string carName = car.name;
                switch (carName)
                {
                    //blue car  
                    case "REALCAR":
                        Turbepush = 15.0f;
                        ScoreManager.instance.SetScoreMultiplier(1.5f);
                        break;
                    //gray car
                    case "REALCAR_x":
                        TurbeMax = 75.0f;
                        TurbeRegen = 30.0f;
                        Turbepush = 20.0f;
                        break;
                    //purple car
                    case "REALCAR_y":
                        Turbepush = 7.0f;
                        ScoreManager.instance.SetScoreMultiplier(1.0f);
                        break;
                    //da Lada
                    case "Lada":
                        TurbeMax = 30.0f;
                        Turbepush = 500.0f;
                        ScoreManager.instance.SetScoreMultiplier(0.75f);
                        break;
                    default:
                        Debug.LogWarning($"Unknown car name: {carName}");
                        break;
                }
                CarTurboValues[carName] = Turbepush;
                return;
            }
        }
    }
    //bobbing effect


    //i hate this so much
    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        if (IsDrifting || GameManager.instance.isPaused || !CanDrift) return;

        Activedrift++;
        IsDrifting = true;

        MaxAcceleration = PerusMaxAccerelation * 0.6f;

        foreach (var wheel in Wheels)
        {
            if (wheel.WheelCollider == null) continue;
            WheelFrictionCurve sideways = wheel.WheelCollider.sidewaysFriction;
            sideways.extremumSlip   = 0.7f;
            sideways.asymptoteSlip  = 1.05f;
            sideways.extremumValue  = 1f;
            sideways.asymptoteValue = 1.2f;
            sideways.stiffness      = 2f;
            wheel.WheelCollider.sidewaysFriction = sideways;
        }

        CarRb.angularDamping = 0.01f;
        AdjustWheelsForDrift();
        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        StopDrifting();
        AdjustForwardFrictrion();
        MaxAcceleration = PerusMaxAccerelation;
        TargetTorque = PerusTargetTorque;
        WheelEffects(false);
    }




    [Header("Logitech G923 Settings")]
    public bool useLogitechWheel = true;
    public float forceFeedbackMultiplier = 1.0f;
    private bool logitechInitialized = false;
    private bool lastLogiConnected = false;

    void InitializeLogitechWheel()
    {
        if (!useLogitechWheel)
        {
            Debug.Log("[CarController] useLogitechWheel is false — skipping Logitech initialization.");
            return;
        }

        logitechInitialized = LogitechSDKManager.Initialize();
        if (logitechInitialized)
            Debug.Log("[CarController] Logitech wheel initialized successfully.");
        else
            Debug.LogWarning("[CarController] Logitech wheel failed to initialize.");
    }

    void GetLogitechInputs()
    {
        if (!LogitechGSDK.LogiIsConnected(0)) return;

        var state = LogitechGSDK.LogiGetStateUnity(0);
        steerInput = state.lX / 32768.0f;
        
        // Logitech pedals: -32768 (fully pressed) to 32767 (not pressed)
        // Invert and normalize to 0-1 range
        float throttle = Mathf.Clamp01(-state.lY / 32768.0f);
        
        // Clutch pedal for reverse - rglSlider is an array, index 0 is clutch
        float clutch = Mathf.Clamp01(-state.rglSlider[0] / 32768.0f);
        
        if (clutch > 0.1f)
            moveInput = -clutch;
        else if (throttle > 0.1f)
            moveInput = throttle;
        else
            moveInput = 0f;
    }

    void StopAllForceFeedback()
    {
        if (!logitechInitialized || !LogitechGSDK.LogiIsConnected(0)) return;
        
        LogitechGSDK.LogiStopDirtRoadEffect(0);
    }

    void ApplyForceFeedback()
    {
        if (!LogitechSDKManager.IsReady) return;

        if (GameManager.instance.isPaused)
        {
            LogitechGSDK.LogiStopDirtRoadEffect(0);
            return;
        }
        if (!logitechInitialized || !LogitechGSDK.LogiIsConnected(0)) return;
        
        float speed = CarRb.linearVelocity.magnitude * 3.6f;

        // Continuously apply spring force (centering)
        int springStrength = Mathf.RoundToInt(40 * forceFeedbackMultiplier);
        LogitechGSDK.LogiPlaySpringForce(0, 0, 100, springStrength);

        // Continuously apply damper force (resistance) for steering
        int damperStrength = Mathf.RoundToInt(10 * forceFeedbackMultiplier);
        LogitechGSDK.LogiPlayDamperForce(0, damperStrength);
        
        // Dirt road only when on grass and moving
        if (IsOnGrassCached() && speed >= 10)
            LogitechGSDK.LogiPlayDirtRoadEffect(0, Mathf.RoundToInt(12.5f * forceFeedbackMultiplier));
        else
            LogitechGSDK.LogiStopDirtRoadEffect(0);
    }
}
