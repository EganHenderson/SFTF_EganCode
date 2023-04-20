using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AdjustVolume : MonoBehaviour
{
    [SerializeField]
    private AudioMixer master = null;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    private void Awake()
    {
        if (!masterSlider) { Debug.LogWarning("No master slider provided to AdjustVolume! This will prevent the updating of the master sliders value!"); }
        if (!musicSlider) { Debug.LogWarning("No music slider provided to AdjustVolume! This will prevent the updating of the music sliders value!"); }
        if (!effectsSlider) { Debug.LogWarning("No effects slider provided to AdjustVolume! This will prevent the updating of the effects sliders value!"); }
    }

    public void UpdateSliders()
    {
        float temp;

        if (masterSlider)
        {
            master.GetFloat("MasterVolume", out temp);

            masterSlider.value = Mathf.Pow(10, temp / 20);
        }
        if (musicSlider)
        {
            master.GetFloat("MusicVolume", out temp);

            musicSlider.value = Mathf.Pow(10, temp / 20);

        }
        if (effectsSlider)
        {
            master.GetFloat("SoundEffectsVolume", out temp);
            
            effectsSlider.value = Mathf.Pow(10, temp / 20);
        }
    }

    public void AdjustMasterVolume(float volume)
    {
        master.SetFloat("MasterVolume", SetVolumeToDecibel(volume));
    }

    public void AdjustMusicVolume(float volume)
    {
        master.SetFloat("MusicVolume", SetVolumeToDecibel(volume));
    }

    public void AdjustSoundEffectVolume(float volume)
    {
        master.SetFloat("SoundEffectsVolume", SetVolumeToDecibel(volume));
    }

    private float SetVolumeToDecibel(float volume)
    {
        return Mathf.Log10(volume) * 20;
    }
}
