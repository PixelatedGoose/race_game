using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float maxSpeedMultiplier = 1.2f;
    [SerializeField] private float boostStrenght = 10;
    [SerializeField] private float decayDelay = 4f;
    [SerializeField] private float decayDuration = 3f;


    void OnTriggerEnter(Collider trigger)
    {
        BaseCarController car = trigger.GetComponentInParent<BaseCarController>();
        if (car != null)
        {
            car.MaxSpeed *= maxSpeedMultiplier;
            car.CarRb.linearVelocity += boostStrenght * car.transform.forward;
            car.DecayMaxSpeed(decayDelay, decayDuration);
        }
    }
}
