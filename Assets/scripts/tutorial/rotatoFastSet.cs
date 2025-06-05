using UnityEngine;

public class rotatoFastSet : MonoBehaviour
{
    public float speed;
    public float rotation;

    void Start()
    {
        var rot = gameObject.transform.rotation;
        rot = Quaternion.Euler(rot.eulerAngles.x, rotation, rot.eulerAngles.z);
        gameObject.transform.rotation = rot;

        LeanTween.rotateY(gameObject, rotation + 180f, speed)
            .setEaseLinear()
            .setLoopClamp();
    }
}