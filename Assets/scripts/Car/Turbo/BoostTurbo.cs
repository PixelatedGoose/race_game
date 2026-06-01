using UnityEngine;

public class BoostTurbo : AbstractTurbo
{
    protected override void Use()
    {
        carController.CarRb.linearVelocity += strenght * Time.deltaTime * carController.transform.forward;
    }
}