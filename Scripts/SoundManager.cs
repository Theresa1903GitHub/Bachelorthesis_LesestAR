using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public Slider VolumeSlider;

    void Start()
    {
        float volume = PlayerPrefs.GetFloat("soundVolume", 1f);
        VolumeSlider.value = volume;
        AudioListener.volume = volume;
    }

    public void SetVolume()
    {
        AudioListener.volume = VolumeSlider.value;
        PlayerPrefs.SetFloat("soundVolume", VolumeSlider.value);
    }
}
