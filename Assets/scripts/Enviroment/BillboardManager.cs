using System;
using System.Collections.Generic;
using UnityEngine;

public class BillboardManager : MonoBehaviour
{
    static readonly List<BillboardingObject> objects = new();

    private Camera billboardCamera;
    [SerializeField] private float updateInterval = 0.2f;
    [SerializeField] private float lenientAngle = 90f;

    float timer;

    void Awake()
    {
        billboardCamera = Camera.main;
        if (billboardCamera == null) throw new NullReferenceException("billboardCamera is null");
    }

    void Update()
    {
        //käytetään unscaledDeltaTimeä sillä ne puut ei muuten tykkää billboardata alussa.
        //JA että se kamera toivottavasti ei snappaa oudosti mapin alussa
        timer += Time.unscaledDeltaTime;
        if (timer < updateInterval) return; 
        timer = 0f;

        UpdateBillboarding();
    }

    void UpdateBillboarding()
    {
        Vector3 camPos = billboardCamera.transform.position;
        Vector3 camForward = billboardCamera.transform.forward;

        for (int i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];

            Vector3 toObj = (obj.transform.position - camPos).normalized;

            if (Vector3.Angle(camForward, toObj) > lenientAngle) continue;

            Vector3 lookDir = camPos - obj.transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion baseRotation = Quaternion.LookRotation(-lookDir);
                obj.transform.rotation = baseRotation * obj.RotationOffset;
            }
        }
    }

    public static void Register(BillboardingObject obj)
    {
        if (!objects.Contains(obj)) objects.Add(obj);
    }

    public static void Unregister(BillboardingObject obj)
    {
        objects.Remove(obj);
    }
}
