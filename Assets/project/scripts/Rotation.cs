using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class Rotation :  MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
        
    }  
}
