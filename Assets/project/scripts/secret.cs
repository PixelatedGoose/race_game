// Project: Capsule Creator
// Created by: Joonas "Joonas" Kallio
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class secret : MonoBehaviour
{
    public GameObject capsule;
    public List<GameObject> capsules = new List<GameObject>();
    private int counter = 0;
    private int set = 1;
    private readonly int[] setValues = {1, 5, 10, 25, 50, 100, 200, 300, 400, 500, 1000, 2500, 5000, 10000, 20000, 30000, 40000, 50000, 60000};
    private int selectorValue = 0;
    public Text capsuleCreateText;
    public Text capsuleCreateText2;
    private bool allowHold = false;
    private bool ohOops = true;

    void Awake()
    {
        capsuleCreateText.text = "how many: " + set;
        capsuleCreateText2.text = "allow holding: " + allowHold;
    }
    void Update()
    {
        if (ohOops == true && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.LeftAlt].isPressed && Keyboard.current[Key.LeftShift].isPressed && Keyboard.current[Key.Space].isPressed && Keyboard.current[Key.C].isPressed)
        {
            Debug.Log("Oh, oops.");
            ohOops = false;
        }
        else if (ohOops == false)
        {
            StartCoroutine(Selector());
        }
    }

    void CreateCapsule(int amount)
    {
        while (counter < amount)
        {
            // Instantiate the capsule
            GameObject newObject = Instantiate(capsule);

            // Add a Rigidbody component to enable physics
            Rigidbody rb = newObject.AddComponent<Rigidbody>();
            rb.useGravity = true; // Enable gravity
            rb.mass = 0.2f; // Set mass (adjust as needed)
            rb.linearDamping = 0.2f; // Add some drag to slow down movement
            rb.angularDamping = 0.5f; // Add angular drag for rotation damping

            // Add a collider if the capsule doesn't already have one
            if (newObject.GetComponent<Collider>() == null)
            {
                newObject.AddComponent<CapsuleCollider>();
            }

            // Set a random position for the capsule
            newObject.transform.position = new Vector3(
                Random.Range(450, 505),
                Random.Range(1, 19),
                Random.Range(0, 80)
            );
            newObject.transform.rotation = new Quaternion(
                Random.Range(0, 20),
                Random.Range(0, 20),
                Random.Range(0, 20),
                Random.Range(0, 1)
            );

            // Add the capsule to the list
            capsules.Add(newObject);
            counter++;
        }
        counter = 0;
    }

    private IEnumerator Selector()
    {
        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame && selectorValue > 0)
        {
            selectorValue--;
        }
        if (Keyboard.current[Key.RightArrow].wasPressedThisFrame && selectorValue < setValues.Length - 1)
        {
            selectorValue++;
        }
        if (Keyboard.current[Key.Space].wasPressedThisFrame)
        {
            allowHold = !allowHold;
        }

        //ctrl + shift + alt + space + c antaa käyttää tätä

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            set = setValues[selectorValue];
            capsuleCreateText.text = "how many: " + set;
            capsuleCreateText2.text = "allow holding: " + allowHold;
        }

        if (allowHold == false)
        {
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
            {
                CreateCapsule(set);
            }
        }
        else
        {
            if (Keyboard.current[Key.Digit1].isPressed)
            {
                CreateCapsule(set);
            }
        }

        if (selectorValue > 10)
        {
            capsuleCreateText.text = "how many: " + set + " // WARNING: the value you are using may freeze or crash your game";
        }

        yield return null;
    }
}
