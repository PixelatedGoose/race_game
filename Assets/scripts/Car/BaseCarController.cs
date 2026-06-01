using System;
using UnityEngine;
using System.Linq;
using System.Collections;

public class BaseCarController : MonoBehaviour
{
    [Header("Auton asetukset")]
    //movement reworking jälkeen Acceleration ei tarvi olla 700 enää, 10 on jo hyvä 
    public float Acceleration = 700.0f;
    [SerializeField] protected float BrakeAcceleration = 500.0f;
    [Header("turn asetukset")]
    [SerializeField] protected float TurnSensitivity = 1.0f;
    [SerializeField] protected float MinTurnSensitivity = 17.5f;
    [SerializeField] protected float MaxTurnSensitivity = 30.0f;
    [SerializeField] protected float SteerStrength = 10.0f;
    protected float turnSensitivityRange;
    public float MaxSpeed
    {
        get
        {
            return CarRb.maxLinearVelocity * 3.6f;
        }
        set
        {
            CarRb.maxLinearVelocity = value / 3.6f;
        }
    }
    public float MpsMaxSpeed
    {
        get
        {
            return CarRb.maxLinearVelocity;
        }
        set
        {
            CarRb.maxLinearVelocity = value;
        }
    }
    [SerializeField] protected float BaseMaxSpeed = 130f;
    [SerializeField] protected Wheels Wheels;
    [Header("Trail settings")]
    public Vector2 MovementInputs;
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
    public bool IsTurboActive { get; set; } = false;
    protected Coroutine TurbeRegeneration = null;
    [NonSerialized] public bool CanDrift = true;
    [NonSerialized] public bool CanUseTurbo = true;
    protected Collider carCollider;
    public Vector3 CarExtents { get; protected set; }
    protected AbstractTurbo turbo;
    private Coroutine MaxSpeedDecay;

    virtual protected void Awake()
    {
        MaxSpeed = BaseMaxSpeed;
        turnSensitivityRange = MaxTurnSensitivity - MinTurnSensitivity;
        TryGetComponent(out turbo);
        AutoAssignWheelsAndMaterials();
    }

    virtual protected void Start()
    {
        carCollider = GetComponentInChildren<Collider>();
        CarExtents = carCollider.bounds.size;
        ClearWheelTrails();
    }

    virtual protected void FixedUpdate() {}
    public void ResetMaxSpeed() => MaxSpeed = BaseMaxSpeed;

    [ContextMenu("Auto Assign Wheels")]
    protected void AutoAssignWheelsAndMaterials()
    {
        var Colliders = GetComponentsInChildren<WheelCollider>(true);
        var Meshes = transform.GetComponentsInChildren<Transform>().First(obj => obj.name == "meshes");
        
        var Effects = transform.GetComponentsInChildren<Transform>().First(obj => obj.name == "wheelEffectobj");

        Wheels = new Wheels(
            GetComponentsInChildren<WheelCollider>(true),
            transform.GetComponentsInChildren<Transform>().First(obj => obj.name == "meshes"),
            transform.GetComponentsInChildren<Transform>().First(obj => obj.name == "wheelEffectobj")
        );
    }


    protected void AdjustSuspension()
    {
        foreach (Wheel wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.collider.suspensionSpring;
            suspensionSpring.spring = 8000.0f;
            suspensionSpring.damper = 5000.0f;
            wheel.collider.suspensionSpring = suspensionSpring;
        }
    }

    public float GetDriftSharpness()
    {
        if (!IsDrifting)
            return 0f;

        Vector3 flatVelocity = CarRb.linearVelocity;
        flatVelocity.y = 0f;

        if (flatVelocity.sqrMagnitude < 0.1f)
            return 0f;

        return Vector3.Angle(transform.forward, flatVelocity.normalized);
    }
    
    protected void Decelerate()
    {
        if (MovementInputs.y == 0)
        {
            if (CarRb.linearVelocity.magnitude < 0.1f) CarRb.linearVelocity = Vector3.zero;
            else CarRb.linearVelocity = Vector3.Lerp(CarRb.linearVelocity, Vector3.zero, Time.deltaTime);
        }
    }



    protected void Steer()
    {
        Wheels.SteerAngle = Mathf.Lerp(Wheels.SteerAngle, MovementInputs.x * TurnSensitivity, SteerStrength * Time.deltaTime);
    }


    protected void AdjustWheelsForDrift()
    {
        foreach (Wheel wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.collider.suspensionSpring;
            suspensionSpring.spring = 500.0f;
            suspensionSpring.damper = 2500.0f;
            wheel.collider.suspensionSpring = suspensionSpring;

            WheelFrictionCurve forwardFriction = wheel.collider.forwardFriction;
            forwardFriction.extremumSlip = 0.45f;
            forwardFriction.asymptoteSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 5.5f;
            wheel.collider.forwardFriction = forwardFriction;

            if (wheel.axel == Wheel.Axel.Front)
            {
                WheelFrictionCurve sidewaysFriction = wheel.collider.sidewaysFriction;
                sidewaysFriction.stiffness = 2f;
                wheel.collider.sidewaysFriction = sidewaysFriction;
            }
        }        
    }

    public void Animatewheels()
    {
        foreach (Wheel wheel in Wheels)
        {
            wheel.collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.model.transform.SetPositionAndRotation(pos, rot);
        }
    }

    //bobbing effect

    /// <summary>
    /// calls tje wjeeöeffects
    /// </summary>
    protected void WheelEffects(bool enabled)
    {
        foreach (Wheel wheel in Wheels.RearWheels)
        {
            wheel.trailRenderer.emitting = enabled && wheel.IsGrounded();

            if (wheel.smokeParticle == null) continue;
            if (wheel.trailRenderer.emitting) wheel.smokeParticle.Play();
            else wheel.smokeParticle.Stop();
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

    /// <summary>
    /// Decays max speed after delay seconds over decayTime amount of seconds.
    /// </summary>
    /// <param name="delay">How long to wait before starting to decay?</param>
    /// <param name="decayTime">How long should the decay take?</param>
    /// <param name="overrideLast">Should the previous decay coroutine be overridden, if there is one. If there is one and this is false then this one is ignored.</param>
    public void DecayMaxSpeed(float delay, float decayTime, bool overrideLast)
    {
        if (overrideLast && MaxSpeedDecay != null) StopCoroutine(MaxSpeedDecay);
        MaxSpeedDecay = StartCoroutine(MaxSpeedDecayer(delay, decayTime));
    }

    /// <summary>
    /// Decays max speed after delay seconds over decayTime amount of seconds.
    /// </summary>
    /// <param name="delay">How long to wait before starting to decay?</param>
    /// <param name="decayTime">How long should the decay take?</param>
    public void DecayMaxSpeed(float delay, float decayTime)
    {
        if (MaxSpeed <= BaseMaxSpeed) return;
        if (MaxSpeedDecay != null) StopCoroutine(MaxSpeedDecay);
        MaxSpeedDecay = StartCoroutine(MaxSpeedDecayer(delay, decayTime));
    }

    private IEnumerator MaxSpeedDecayer(float delay, float decayTime)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        float decrement = MaxSpeed / decayTime;
        while (MaxSpeed > BaseMaxSpeed)
        {
            MaxSpeed -= decrement * Time.deltaTime;
            yield return null;
        }

        ResetMaxSpeed();
        yield break;
    }
}