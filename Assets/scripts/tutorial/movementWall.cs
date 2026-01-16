using UnityEngine;

public class movementWall : MonoBehaviour
{
    public AudioSource beginMove, endMove;

    void Start()
    {
        beginMove = GameObject.Find("wallMovement_Lower").GetComponent<AudioSource>();
        endMove = GameObject.Find("wallMovement_End").GetComponent<AudioSource>();
    }

    public void MoveWall()
    {
        LeanTween.moveY(gameObject, -10f, 2.5f).setEaseLinear().setOnComplete(() =>
        {
            endMove.Play();
            beginMove.Stop();
        });
        beginMove.Play();
    }
}