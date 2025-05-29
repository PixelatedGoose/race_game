using UnityEngine;

public class rotato : MonoBehaviour
{
    void Start()
    {
        //MITÄ VITTUA SE AUTO NYT TEKEE :DDD
        LeanTween.rotateY(gameObject, 180.0f, 4.0f)
            .setEaseLinear()
            .setLoopClamp();
    }
}