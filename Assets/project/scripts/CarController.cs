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
    public float gravityMultiplier = 1.5f; 
    public float accelerationRate = 5.0f;
    public List<Wheel> wheels; 
    float moveInput;
    float steerInput;

    public Vector3 _centerofMass;

    private Rigidbody carRb;

    void Start()
    {
        if (carRb == null)
            carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerofMass;
    }

    void LateUpdate()
    {
        GetInputs();
        Move();
        Steer();
        HandleDrift();
        Animatewheels();
        ApplyGravity();
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


    void Move() 
    {
        foreach(var wheel in wheels)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Apply brake torque
                wheel.wheelCollider.brakeTorque = brakeAcceleration * 1000f;
                wheel.wheelCollider.motorTorque = 0f; // Ensure motor torque is zero when braking
            }
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                // Apply motor torque directly for more responsiveness
                float targetTorque = moveInput * maxAcceleration;
                wheel.wheelCollider.motorTorque = targetTorque;
                wheel.wheelCollider.brakeTorque = 0f;
                
                
                
                 // Remove brake torque when accelerating
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0f;
                wheel.wheelCollider.brakeTorque = 0f; // Remove brake torque
            }
        }

        // Always apply deceleration when no input is provided
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
                // Reset to normal values
                sidewaysFriction.extremumSlip = 0.2f;
                sidewaysFriction.asymptoteSlip = 0.5f;
                sidewaysFriction.extremumValue = 1.0f;
                sidewaysFriction.asymptoteValue = 1f;
            }

            // Assign the updated friction values back to the wheelCollider
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

    private void Update()
    {
        // Handle forward and backward movement

        // Apply speed limit
        float speed = carRb.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h
        if (speed > maxspeed)
        {
            carRb.linearVelocity = carRb.linearVelocity.normalized * (maxspeed / 3.6f); // Convert km/h to m/s
        }
    }
    
}