using UnityEngine;

[CreateAssetMenu(fileName = "CarValues", menuName = "Scriptable Objects/CarValues")]
public class CarValues : ScriptableObject
{
    [Header("Acceleration")]
    public float MaxAcceleration = 1000.0f;
    public float BrakeAcceleration = 500.0f;
    public float Deceleration = 1.0f;

    [Header("Steering")]
    public float TurnSensitivityAtHighSpeed = 12f;
    public float TurnSensitivityAtLowSpeed = 16f;

    [Header("Speed")]
    public float BaseSpeed = 120f;
    public float DriftMaxSpeed = 140f;
    public float GrassMaxSpeed = 50.0f;

    [Header("Physics")]
    public float GravityMultiplier = 1.5f;
    public float GrassSpeedMultiplier = 0.5f;

    [Header("Turbo")]
    public float TurboSpeed = 60.0f;
    public float TurboMax = 100.0f;
    public float TurboPush = 15.0f;
    public float TurboReduceRate = 10.0f;
    public float TurboRegenRate = 15.0f;

    public void ApplyValues(BaseCarController BaseCar){
        BaseCar.MaxAcceleration = MaxAcceleration;
        BaseCar.BrakeAcceleration = BrakeAcceleration;
        BaseCar.Deceleration = Deceleration;
        BaseCar.TurnSensitivtyAtHighSpeed = TurnSensitivityAtHighSpeed;
        BaseCar.TurnSensitivtyAtLowSpeed = TurnSensitivityAtLowSpeed;
        BaseCar.BaseSpeed = BaseSpeed;
        BaseCar.DriftMaxSpeed = DriftMaxSpeed;
        BaseCar.Grassmaxspeed = GrassMaxSpeed;
        BaseCar.GravityMultiplier = GravityMultiplier;
        BaseCar.GrassSpeedMultiplier = GrassSpeedMultiplier;
        BaseCar.Turbesped = TurboSpeed;
        BaseCar.TurbeMax = TurboMax;
        BaseCar.Turbepush = TurboPush;
        BaseCar.TurbeReduce = TurboReduceRate;
        BaseCar.TurbeRegen = TurboRegenRate;
    }
}
