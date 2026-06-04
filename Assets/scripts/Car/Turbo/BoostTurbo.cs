using UnityEngine;

public class BoostTurbo : AbstractTurbo
{
    protected override void Use()
    {
        Vector3 dir = carController.transform.forward;
        if (!carController.Wheels.IsTouchingGround()) dir.y = 0;
        carController.CarRb.linearVelocity += strenght * Time.deltaTime * dir;
    }
}