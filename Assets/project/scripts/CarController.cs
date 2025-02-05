using System;
using System.Collections.Generic;
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
        public Axel axel;
    }

    public float maxAcceleration = 50.0f;
    public float brakeAcceleration = 3.0f;
    public float turnSensitivty = 1.0f;
    public float maxsteerAngle = 30.0f;
    public float deceleration = 1.0f; 
    public float maxspeed = 100.0f;

    public List<Wheel> wheels; 
    float moveInput;
    float steerInput;

    public Vector3 _centerofMass;

    private Rigidbody carRb;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerofMass;
    }

    void LateUpdate()
    {
        GetInputs();
        Move();
        Steer();
        HandleDrift();
        HandleBrake();
        Animatewheels();
    }

    void GetInputs()    
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void Move() 
    {
        foreach(var wheel in wheels)
        {
            if (moveInput != 0)
            {
                wheel.wheelCollider.motorTorque = moveInput * 1600 * maxAcceleration * Time.deltaTime;
                wheel.wheelCollider.brakeTorque = 0f; // Remove brake torque when accelerating
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0f;
                wheel.wheelCollider.brakeTorque = 0f; // Remove brake torque


                Vector3 velocity = carRb.linearVelocity;
                velocity -= velocity.normalized * deceleration * Time.deltaTime;
                if (velocity.magnitude < 0.1f) 
                {
                    velocity = Vector3.zero;
                }
                carRb.linearVelocity = velocity;
            }
        }
    }

    void Steer() 
    { 
        foreach(var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivty * maxsteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void HandleDrift()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
                sidewaysFriction.extremumSlip = 1.5f; // Increase slip for drifting
                sidewaysFriction.asymptoteSlip = 2.0f; 
                sidewaysFriction.extremumValue = 0.5f; // Adjust friction values
                sidewaysFriction.asymptoteValue = 0.75f; 
                wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
                sidewaysFriction.extremumSlip = 0.2f; // Reset to normal values
                sidewaysFriction.asymptoteSlip = 0.5f;
                sidewaysFriction.extremumValue = 1.0f;
                sidewaysFriction.asymptoteValue = 0.75f;
                wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }
    }

    void HandleBrake()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 1 * brakeAcceleration * Time.deltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0f;
            }
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

    private void Update()
    {
        // Apply speed limit
        float speed = carRb.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h
        if (speed > maxspeed)
        {
            carRb.linearVelocity = carRb.linearVelocity.normalized * (maxspeed / 3.6f); // Convert km/h to m/s
        }

        // Existing code for controlling the car...
    }
}