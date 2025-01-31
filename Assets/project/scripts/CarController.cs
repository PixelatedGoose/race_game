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
    public float brakeAcceleration = 50.0f;

    public float turnSensitivty = 1.0f;
    public float maxsteerAngle = 30.0f;

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
    void Update()
    {
        GetInputs();
        Animatewheels();
    }

    void LateUpdate()
    {
        Move();
        steer();
        brake();
    }

    void GetInputs()    
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void Move() {
        foreach(var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 1600 * maxAcceleration * Time.deltaTime;
        }

    }

    void steer() 
    { 
        foreach(var wheel in wheels)
        {
            if (wheel.axel  == Axel.Front)
            {
            var _steerAngle = steerInput * turnSensitivty * maxsteerAngle;
            wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }

    }
    void brake()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            foreach(var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque =  300 * brakeAcceleration * Time.deltaTime;
            }
        }
        else {
            foreach(var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
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

}

