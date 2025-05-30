using UnityEngine;

public class rotato : MonoBehaviour
{
    public float speed;
    void Start()
    {
        //MITÄ VITTUA SE AUTO NYT TEKEE :DDD
        LeanTween.rotateY(gameObject, 180.0f, speed)
            .setEaseLinear()
            .setLoopClamp();
    }
}