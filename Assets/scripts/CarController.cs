using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using NUnit.Framework;

public class CarController : MonoBehaviour
{

    CarInputActions Controls;

    RacerScript racerScript;

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;

        public GameObject wheelEffectobj;
        public ParticleSystem smokeParticle;
        public Axel axel;
    }

    [Header("Auton asetukset")]
    public float maxAcceleration = 700.0f;
    public float brakeAcceleration = 3.0f;
    [Header("turn asetukset")]
    public float turnSensitivty = 1.0f;
    public float turnSensitivtyAtHighSpeed = 1.0f, turnSensitivtyAtLowSpeed = 1.0f;
    public float deceleration = 1.0f;
    [Min(100.0f)]
    public float maxspeed = 100.0f;
    public float gravityMultiplier = 1.5f;
    public float grassSpeedMultiplier = 0.5f;
    public List<Wheel> wheels;
    float moveInput, steerInput;
    public Vector3 _centerofMass;
    public LayerMask grass;
    public float targetTorque;
    public Material grassMaterial, roadMaterial, driftmaterial;
    public Rigidbody carRb;
    public bool isTurboActive = false;
    private float activedrift = 0.0f;
    public float Turbesped = 150.0f, basespeed = 100.0f, grassmaxspeed = 50.0f, driftMaxSpeed = 40f;
    [Header("Drift asetukset")]
    public float driftMultiplier = 1.0f;
    public bool isTurnedDown = false, isDrifting;
    private float perusMaxAccerelation, perusTargetTorque, throttlemodifier, smoothedMaxAcceleration, modifiedMaxAcceleration;
    //fuck this shit im doing this controller keyboard the fucking lazy/shit way!!!!!!!!!!
    [SerializeField] private PlayerInput playerInput;
    private string currentControlScheme = "Keyboard";


    [Header("turbe asetukset")]
    public Image turbeMeter;
    public float turbeAmount = 100.0f, turbeMax = 100.0f, turbepush = 50.0f;
    public float turbeReduce;
    public float turbeRegen;


    public bool isRegenerating = false;
    public int turbeRegenCoroutineAmount = 0;
    Dictionary<string, float> carTurboValues = new Dictionary<string, float>();

    public bool canDrift = false;
    public bool canUseTurbo = false;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
        turbeMeter = GameObject.Find("turbeFull").GetComponent<Image>();
    }

    void Start()
    {
        perusMaxAccerelation = maxAcceleration;
        smoothedMaxAcceleration = perusMaxAccerelation;
        perusTargetTorque = targetTorque;
        if (carRb == null)
            carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerofMass;
        if (GameManager.instance.sceneSelected != "tutorial")
        {
            canDrift = true;
            canUseTurbo = true;
        }
        AdjustTurboForEachCar(carsParent: GameObject.Find("cars"));

        racerScript = FindAnyObjectByType<RacerScript>();
    }

    private void OnControlsChanged(PlayerInput input)
    {
        currentControlScheme = input.currentControlScheme;
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


    private void OnEnable()
    {
        Controls.Enable();
        if (playerInput != null)
            playerInput.onControlsChanged += OnControlsChanged;

        // INPUT SUBSCRIPTIONS: KERRAN
        Controls.CarControls.Move.performed += OnMovePerformed;
        Controls.CarControls.Move.canceled  += OnMoveCanceled;

        Controls.CarControls.Drift.performed   += OnDriftPerformed;
        Controls.CarControls.Drift.canceled    += OnDriftCanceled;
    }

    private void OnDisable()
    {
        Controls.Disable();
        if (playerInput != null)
            playerInput.onControlsChanged -= OnControlsChanged;

        // UNSUBSCRIBE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        Controls.CarControls.Move.performed -= OnMovePerformed;
        Controls.CarControls.Move.canceled  -= OnMoveCanceled;
        Controls.CarControls.Drift.performed -= OnDriftPerformed;
        Controls.CarControls.Drift.canceled  -= OnDriftCanceled;
    }

    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();
    }

    public float GetSpeed()
    {
        GameManager.instance.carSpeed = carRb.linearVelocity.magnitude * 3.6f;
        return carRb.linearVelocity.magnitude * 3.6f;
    }

    public float GetMaxSpeed()
    {
        return maxspeed;
    }

    void Update()
    {
        GetInputs();
        Animatewheels();
    }

    bool isOnGrassCached;
    bool isOnGrassCachedValid;

    void FixedUpdate()
    {
        float speed = carRb.linearVelocity.magnitude * 3.6f;
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
    }

    bool IsCarActive()
    {
        // jos tulee random paskaa kuten peli paussi tai auto kolaroi, LISÄTKÄÄ SE TÄHÄN EI MISSÄÄN NIMESSÄ FIXEDUPDATEEN!!!!!!!!!!!
        return true;
    }

    void UpdateDriftSpeed()
    {
        if (!isDrifting) return;

        float sharpness = GetDriftSharpness();
        //Debug.Log("Drift Sharpness: " + sharpness);

        if (isTurboActive)
            maxspeed = Mathf.Lerp(maxspeed, Turbesped, Time.deltaTime * 0.5f);

        maxspeed = Mathf.Lerp(maxspeed, driftMaxSpeed, Time.deltaTime * 0.1f);
    }

    void StopDriftIfRaceFinished()
    {
        if (racerScript == null) return;
        if (!racerScript.raceFinished) return;
        if (activedrift <= 0) return;

        StopDrifting();
    }



    void HandleTurbo()
    {
        if (!canUseTurbo) return;
        TURBE();
        TURBEmeter();
    }

        void OnGrass()
        {
            int wheelsOnGrass = 0;

            foreach (var wheel in wheels)
            {
                if (wheel.wheelEffectobj == null) continue;

                var trailRenderer = wheel.wheelEffectobj.GetComponentInChildren<TrailRenderer>();
                if (trailRenderer == null) continue;

                bool wheelOnGrass = IsWheelGrounded(wheel) && IsWheelOnGrass(wheel);

                // per‑wheel line material
                trailRenderer.material = wheelOnGrass ? grassMaterial : roadMaterial;

                if (wheelOnGrass)
                    wheelsOnGrass++;
            }

            // 1 oli liian kiree pisteen vähenemiseen, 2 on parempi
            const int wheelsNeededForPenalty = 2;   

            bool onGrassForScore = wheelsOnGrass >= wheelsNeededForPenalty;

            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.SetOnGrass(onGrassForScore);
            }
        }


    private bool IsOnSteepSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle > 30.0f;
        }
        return false;
    }

    void GetInputs()
    {
        if (Controls.CarControls.MoveForward.IsPressed())
            moveInput = Controls.CarControls.MoveForward.ReadValue<float>();
        else if (Controls.CarControls.MoveBackward.IsPressed())
            moveInput = -Controls.CarControls.MoveBackward.ReadValue<float>();
        else
            moveInput = 0f;

        if (!Controls.CarControls.Drift.IsPressed())
            StopDrifting();

        throttlemodifier = Controls.CarControls.ThrottleMod.ReadValue<float>();
    }

    bool IsWheelGrounded(Wheel wheel)
    {
        return Physics.Raycast(wheel.wheelCollider.transform.position, -wheel.wheelCollider.transform.up, out RaycastHit hit, wheel.wheelCollider.radius + wheel.wheelCollider.suspensionDistance);
    }

    bool IsWheelOnGrass(Wheel wheel)
    {
        if (Physics.Raycast(
                wheel.wheelCollider.transform.position,
                -wheel.wheelCollider.transform.up,
                out RaycastHit hit,
                wheel.wheelCollider.radius + wheel.wheelCollider.suspensionDistance))
        {
            // check if hit collider is on a grass layer
            return (grass.value & (1 << hit.collider.gameObject.layer)) != 0;
        }
        return false;
    }


    public bool IsOnGrass()
    {
        foreach (var wheel in wheels)
        {
            // car is "on grass" if ANY wheel is grounded AND on a grass collider
            if (IsWheelGrounded(wheel) && IsWheelOnGrass(wheel))
            {
                return true;
            }
        }
        return false;
    }

    bool IsOnGrassCached()
    {
        if (!isOnGrassCachedValid)
        {
            isOnGrassCached = IsOnGrass();
            isOnGrassCachedValid = true;
        }
        return isOnGrassCached;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.02f);
    }

    void ApplySpeedLimit(float speed)
    {
        if (speed <= maxspeed) return;
        carRb.linearVelocity = carRb.linearVelocity.normalized * (maxspeed / 3.6f);
    }

    void Applyturnsensitivity(float speed)
    {
        turnSensitivty = Mathf.Lerp(
            turnSensitivtyAtLowSpeed,
            turnSensitivtyAtHighSpeed,
            Mathf.Clamp01(speed / maxspeed));
    }

    void TURBE()
    {
        //uskon että tää on tarpeeton; viittauksia KOMMENTEISSA yhessä scriptis, ei missää muualla
        //tätä ei myöskää muuteta koskaan...
        if (isTurnedDown)
        {
            isTurboActive = false;
            return;
        }
        isTurboActive = Controls.CarControls.turbo.IsPressed() && turbeAmount > 0;
        if (isTurboActive)
        {
            carRb.AddForce(transform.forward * turbepush, ForceMode.Acceleration);
            targetTorque *= 1.5f;
        }
    }

    void Move()
    {
        if (activedrift > 0) return;
        //HandeSteepSlope();
        UpdateTargetTorgue();
        AdjustSpeedForGrass();
        AdjustSuspension();
        foreach (var wheel in wheels)
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
        foreach (var wheel in wheels)
        {
            JointSpring suspensionSpring = wheel.wheelCollider.suspensionSpring;
            suspensionSpring.spring = 8000.0f;
            suspensionSpring.damper = 5000.0f;
            wheel.wheelCollider.suspensionSpring = suspensionSpring;
        }
    }
    private void AdjustForwardFrictrion()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteSlip = 1.0f;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 5f;
            wheel.wheelCollider.forwardFriction = forwardFriction;
        }
    }

    private void UpdateTargetTorgue()
    {
        if (activedrift > 0) return;

        float inputValue = currentControlScheme == "Gamepad"
            ? Controls.CarControls.ThrottleMod.ReadValue<float>()
            : Mathf.Abs(moveInput);

        float power = currentControlScheme == "Gamepad" ? 0.9f : 0.1f;

        float throttle = Mathf.Pow(inputValue, power);
        float targetMaxAcc = perusMaxAccerelation * Mathf.Lerp(0.4f, 1f, throttle);

        smoothedMaxAcceleration = Mathf.MoveTowards(
            smoothedMaxAcceleration,
            targetMaxAcc,
            Time.deltaTime * 250f
        );

        if (moveInput > 0f)
            targetTorque =  smoothedMaxAcceleration;
        else if (moveInput < 0f)
            targetTorque = -smoothedMaxAcceleration;
        else
            targetTorque = 0f;

        if (!isDrifting)
        {
            float targetMaxSpeed = isTurboActive ? Turbesped : basespeed;
            maxspeed = Mathf.Lerp(maxspeed, targetMaxSpeed, Time.deltaTime);
        }
    }

    private void Brakes(Wheel wheel)
    {
        GameManager.instance.StopAddingPoints();
        wheel.wheelCollider.brakeTorque = brakeAcceleration * 15f;
    }

    private void MotorTorgue(Wheel wheel)
    {
        wheel.wheelCollider.motorTorque = targetTorque;
        wheel.wheelCollider.brakeTorque = 0f;
    }

    private void AdjustSpeedForGrass()
    {
        if (IsOnGrassCached() && !isDrifting)
        {
            targetTorque *= grassSpeedMultiplier;
            maxspeed = Mathf.Lerp(maxspeed, grassSpeedMultiplier, Time.deltaTime);
            if (GameManager.instance.carSpeed < 50.0f)
            {
                maxspeed = 50.0f;
            }
        }
    }

    void Decelerate()
    {
        if (moveInput == 0)
        {
            Vector3 velocity = carRb.linearVelocity;

            velocity -= velocity.normalized * deceleration * 2.0f * Time.deltaTime;

            if (velocity.magnitude < 0.1f)
            {
                velocity = Vector3.zero;
            }
            carRb.linearVelocity = velocity;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivty;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            carRb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }

    }


    void HandleDrift()
    {
        Controls.CarControls.Drift.performed += ctx =>
        {


            if (isDrifting || GameManager.instance.isPaused) return;
            RacerScript racerScript = FindAnyObjectByType<RacerScript>();
            //varmistaa että drift tapahtuu
            activedrift++;
            isDrifting = true;
            float sharpness = GetDriftSharpness();
            Debug.Log("Drift Sharpness: " + sharpness);
            // arvot vaihtuu ja huonotuu driftin ajaksi
            maxAcceleration = perusMaxAccerelation * 0.7f;
            float speedFactor = Mathf.Clamp(maxspeed / 100.0f, 0.5f, 2.0f);
            foreach (var wheel in wheels)
            {
                if (wheel.wheelCollider == null) continue;

                WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
                sidewaysFriction.extremumSlip = 2.0f * speedFactor * driftMultiplier;
                sidewaysFriction.asymptoteSlip = 2.5f * speedFactor * driftMultiplier;
                sidewaysFriction.extremumValue = 0.75f / (speedFactor * driftMultiplier);
                sidewaysFriction.asymptoteValue = 0.75f / (speedFactor * driftMultiplier);
                sidewaysFriction.stiffness = 3f;
                wheel.wheelCollider.sidewaysFriction = sidewaysFriction;

            }
            //laittaa jousitukset driftiä varten
            AdjustWheelsForDrift();
            WheelEffects(true);
        };
        Controls.CarControls.Drift.canceled += ctx =>
        {
            StopDrifting();
            AdjustForwardFrictrion();
            maxAcceleration = perusMaxAccerelation;
            targetTorque = perusTargetTorque;
            WheelEffects(false);
        };
    }

    public float GetDriftSharpness()
    {
        if (isDrifting)
        {
            Vector3 velocity = carRb.linearVelocity;
            Vector3 forward = transform.forward;
            float angle = Vector3.Angle(forward, velocity);
            return angle;  
        }
        //checks the angle between the car's forward direction and its velocity vector constantly while drifting
        return 0.0f;
    }

    private void AdjustWheelsForDrift()
    {
        foreach (var wheel in wheels)
        {
            JointSpring suspensionSpring = wheel.wheelCollider.suspensionSpring;
            suspensionSpring.spring = 4000.0f;
            suspensionSpring.damper = 1000.0f;
            wheel.wheelCollider.suspensionSpring = suspensionSpring;
        }

        foreach (var wheel in wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.asymptoteSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 3f;
            wheel.wheelCollider.forwardFriction = forwardFriction;
        }

    }

    void StopDrifting()
    {
        activedrift = 0;
        isDrifting = false;
        maxAcceleration = perusMaxAccerelation;

        if (racerScript != null &&
            (racerScript.raceFinished || GameManager.instance.carSpeed < 20.0f))
        {
            GameManager.instance.StopAddingPoints();
            return;
        }
        GameManager.instance.StopAddingPoints();


        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider == null) continue;

            WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.asymptoteSlip = 0.4f;
            sidewaysFriction.extremumValue = 1.0f;
            sidewaysFriction.asymptoteValue = 1f;
            sidewaysFriction.stiffness = 5f;
            wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }

    void Animatewheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }


    void AdjustTurboForEachCar(GameObject carsParent)
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
                    case "REALCAR":
                        turbepush = 15.0f;
                        break;
                    case "REALCAR_x":
                        turbepush = 10.0f;
                        break;
                    case "REALCAR_y":
                        turbepush = 7.0f;
                        break;
                    case "Lada":
                        turbepush = 50.0f;
                        break;
                    default:
                        Debug.LogWarning($"Unknown car name: {carName}");
                        break;
                }
                carTurboValues[carName] = turbepush;
                return;
            }
        }
    }
    //bobbing effect

    /// <summary>
    /// does wheel effects
    /// </summary>
    void WheelEffects(bool enable)
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear)
            {
                var trailRenderer = wheel.wheelEffectobj.GetComponentInChildren<TrailRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.emitting = enable;
                }
                if (wheel.smokeParticle != null)
                {
                    if (enable)
                        wheel.smokeParticle.Play();
                    else
                        wheel.smokeParticle.Stop();
                }
            }
        }
    }

    /// <summary>
    /// käytetään TURBEmeterin päivittämiseen joka frame
    /// </summary>
    void TURBEmeter()
    {
        if (isTurboActive && turbeAmount != 0) //jos käytät turboa ja sitä o jäljellä
        {
            GameManager.instance.turbeActive = true;

            if (turbeRegenCoroutineAmount > 0)
            {
                turbeRegenCoroutines("stop");
            }
            isRegenerating = false;
            turbeRegenCoroutineAmount = 0;

            turbeAmount -= turbeReduce * Time.deltaTime;
        }
        else if (!isTurboActive && turbeAmount < turbeMax) //jos et käytä turboa ja se ei oo täynnä
        {

            GameManager.instance.turbeActive = false;

            if (turbeRegenCoroutineAmount == 0 && isRegenerating == false)
            {
                turbeRegenCoroutines("start");
                turbeRegenCoroutineAmount += 1;
            }
        }

        if (turbeAmount < 0)
        {
            turbeAmount = 0;
        }
        if (turbeAmount > turbeMax)
        {
            //Debug.Log("I bought a property in Egypt, and what they do is they give you the property");
            turbeAmount = turbeMax;

            turbeRegenCoroutines("stop");
            isRegenerating = false;
            turbeRegenCoroutineAmount = 0;
        }

        turbeMeter.fillAmount = turbeAmount / turbeMax;
    }

    /// <summary>
    /// käytetään TURBEn regeneroimiseen
    /// ...koska fuck C#
    /// </summary>
    private IEnumerator turbeRegenerate()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        isRegenerating = true;

        if (isRegenerating && turbeRegenCoroutineAmount == 1)
        {
            while (isRegenerating && turbeRegenCoroutineAmount == 1)
            {
                yield return StartCoroutine(RegenerateTurbeAmount());
            }
        }
        else
        {
            Debug.Log("stopped regen coroutine");
            yield break;
            //scriptin ei pitäs päästä tähä tilanteeseen missään vaiheessa, mutta se on täällä varmuuden vuoksi
        }
    }

    private IEnumerator RegenerateTurbeAmount()
    {
        turbeAmount += turbeRegen * Time.deltaTime;
        yield return null; // Wait for the next frame
    }

    /// <summary>
    /// aloita tai pysäytä TURBEn regenerointi coroutine
    /// </summary>
    /// <param name="option">start / stop</param>
    private void turbeRegenCoroutines(string option)
    {
        switch (option)
        {
            case "start":
                StartCoroutine("turbeRegenerate");
                break;

            case "stop":
                StopCoroutine("turbeRegenerate");
                break;
        }
    }


    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        if (isDrifting || GameManager.instance.isPaused || !canDrift) return;

        activedrift++;
        isDrifting = true;

        maxAcceleration = perusMaxAccerelation * 0.7f;

        float speed = carRb.linearVelocity.magnitude * 3.6f;
        float speedFactor = Mathf.Clamp(maxspeed / 100.0f, 0.5f, 2.0f);
        float driftMultiplier = 1.0f;

        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider == null) continue;
            WheelFrictionCurve sideways = wheel.wheelCollider.sidewaysFriction;
            sideways.extremumSlip   = 2.0f * speedFactor * driftMultiplier;
            sideways.asymptoteSlip  = 2.5f * speedFactor * driftMultiplier;
            sideways.extremumValue  = 0.75f / (speedFactor * driftMultiplier);
            sideways.asymptoteValue = 0.75f / (speedFactor * driftMultiplier);
            sideways.stiffness      = 3f;
            wheel.wheelCollider.sidewaysFriction = sideways;
        }

        AdjustWheelsForDrift();
        WheelEffects(true);
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        StopDrifting();
        AdjustForwardFrictrion();
        maxAcceleration = perusMaxAccerelation;
        targetTorque = perusTargetTorque;
        WheelEffects(false);
    }
}