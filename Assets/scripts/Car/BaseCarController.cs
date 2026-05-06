using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class BaseCarController : MonoBehaviour
{
    [Header("Auton asetukset")]
    //movement reworking jälkeen Acceleration ei tarvi olla 700 enää, 10 on jo hyvä 
    public float Acceleration = 700.0f;
    public float Deceleration = 700.0f;
    [SerializeField] protected float BrakeAcceleration = 500.0f;
    [Header("turn asetukset")]
    [SerializeField] protected float TurnSensitivity = 1.0f;
    [SerializeField] protected float MinTurnSensitivity = 17.5f;
    [SerializeField] protected float MaxTurnSensitivity = 30.0f;
    protected float turnSensitivityRange;
    public float MaxSpeed = 180.0f;
    /// <summary>
    /// Max speed in meters per second.
    /// </summary>
    public float MpsMaxSpeed { get; protected set; }
    [SerializeField] protected List<Wheel> Wheels;
    protected readonly Func<Wheel, bool> frontWheelPredicate = w => w.Axel == Axel.Front;
    protected readonly Func<Wheel, bool> rearWheelPredicate = w => w.Axel == Axel.Rear;
    [Header("Trail settings")]
    public float MoveInput;
    public float SteerInput;
    protected Vector3 _CenterofMass;
    public float TargetTorque;
    public Rigidbody CarRb { get; protected set; }
    public float Turbesped = 60.0f;
    public float BaseSpeed = 180f;
    public float DriftMaxSpeed = 140f;
    [Header("Drift asetukset")]
    public bool IsDrifting { get; protected set; } = false;
    public float BaseMaxAccerelation { get; protected set; }
    public float BaseTargetTorque { get; protected set; }
    public float SmoothedMaxAcceleration { get; protected set; }
    [Header("turbe asetukset")]
    protected Image TurbeBar;
    public bool IsTurboActive { get; set; } = false;
    public float TurbeAmount { get; protected set; } = 100.0f;
    [SerializeField] protected float TurbeMax = 100.0f;
    public float Turbepush = 15.0f;
    [SerializeField] protected float TurbeReduce = 10.0f;
    [SerializeField] protected float TurbeRegen = 10.0f;
    [SerializeField] protected float TurbeWaitTime = 2.0f;
    protected Coroutine TurbeRegeneration = null;
    [NonSerialized] public bool CanDrift = true;
    [NonSerialized] public bool CanUseTurbo = true;
    protected Collider carCollider;
    public Vector3 CarExtents { get; protected set; }
    protected Turbo turbo;

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public class Wheel
    {
        public GameObject WheelModel;
        public WheelCollider WheelCollider;

        public GameObject WheelEffectobj;
        public ParticleSystem SmokeParticle;
        public Axel Axel;
        public TrailRenderer trailRenderer;

        public bool IsGrounded()
        {
            return WheelCollider.GetGroundHit(out WheelHit hit);
        }

        public void Brake(float BrakeAcceleration)
        {
            WheelCollider.brakeTorque = BrakeAcceleration * 15f;
        }

        public void SetTorque(float TargetTorque)
        {
            WheelCollider.motorTorque = TargetTorque;
            WheelCollider.brakeTorque = 0f;
        }
    }

    virtual protected void OnValidate()
    {
        MpsMaxSpeed = MaxSpeed / 3.6f;
        turnSensitivityRange = MaxTurnSensitivity - MinTurnSensitivity;
    }

    virtual protected void Awake()
    {
        //AutoAssignWheelsAndMaterials();
        MpsMaxSpeed = MaxSpeed / 3.6f;
        Debug.Log(MpsMaxSpeed);
        TryGetComponent(out turbo);
    }

    virtual protected void Start()
    {
        carCollider = GetComponentInChildren<Collider>();
        CarExtents = carCollider.bounds.size;
        //AutoAssignWheelsAndMaterials();
        ClearWheelTrails();
    }

    virtual protected void FixedUpdate()
    {
        ApplySpeedLimit();
    }

    virtual protected void ApplySpeedLimit()
    {
        if (CarRb.linearVelocity.magnitude > MpsMaxSpeed) CarRb.linearVelocity = MpsMaxSpeed * CarRb.linearVelocity.normalized;
    }

    [ContextMenu("Auto Assign Wheels")]
    protected void AutoAssignWheelsAndMaterials()
    {
        Wheels.Clear();

        var Colliders = GetComponentsInChildren<WheelCollider>(true);
        var Meshes = transform.GetComponentsInChildren<Transform>().First(obj => obj.name == "meshes");
        
        var Effects = transform.GetComponentsInChildren<Transform>().First(obj => obj.name == "wheelEffectobj");

        foreach (WheelCollider WheelCollider in Colliders)
        {
            Wheel wheel = new()
            {
                WheelCollider = WheelCollider
            };

            Transform Mesh = Meshes.Find(WheelCollider.name);

            wheel.WheelModel = Mesh != null ? Mesh.gameObject : null;

            Transform Effect = Effects.transform.Find(WheelCollider.name);

            wheel.WheelEffectobj = Effect != null ? Effect.gameObject : null;
            TrailRenderer trailRenderer = wheel.WheelEffectobj != null ? wheel.WheelEffectobj.GetComponentInChildren<TrailRenderer>(true) : null;
            if (trailRenderer != null && (trailRenderer.sharedMaterial == null || trailRenderer.sharedMaterial.shader == null || !trailRenderer.sharedMaterial.shader.isSupported))
            {
                trailRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            trailRenderer.enabled = true;
            wheel.trailRenderer = trailRenderer;
            wheel.SmokeParticle = wheel.WheelEffectobj != null
                ? wheel.WheelEffectobj.GetComponentInChildren<ParticleSystem>(true)
                : WheelCollider.transform.GetComponentInChildren<ParticleSystem>(true);

            wheel.Axel = WheelCollider.name.IndexOf("front", StringComparison.OrdinalIgnoreCase) >= 0 ? Axel.Front : Axel.Rear;

            Wheels.Add(wheel);
        }
    }

    protected void AdjustSuspension()
    {
        foreach (Wheel wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.WheelCollider.suspensionSpring;
            suspensionSpring.spring = 8000.0f;
            suspensionSpring.damper = 5000.0f;
            wheel.WheelCollider.suspensionSpring = suspensionSpring;
        }
    }

    protected void Decelerate()
    {
        if (MoveInput == 0)
        {
            if (CarRb.linearVelocity.magnitude < 0.1f) CarRb.linearVelocity = Vector3.zero;
            else CarRb.linearVelocity = Vector3.Lerp(CarRb.linearVelocity, Vector3.zero, Time.deltaTime);
        }
    }



    protected void Steer()
    {
        foreach (Wheel wheel in Wheels)
        {
            if (wheel.Axel == Axel.Front) wheel.WheelCollider.steerAngle = Mathf.Lerp(wheel.WheelCollider.steerAngle, SteerInput * TurnSensitivity * (IsDrifting ? 0.8f : 0.35f), 0.6f);           
        }
    }


    protected void AdjustWheelsForDrift()
    {
        foreach (Wheel wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.WheelCollider.suspensionSpring;
            suspensionSpring.spring = 500.0f;
            suspensionSpring.damper = 2500.0f;
            wheel.WheelCollider.suspensionSpring = suspensionSpring;

            WheelFrictionCurve forwardFriction = wheel.WheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.45f;
            forwardFriction.asymptoteSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 5.5f;
            wheel.WheelCollider.forwardFriction = forwardFriction;

            if (wheel.Axel == Axel.Front)
            {
                WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = 2f;
                wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }        
    }

    public void Animatewheels()
    {
        foreach (Wheel wheel in Wheels)
        {
            wheel.WheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.WheelModel.transform.SetPositionAndRotation(pos, rot);
        }
    }

    //bobbing effect

    /// <summary>
    /// calls tje wjeeöeffects
    /// </summary>
    protected void WheelEffects(bool enabled)
    {
        foreach (Wheel wheel in Wheels)
        {
            if (wheel.Axel != Axel.Rear) continue;

            wheel.trailRenderer.emitting = enabled && wheel.IsGrounded();

            if (wheel.SmokeParticle == null) continue;
            if (wheel.trailRenderer.emitting) wheel.SmokeParticle.Play();
            else wheel.SmokeParticle.Stop();
        }
    }

    public void ClearWheelTrails()
    {
        foreach (Wheel wheel in Wheels)
        {
            wheel.trailRenderer.emitting = false;
            wheel.trailRenderer.Clear();
        }
    }
}