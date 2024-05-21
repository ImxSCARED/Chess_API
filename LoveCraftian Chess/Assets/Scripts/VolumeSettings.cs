using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class SFXsettings: MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
        public void SetMusicVolume()
        {
        float volume = musicSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume)*20);
        }


}

