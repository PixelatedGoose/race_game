using UnityEngine;

public class additionalCarController : MonoBehaviour
{
    CarInputActions Controls;
    public Rigidbody car;
    private float[] car_positionrotation = new float[6];
    private float carRotationSpeed = 1.0f;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    void Start()
    {
        car_positionrotation = new float[] {car.position.x, car.position.y, car.position.z, 0, 0, 0};
    }

    private void FixedUpdate()
    {
        //car.MovePosition(new Vector3(car_positionrotation[0], car_positionrotation[1], car_positionrotation[2]));
        car_positionrotation[4] += carRotationSpeed;
        car.MoveRotation(Quaternion.Euler(0, car_positionrotation[4], 0));

        if (car_positionrotation[4] >= 360.0f) //jotta peli ei crashaa 415 päivän jälkeen
        {
            car_positionrotation[4] = 0.0f;
        }
    }
}