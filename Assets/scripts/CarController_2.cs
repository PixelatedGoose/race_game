using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class CarController_2 : MonoBehaviour
{
    //#pragma warning disable CS0618
    
    CarInputActions Controls;

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
    public float maxAcceleration = 300.0f;
    public float brakeAcceleration = 3.0f;
    [Header("turn asetukset")]
    public float turnSensitivty = 1.0f;
    public float turnSensitivtyAtHighSpeed = 1.0f;
    public float turnSensitivtyAtLowSpeed = 1.0f;
    public float deceleration = 1.0f;
    [Min (100.0f)] 
    public float maxspeed = 100.0f;
    public float gravityMultiplier = 1.5f; 
    public float grassSpeedMultiplier = 0.5f;
    public List<Wheel> wheels; 
    float moveInput;
    float steerInput;
    public Vector3 _centerofMass;
    public LayerMask grass;
    public float targetTorque;
    public Material grassMaterial;
    public Material roadMaterial;
    public Material driftmaterial;
    public Rigidbody carRb;
    bool isTurboActive = false;
    private float activedrift = 0.0f;
    public float Turbesped = 150.0f;
    public float basespeed = 100.0f;
    public float grassmaxspeed = 50.0f;
    [Header("Drift asetukset")]
    public float driftMultiplier = 1.0f;
    public bool isTurnedDown = false;
    

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

        if (carRb == null)
            carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerofMass;
        AdjustTurboForEachCar(carsParent: GameObject.Find("cars"));
    }

    private void AdjustTurboForEachCar(object carsParent)
    {
        throw new NotImplementedException();
    }

    private void OnEnable()
    {
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable(); 
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

    void FixedUpdate()
    {

        // Stop drifting if the race is finished
        RacerScript racerScript = FindAnyObjectByType<RacerScript>();
        if (racerScript != null && racerScript.raceFinished && activedrift > 0)
        {
            StopDrifting();
        }
        ApplyGravity();
        Move();
        Steer();
        HandleDrift();
        Decelerate();
        ApplySpeedLimit();
        Applyturnsensitivity(); 
        OnGrass();
        TURBE();
        TURBEmeter();
        
    }

    void OnGrass()
    {        
        TrailRenderer trailRenderer = null;
        foreach (var wheel in wheels)
        {
            trailRenderer = wheel.wheelEffectobj.GetComponentInChildren<TrailRenderer>();
            if (IsOnGrass())
            {
                trailRenderer.material = grassMaterial;
                GameManager.instance.scoreAddWT = 0.10f;
            }
            else
            {
                trailRenderer.material = roadMaterial;
                GameManager.instance.scoreAddWT = 0.01f;
            }
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
         //moveInput = Input.GetAxis("Vertical");
         //steerInput = Input.GetAxis("Horizontal");

        Controls.CarControls.Move.performed += ctx => {
            steerInput = ctx.ReadValue<Vector2>().x;
        };

        if(Controls.CarControls.MoveForward.IsPressed()) {
            moveInput = Controls.CarControls.MoveForward.ReadValue<float>();
        }

        if(Controls.CarControls.MoveBackward.IsPressed()) {
            moveInput = -Controls.CarControls.MoveBackward.ReadValue<float>();
        }

        if(!Controls.CarControls.MoveBackward.IsPressed() && !Controls.CarControls.MoveForward.IsPressed()) {
            moveInput = 0.0f;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 500 * 1.0f);
    }

    bool IsWheelGrounded(Wheel wheel)
    {
        return Physics.Raycast(wheel.wheelCollider.transform.position, -wheel.wheelCollider.transform.up, out RaycastHit hit, wheel.wheelCollider.radius + wheel.wheelCollider.suspensionDistance);
    }

    bool IsWheelOnGrass(Wheel wheel)
    {
        if (Physics.Raycast(wheel.wheelCollider.transform.position, -wheel.wheelCollider.transform.up, out RaycastHit hit, wheel.wheelCollider.radius + wheel.wheelCollider.suspensionDistance))
        {
            return ((1 << hit.collider.gameObject.layer) & grass) != 0;
        }
        return false;
    }

    bool IsOnGrass()
    {
        foreach (var wheel in wheels)
        {
            if (!IsWheelGrounded(wheel))
            {
                return false;
            }
            if (!IsWheelOnGrass(wheel))
            {
                return false;
            }
        }
        return true;
    }

    void ApplySpeedLimit()
    {
        float speed = carRb.linearVelocity.magnitude * 3.6f; 
        if (speed > maxspeed)
        {
            carRb.linearVelocity = carRb.linearVelocity.normalized * (maxspeed / 3.6f);
        }
    }

    void Applyturnsensitivity()
    {
        float speed = carRb.linearVelocity.magnitude * 3.6f;
        
        turnSensitivty = Mathf.Lerp(turnSensitivtyAtLowSpeed, turnSensitivtyAtHighSpeed, Mathf.Clamp01(speed / maxspeed));
    }

    void TURBE()
    {
        if (!canUseTurbo)
        {
            return;
        }

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
        HandeSteepSlope();
        UpdateTargetTorgue();
        AdjustSpeedForGrass();
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

    private void HandeSteepSlope()
    {
        if (IsOnSteepSlope())
        {
            targetTorque *= 0.5f;
            carRb.linearVelocity = Vector3.ClampMagnitude(carRb.linearVelocity, maxspeed / 3.6f);
        }
    }
    private void AdjustSuspension()
    {
        foreach (var wheel in wheels)
        {
            JointSpring suspensionSpring = wheel.wheelCollider.suspensionSpring;
            suspensionSpring.spring = 8000.0f;
            suspensionSpring.damper = 2000.0f;
            wheel.wheelCollider.suspensionSpring = suspensionSpring;
        }
    }
    private void AdjustForwardFrictrion()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteSlip = 0.8f;
            forwardFriction.asymptoteValue = 1;
        }

    }

    private void UpdateTargetTorgue()
    {
        if (moveInput > 0)
        {
            targetTorque = 1 * maxAcceleration;
        }
        else if (moveInput < 0)
        {
            targetTorque = -1 * maxAcceleration;
        }
        else
        {
            targetTorque = 0.0f;
        }
        ;
        maxspeed = Mathf.Lerp(maxspeed, isTurboActive ? Turbesped : basespeed, Time.deltaTime);
    }

    private void Brakes(Wheel wheel)
    {
        GameManager.instance.StopAddingPoints();
        wheel.wheelCollider.brakeTorque = brakeAcceleration * 1000f;
        wheel.wheelCollider.motorTorque = 0f;
    }

    private void MotorTorgue(Wheel wheel)
    {
        wheel.wheelCollider.motorTorque = targetTorque;
        wheel.wheelCollider.brakeTorque = 0f;
    }

    private void AdjustSpeedForGrass()
    {
        if (IsOnGrass())
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

            velocity -= velocity.normalized * deceleration * 2.0f *  Time.deltaTime;
            
            if (velocity.magnitude < 0.1f) 
            {
                velocity = Vector3.zero;
            }
        carRb.linearVelocity = velocity;
        }
    }

    void Steer() 
    { 
        foreach(var wheel in wheels)
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
        carRb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
    }

    void HandleDrift()
    {
        if (!canDrift)
        {
            return;
        }
        Controls.CarControls.Drift.performed += ctx => {
            RacerScript racerScript = FindAnyObjectByType<RacerScript>();
            if (activedrift > 0)
            {
                return;
            }
            activedrift++;

            float speed = carRb.linearVelocity.magnitude * 3.6f;
            float speedFactor = Mathf.Clamp(maxspeed / 100.0f, 0.5f, 2.0f); 
            float driftMultiplier = 1.0f;

            foreach (var wheel in wheels)
            {
                if (wheel.wheelCollider == null) continue;

                WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
                sidewaysFriction.extremumSlip = 1.5f * speedFactor * driftMultiplier;
                sidewaysFriction.asymptoteSlip = 2.0f * speedFactor * driftMultiplier;
                sidewaysFriction.extremumValue = 0.5f / (speedFactor * driftMultiplier);
                sidewaysFriction.asymptoteValue = 0.75f / (speedFactor * driftMultiplier);
                wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
                
            }
            AdjustdriftSuspension();
            AdjustDriftForwardFriction();

            if (speed > 20.0f)
            {
                GameManager.instance.AddPoints();
            }

            WheelEffects(true);
        };
        Controls.CarControls.Drift.canceled += ctx => {
            StopDrifting();
            WheelEffects(false);
        };        
    }
    private void AdjustdriftSuspension()
    {
        foreach (var wheel in wheels)
        {
            JointSpring suspensionSpring = wheel.wheelCollider.suspensionSpring;
            suspensionSpring.spring = 4000.0f;
            suspensionSpring.damper = 1000.0f;
            wheel.wheelCollider.suspensionSpring = suspensionSpring;
         }

    }

    private void AdjustDriftForwardFriction()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.2f;
            forwardFriction.asymptoteSlip = 0.5f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteValue = 1;
            wheel.wheelCollider.forwardFriction = forwardFriction;
        }
    }

    void StopDrifting()
    {
        activedrift = 0;
        RacerScript racerScript = FindAnyObjectByType<RacerScript>();
        if (racerScript != null && racerScript.raceFinished || GameManager.instance.carSpeed < 20.0f)
        {
            GameManager.instance.StopAddingPoints();
            return;
        }
        GameManager.instance.StopAddingPoints();
        AdjustSuspension();
        AdjustForwardFrictrion();
        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider == null) continue;

            WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.asymptoteSlip = 0.5f;
            sidewaysFriction.extremumValue = 1.0f;
            sidewaysFriction.asymptoteValue = 1f;
            wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
        }    
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        GameManager.instance.AddPoints();
    }

    void Animatewheels()
    {
        foreach(var wheel in wheels) 
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
        Controls.CarControls.Move.canceled+= ctx => {
            steerInput = 0.0f;
        };   
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
                        turbepush = 50.0f;
                        break;
                    case "REALCAR_x":
                        turbepush  = 10.0f;
                        break;
                    case "REALCAR_y":
                        turbepush = 70.0f;
                        break;
                    case "Lada":
                        turbepush = 50.0f;
                        break;
                    default:
                        Debug.LogWarning($"Unknown car name: {carName}");
                        break;
                }
                carTurboValues[carName] = turbepush;
                Debug.Log($"Turbo set for active car: {carName} = {turbepush}");
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
        if (!canUseTurbo)
        {
            return;
        }
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
        switch(option)
        {
            case "start":
                StartCoroutine("turbeRegenerate");
                break;

            case "stop":
                StopCoroutine("turbeRegenerate");
                break;
        }
    }
}