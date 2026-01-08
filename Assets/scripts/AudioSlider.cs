using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        //init
        volumeSlider.value = PlayerPrefs.GetFloat("audio_value");
    }

    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }
}
