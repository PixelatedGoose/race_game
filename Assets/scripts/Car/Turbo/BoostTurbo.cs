using UnityEngine;

public class BoostTurbo : AbstractTurbo
{
    protected override void Use()
    {
        carController.CarRb.linearVelocity += strength * Time.deltaTime * carController.transform.forward;
    }
}