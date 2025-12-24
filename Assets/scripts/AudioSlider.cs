using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        if (volumeSlider != null)
        {
            //init
            volumeSlider.value = AudioListener.volume;
        }
    }

    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }
}
