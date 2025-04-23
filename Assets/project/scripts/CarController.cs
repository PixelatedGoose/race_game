using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class CarController : MonoBehaviour
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

    [Header("turbe asetukset")]
    public Image turbeMeter;
    public float turbeAmount = 100.0f, turbeMax = 100.0f;
    public float turbeReduce;
    public float turbeRegen;

    public bool isRegenerating = false;
    public int turbeRegenCoroutineAmount = 0;



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
        WheelEffects();
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
        //AdjustWheelFriction();
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
                GameManager.instance.scoreAddWT = 0.08f;
            }
            else
            {
                trailRenderer.material = roadMaterial;
                GameManager.instance.scoreAddWT = 0.01f;
            }
        }
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
        isTurboActive = Controls.CarControls.turbo.IsPressed() && turbeAmount > 0;
        if (isTurboActive)
        {
            carRb.AddForce(transform.forward * 50f, ForceMode.Acceleration);
            targetTorque *= 1.5f;
        }
    }

    void Move()
    {
        // targetTorque = moveInput * maxAcceleration;

        if(moveInput > 0) {
            targetTorque = 2 * maxAcceleration;
        } else if (moveInput < 0) {
            targetTorque = -2 * maxAcceleration;
        } else {
            targetTorque = 0.0f;
        };
        maxspeed = Mathf.Lerp(maxspeed, isTurboActive ? Turbesped : basespeed, Time.deltaTime);

        if (IsOnGrass())
        {
            targetTorque *= grassSpeedMultiplier;
            maxspeed = Mathf.Lerp(maxspeed, grassSpeedMultiplier, Time.deltaTime);
            if (GameManager.instance.carSpeed < 50.0f)
            {
                maxspeed = 50.0f;
            }
            
        }

        foreach (var wheel in wheels)
        {
            if (Controls.CarControls.Brake.IsPressed())
            {
                GameManager.instance.StopAddingPoints();
                wheel.wheelCollider.brakeTorque = brakeAcceleration * 1000f;
                wheel.wheelCollider.motorTorque = 0f;
            }
            else if (moveInput != 0)
            {
                wheel.wheelCollider.motorTorque = targetTorque;
                wheel.wheelCollider.brakeTorque = 0f;
            }
            else
            {
                wheel.wheelCollider.motorTorque = wheel.wheelCollider.brakeTorque = 0f;
            }
        }
    }

    void Decelerate()
    {
        if (moveInput == 0)
        {
            Vector3 velocity = carRb.linearVelocity;
            if (IsGrounded())
            {
                velocity -= velocity.normalized * deceleration * Time.deltaTime;
            }
            if (velocity.magnitude < 0.1f) 
            {
                velocity = Vector3.zero;
            }
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
        if (!IsGrounded())
        {
            
            carRb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }
    }
    void HandleDrift()
    {
        Controls.CarControls.Drift.performed += ctx => {
            RacerScript racerScript = FindAnyObjectByType<RacerScript>();
            if (racerScript != null && racerScript.raceFinished)
            {
                return;
            }

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

            if (speed > 20.0f) 
            {
                GameManager.instance.AddPoints();
            }
        };

        Controls.CarControls.Drift.canceled += ctx => {
            StopDrifting();
        };
    }

    void StopDrifting()
    {
        activedrift = 0;

        // Stop adding points if the race is finished
        RacerScript racerScript = FindAnyObjectByType<RacerScript>();
        if (racerScript != null && racerScript.raceFinished || GameManager.instance.carSpeed < 20.0f)
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
            sidewaysFriction.asymptoteSlip = 0.5f;
            sidewaysFriction.extremumValue = 1.0f;
            sidewaysFriction.asymptoteValue = 1f;
            wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
        }
        Controls.CarControls.Move.performed -= OnMovePerformed;
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
    //bobbing effect

    /// <summary>
    /// does wheel effects
    /// </summary>
    void WheelEffects()
    {
        foreach(var wheel in wheels)
        {
            var trailRenderer = wheel.wheelEffectobj.GetComponentInChildren<TrailRenderer>();
            if (Controls.CarControls.Drift.IsPressed() && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded && carRb.linearVelocity.magnitude >= 10.0f)
            {
                trailRenderer.emitting = true;
                wheel.smokeParticle.Emit(1);
                if (IsWheelOnGrass(wheel))
                {
                    trailRenderer.material = grassMaterial;
                }
                else
                {
                    trailRenderer.material = driftmaterial;
                }
            }
            else
            {
                trailRenderer.emitting = false;
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
            Debug.Log("turbe aktiivisena");
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
            Debug.Log("turbe epäaktiivisena");
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
            Debug.Log("liikaa turboa; laitetaan maksimiin");
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
        switch(option)
        {
            case "start":
                StartCoroutine("turbeRegenerate");
                Debug.Log("coroutine lisätty");

                break;

            case "stop":
                StopCoroutine("turbeRegenerate");
                Debug.Log("coroutine poistettu");

                break;
        }
    }
}