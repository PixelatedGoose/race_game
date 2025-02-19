using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class fumoFumo : MonoBehaviour
{
    public GameObject fumoImage;
    int randomNumber;

    void Start()
    {
        fumo();
    }

    void fumo()
    {
        LeanTween.moveLocalX(fumoImage, -546.0f, 2.5f).setOnComplete(() =>
        {
            randomNumber = UnityEngine.Random.Range((int)57.5f, (int)498.5f);
            fumoImage.transform.position = new Vector3(1092.0f, randomNumber, default);
            fumo();
        });
    }
}