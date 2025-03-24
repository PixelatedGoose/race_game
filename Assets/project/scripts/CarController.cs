using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
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

    public float maxAcceleration = 300.0f;
    public float brakeAcceleration = 3.0f;
    public float turnSensitivty = 1.0f;
    public float maxsteerAngle = 30.0f;
    public float deceleration = 1.0f; 
    public float maxspeed = 100.0f;
    public float gravityMultiplier = 1.5f; 
    public float accelerationRate = 5.0f;
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


    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

    }


    void Start()
    {
        if (carRb == null)
            carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerofMass;
        
    }

    private void Onable()
    {
        Controls.Enable();
    }

    private void Disable()
    {
        Controls.Disable();
    }


    public float GetSpeed()
    {
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
        Move();
        Steer();
        HandleDrift();
        Decelerate();
        ApplySpeedLimit();
        Applyturnsensitivity();
        OnGrass();
        ApplyGravity();
        TURBE();
        test();
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
        turnSensitivty = speed > 60.0f ? 10.0f : (speed > 40.0f ? 10.0f : 35.0f);
    }
    void TURBE()
    {
        isTurboActive = Controls.CarControls.turbo.IsPressed();
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
        }

        if (IsOnGrass())
        {
            targetTorque *= grassSpeedMultiplier;
            maxspeed = Mathf.Lerp(maxspeed, 50.0f, Time.deltaTime);
        }
        else
        {
            maxspeed = Mathf.Lerp(maxspeed, isTurboActive ? 150.0f : 100.0f, Time.deltaTime);
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
        if (!IsGrounded())
        {
            carRb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    void HandleDrift()
    {
        Controls.CarControls.Drift.performed += ctx => {
            if (activedrift > 0)
            {
                return;
            }
            activedrift++;
            print(activedrift);

            foreach (var wheel in wheels)
            {
                if (wheel.wheelCollider == null) continue;

                WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
                sidewaysFriction.extremumSlip = 1.5f;
                sidewaysFriction.asymptoteSlip = 2.0f;
                sidewaysFriction.extremumValue = 0.5f;
                sidewaysFriction.asymptoteValue = 0.75f;
                wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
            }
            if (carRb.linearVelocity.magnitude > 1.0f)
            {
                Controls.CarControls.Move.performed += OnMovePerformed;
            }
        };

        Controls.CarControls.Drift.canceled += ctx => {
            if (activedrift > 0) activedrift--;
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
        };
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
    void test()
    {
        foreach (var wheel in wheels)
        {
            if (carRb.linearVelocity.magnitude > 10.0f)
            {
                print("fuck this");
            }
        }
    }
}
