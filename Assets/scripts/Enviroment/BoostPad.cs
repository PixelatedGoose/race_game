using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float maxSpeedMultiplier = 1.2f;
    [SerializeField] private float boostStrenght = 10;
    [SerializeField] private float decayDelay = 0f;

    void OnTriggerEnter(Collider trigger)
    {
        BaseCarController car = trigger.GetComponentInParent<BaseCarController>();
        if (car != null)
        {
            float newMaxSpeed = car.BaseMaxSpeed * maxSpeedMultiplier;
            if (newMaxSpeed > car.MaxSpeed)
            {
                car.MaxSpeed = newMaxSpeed;
                car.DecayMaxSpeed(decayDelay);
            }
            car.CarRb.linearVelocity += boostStrenght * car.transform.forward;
        }
    }
}
