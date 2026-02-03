using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using Logitech;
using System.Linq;

public class BaseCarController : MonoBehaviour
{

    internal CarInputActions Controls;

    internal RacerScript RacerScript;

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject WheelModel;
        public WheelCollider WheelCollider;

        public GameObject WheelEffectobj;
        public ParticleSystem SmokeParticle;
        public Axel Axel;
    }

    [Header("Auton asetukset")]
    internal float MaxAcceleration = 700.0f;
    internal float BrakeAcceleration { get; set; } = 500.0f;
    [Header("turn asetukset")]
    internal float TurnSensitivty  = 1.0f;
    internal float TurnSensitivtyAtHighSpeed  = 14.0f;
    internal float TurnSensitivtyAtLowSpeed  = 20.0f;
    internal float Deceleration  = 1.0f;
    [Min(100.0f)]
    internal float Maxspeed  = 100.0f;
    internal float GravityMultiplier  = 1.5f;
    internal List<Wheel> Wheels;
    internal float moveInput, steerInput;
    internal Vector3 _CenterofMass;
    internal float TargetTorque  = 0.0f;
    internal Rigidbody CarRb;
    internal bool IsTurboActive = false;
    internal float Activedrift   = 0.0f;
    internal float Turbesped = 150.0f, BaseSpeed = 180f, Grassmaxspeed = 50.0f, DriftMaxSpeed = 40f;
    [Header("Drift asetukset")]
    internal float DriftMultiplier = 1.0f;
    internal bool IsTurnedDown = false, IsDrifting;
    internal float PerusMaxAccerelation, PerusTargetTorque, SmoothedMaxAcceleration;


    [Header("turbe asetukset")]
    internal Image TurbeMeter;
    internal float TurbeAmount = 100.0f, TurbeMax = 100.0f, Turbepush = 50.0f;
    internal float TurbeReduce = 10.0f;
    internal float TurbeRegen = 10.0f;
//

    internal bool IsRegenerating = false;
    internal int TurbeRegenCoroutineAmount = 0;
    internal Dictionary<string, float> CarTurboValues = new Dictionary<string, float>();

    internal bool CanDrift = false;
    internal bool CanUseTurbo = false;

    



    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();
    
    }

    public float GetSpeed()
    {
        GameManager.instance.carSpeed = CarRb.linearVelocity.magnitude * 3.6f;
        return CarRb.linearVelocity.magnitude * 3.6f;
    }

    public float GetMaxSpeed()
    {
        return Maxspeed;
    }

    

    internal void HandleTurbo()
    {
        if (!CanUseTurbo) return;
        TURBE();
        TURBEmeter();
    }

    

    bool IsWheelGrounded(Wheel wheel)
    {
        return Physics.Raycast(wheel.WheelCollider.transform.position, -wheel.WheelCollider.transform.up, out RaycastHit hit, wheel.WheelCollider.radius + wheel.WheelCollider.suspensionDistance);
    }

    [ContextMenu("Auto Assign Wheels")]
    internal void AutoAssignWheels()
    {
        if (Wheels == null) Wheels = new List<Wheel>();
        Wheels.Clear();

        var colliders = GetComponentsInChildren<WheelCollider>(true);
        Transform meshesRoot = transform.Find("meshes");
        Transform effectsRoot = transform.Find("wheelEffectobj");

        foreach (var wc in colliders)
        {
            Wheel w = new Wheel();
            w.WheelCollider = wc;

 
            Transform modelT = meshesRoot != null ? meshesRoot.Find(wc.name) : null;
            if (modelT == null) modelT = wc.transform.GetComponentInChildren<MeshRenderer>(true)?.transform;
            w.WheelModel = modelT != null ? modelT.gameObject : null;


            Transform effectT = effectsRoot != null ? effectsRoot.Find(wc.name) : null;
            if (effectT == null) effectT = wc.transform.Find("wheelEffectobj");
            w.WheelEffectobj = effectT != null ? effectT.gameObject : null;

    
            w.SmokeParticle = w.WheelEffectobj != null
                ? w.WheelEffectobj.GetComponentInChildren<ParticleSystem>(true)
                : wc.transform.GetComponentInChildren<ParticleSystem>(true);

           
            var n = wc.name.ToLowerInvariant();
            w.Axel = n.Contains("front") ? Axel.Front : Axel.Rear;

            Wheels.Add(w);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.02f);
    }

    internal void ApplySpeedLimit(float speed)
    {
        if (speed <= Maxspeed) return;
        CarRb.linearVelocity = CarRb.linearVelocity.normalized * (Maxspeed / 3.6f);
    }

    internal void TURBE()
    {
        //uskon että tää on tarpeeton; viittauksia KOMMENTEISSA yhessä scriptis, ei missää muualla
        //tätä ei myöskää muuteta koskaan...
        if (IsTurnedDown)
        {
            IsTurboActive = false;
            return;
        }
        IsTurboActive = Controls.CarControls.turbo.IsPressed() && TurbeAmount > 0;
        if (IsTurboActive)
        {
            CarRb.AddForce(transform.forward * Turbepush, ForceMode.Acceleration);
            TargetTorque *= 1.5f;
        }
    }

    void Move()
    {
        //HandeSteepSlope();
        //UpdateTargetTorgue();
        AdjustSuspension();
        foreach (var wheel in Wheels)
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

    private void AdjustSuspension()
    {
        foreach (var wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.WheelCollider.suspensionSpring;
            suspensionSpring.spring = 8000.0f;
            suspensionSpring.damper = 5000.0f;
            wheel.WheelCollider.suspensionSpring = suspensionSpring;
        }
    }

    private void AdjustForwardFrictrion()
    {
        foreach (var wheel in Wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.WheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteSlip = 1.0f;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 3.5f;
            wheel.WheelCollider.forwardFriction = forwardFriction;
        }
    }

    private void Brakes(Wheel wheel)
    {
        GameManager.instance.StopAddingPoints();
        wheel.WheelCollider.brakeTorque = BrakeAcceleration * 15f;
    }

    private void MotorTorgue(Wheel wheel)
    {
        wheel.WheelCollider.motorTorque = TargetTorque;
        wheel.WheelCollider.brakeTorque = 0f;
    }

    

    void Decelerate()
    {
        if (moveInput == 0)
        {
            Vector3 velocity = CarRb.linearVelocity;

            velocity -= velocity.normalized * Deceleration * 2.0f * Time.deltaTime;

            if (velocity.magnitude < 0.1f)
            {
                velocity = Vector3.zero;
            }
            CarRb.linearVelocity = velocity;
        }
    }



    internal void Steer()
    {
        foreach (var wheel in Wheels.Where(w => w.Axel == Axel.Front))
        {
            var _steerAngle = steerInput * TurnSensitivty * (IsDrifting ? 0.45f : 0.35f);
            wheel.WheelCollider.steerAngle = Mathf.Lerp(wheel.WheelCollider.steerAngle, _steerAngle, 0.6f);            
        }
    }

    
    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            CarRb.AddForce(Vector3.down * GravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }

    }
    public float GetDriftSharpness()
    {
        if (IsDrifting)
        {
            Vector3 velocity = CarRb.linearVelocity;
            Vector3 forward = transform.forward;
            float angle = Vector3.Angle(forward, velocity);
            return angle;  
        }
        //checks the angle between the car's forward direction and its velocity vector constantly while drifting
        return 0.0f;
    }

    private void AdjustWheelsForDrift()
    {
        foreach (var wheel in Wheels)
        {
            JointSpring suspensionSpring = wheel.WheelCollider.suspensionSpring;
            suspensionSpring.spring = 4000.0f;
            suspensionSpring.damper = 1000.0f;
            wheel.WheelCollider.suspensionSpring = suspensionSpring;
        }

        foreach (var wheel in Wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.WheelCollider.forwardFriction;
            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.asymptoteSlip = 0.6f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteValue = 1;
            forwardFriction.stiffness = 3f;
            wheel.WheelCollider.forwardFriction = forwardFriction;
            
            if (wheel.Axel == Axel.Front)
            {
                WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = 2.0f;
                wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }
    }

    void StopDrifting()
    {
        Activedrift = 0;
   
        IsDrifting = false;
        MaxAcceleration = PerusMaxAccerelation;
        CarRb.angularDamping = 0.05f;
        if (RacerScript != null &&
            (RacerScript.raceFinished || GameManager.instance.carSpeed < 20.0f))
        {
            GameManager.instance.StopAddingPoints();
            return;
        }
        GameManager.instance.StopAddingPoints();

        foreach (var wheel in Wheels)
        {
            if (wheel.WheelCollider == null) continue;

            WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.asymptoteSlip = 0.4f;
            sidewaysFriction.extremumValue = 1.0f;
            sidewaysFriction.asymptoteValue = 1f;
            sidewaysFriction.stiffness = 5f;
            wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }

    public void Animatewheels()
    {
        foreach (var wheel in Wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.WheelCollider.GetWorldPose(out pos, out rot);
            wheel.WheelModel.transform.position = pos;
            wheel.WheelModel.transform.rotation = rot;
        }
    }

    //bobbing effect

    /// <summary>
    /// does wheel effects
    /// </summary>
    internal void WheelEffects(bool enable)
    {
        foreach (var wheel in Wheels.Where(w => w.Axel == Axel.Rear))
        {
            var trailRenderer = wheel.WheelEffectobj.GetComponentInChildren<TrailRenderer>();
            bool wheelGrounded = IsWheelGrounded(wheel);
            bool shouldEmit = enable && wheelGrounded;

            // Always ensure we can re-enable emitting even if it was previously cleared/disabled.
            if (shouldEmit)
            {
                trailRenderer.emitting = true;
                wheel.SmokeParticle.Play();
            }
            else
            {
                trailRenderer.emitting = false;
                wheel.SmokeParticle.Stop();
            }
        }
    }

    public void ClearWheelTrails()
    {
        foreach (var wheel in Wheels)
        {
            var trail = wheel.WheelEffectobj.GetComponentInChildren<TrailRenderer>();
            // explicitly stop emitting and clear the trail so it can be re-enabled later
            trail.emitting = false;
            trail.Clear();
            // keep the component enabled so emitting can be toggled again
            trail.enabled = true;
        }
    }

    /// <summary>
    /// käytetään TURBEmeterin päivittämiseen joka frame
    /// </summary>
    internal void TURBEmeter()
    {
        if (IsTurboActive && TurbeAmount != 0) //jos käytät turboa ja sitä o jäljellä
        {
            GameManager.instance.turbeActive = true;

            if (TurbeRegenCoroutineAmount > 0)
            {
                turbeRegenCoroutines("stop");
            }
            IsRegenerating = false;
            TurbeRegenCoroutineAmount = 0;

            TurbeAmount -= TurbeReduce * Time.deltaTime;
        }
        else if (!IsTurboActive && TurbeAmount < TurbeMax) //jos et käytä turboa ja se ei oo täynnä
        {

            GameManager.instance.turbeActive = false;

            if (TurbeRegenCoroutineAmount == 0 && IsRegenerating == false)
            {
                turbeRegenCoroutines("start");
                TurbeRegenCoroutineAmount += 1;
            }
        }

        if (TurbeAmount < 0)
        {
            TurbeAmount = 0;
        }
        if (TurbeAmount > TurbeMax)
        {
            //Debug.Log("I bought a property in Egypt, and what they do is they give you the property");
            TurbeAmount = TurbeMax;

            turbeRegenCoroutines("stop");
            IsRegenerating = false;
            TurbeRegenCoroutineAmount = 0;
        }

        TurbeMeter.fillAmount = TurbeAmount / TurbeMax;
    }

    /// <summary>
    /// käytetään TURBEn regeneroimiseen
    /// ...koska fuck C#
    /// </summary>
    private IEnumerator turbeRegenerate()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        IsRegenerating = true;

        if (IsRegenerating && TurbeRegenCoroutineAmount == 1)
        {
            while (IsRegenerating && TurbeRegenCoroutineAmount == 1)
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
        TurbeAmount += TurbeRegen * Time.deltaTime;
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
}
