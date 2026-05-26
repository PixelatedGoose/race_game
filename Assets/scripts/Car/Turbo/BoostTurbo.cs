using UnityEngine;

public class BoostTurbo : AbstractTurbo
{
    [SerializeField] private float maxSpeedMultiplier = 1.3f;

    protected override void Use()
    {
        carController.CarRb.linearVelocity += strength * Time.deltaTime * carController.transform.forward;
    }

    public override void Activate()
    {
        base.Activate();
        carController.MaxSpeed *= maxSpeedMultiplier;
    }

    public override void Stop()
    {
        base.Stop();
        carController.ResetMaxSpeed();
    }
}