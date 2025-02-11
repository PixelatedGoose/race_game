using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class CarController : MonoBehaviour
{
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

    private Rigidbody carRb;

    void Start()
    {
        if (carRb == null)
            carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerofMass;
    }

    void LateUpdate()
    {
        Land();
        GetInputs();
        Move();
        Steer();
        HandleDrift();
        Animatewheels();
        ApplyGravity();
        WheelEffects();
        Decelerate();
    }

    void GetInputs()    
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    bool IsGrounded()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider.isGrounded)
            {
                return true;
            }
        }
        return false;
    }


    bool IsOnGrass()
    {
        foreach (var wheel in wheels)
        {
            RaycastHit hit;
            if (Physics.Raycast(wheel.wheelCollider.transform.position, -wheel.wheelCollider.transform.up, out hit, wheel.wheelCollider.radius + wheel.wheelCollider.suspensionDistance))
            {
                if (((1 << hit.collider.gameObject.layer) & grass) != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }


    void Move() 
    {
        foreach(var wheel in wheels)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                wheel.wheelCollider.brakeTorque = brakeAcceleration * 1000f;
                wheel.wheelCollider.motorTorque = 0f; //auto pysähtyy
            }
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                // auto kiihtyy
                float targetTorque = moveInput * maxAcceleration;
                wheel.wheelCollider.motorTorque = targetTorque;
                wheel.wheelCollider.brakeTorque = 0f;

            }
            else
            {
                wheel.wheelCollider.motorTorque = 0f;
                wheel.wheelCollider.brakeTorque = 0f; 
            }
        }
    }

    void Land()
    {
        targetTorque = moveInput * maxAcceleration;

        if (IsOnGrass())
        {
            targetTorque *= grassSpeedMultiplier;
            maxspeed = Mathf.Lerp(maxspeed, 50.0f, Time.deltaTime);
        }
        else
        {
            maxspeed = Mathf.Lerp(maxspeed, 100.0f, Time.deltaTime); 
        }

        foreach (var wheel in wheels)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                wheel.wheelCollider.motorTorque = targetTorque;
            }
        }
    }

    void Decelerate()
    {
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
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
        carRb.AddForce(Vector3.down   * gravityMultiplier, ForceMode.Acceleration);
    }

    void HandleDrift()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve  sidewaysFriction = wheel.wheelCollider.sidewaysFriction;

            if (Input.GetKey(KeyCode.Space))
            {
                sidewaysFriction.extremumSlip = 1.5f;
                sidewaysFriction.asymptoteSlip = 2.0f;
                sidewaysFriction.extremumValue = 0.5f;
                sidewaysFriction.asymptoteValue = 0.75f;
            }
            else
            {
                // laittaa normaalit arvot 
                sidewaysFriction.extremumSlip = 0.2f;
                sidewaysFriction.asymptoteSlip = 0.5f;
                sidewaysFriction.extremumValue = 1.0f;
                sidewaysFriction.asymptoteValue = 1f;
            }


            wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
        }
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

    }

    void WheelEffects()
    {
        foreach(var wheel in wheels)
        {
            if(Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear)
            {
                Console.WriteLine("Trail");
                wheel.wheelEffectobj.GetComponentInChildren<TrailRenderer>().emitting = true;
            }
            else
            {
                wheel.wheelEffectobj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }

    private void Update()
    {
        // maxspeed

        // Apply speed limit
        float speed = carRb.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h
        if (speed > maxspeed)
        {
            carRb.linearVelocity = carRb.linearVelocity.normalized * (maxspeed / 3.6f);
        }
        if (speed > 40.0f)
        {
            turnSensitivty = 10.0f;
        }
        else
        {
            turnSensitivty = 35.0f;
        }
    }
    
}