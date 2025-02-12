using UnityEngine;

public class musicVolumeControl : MonoBehaviour
{
    public AudioSource cirno;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cirno = this.GetComponent<AudioSource>();
        cirno.volume = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && cirno.volume <= 1.0f)
        {
            cirno.volume += 0.005f;
        }

        else
        {
            cirno.volume -= 0.02f;
        }
    }
}
