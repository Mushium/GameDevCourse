using UnityEngine;
using UnityEngine.UI;

public class VolumeBar : MonoBehaviour
{
    public Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = AudioSingleton.Instance.volume;
        slider.onValueChanged.AddListener(ChangeAudioVolume);
    }

    // Update is called once per frame
    void ChangeAudioVolume(float value)
    {
        AudioSingleton.Instance.ChangeVolume(slider);
    }
}
