using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class additionalCarController : MonoBehaviour
{
    public GameObject speedMeter;
    public Rigidbody car;

    private float[] car_positionrotation = new float[6];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car_positionrotation = new float[] {356.0f, -46.0f, 817.0f, 0, 0, 0};
    }

    // Update is called once per frame
    private void Update()
    {
        // car.MovePosition(new Vector3(car_positionrotation[0], car_positionrotation[1], car_positionrotation[2]));
        car_positionrotation[4] += 0.2f;
        car.MoveRotation(Quaternion.Euler(0, car_positionrotation[4], 0));
    }
}