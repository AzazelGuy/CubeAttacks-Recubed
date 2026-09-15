using UnityEngine;
using UnityEngine.UI; // Importante para reconhecer o Slider

public class AudioSlider : MonoBehaviour
{
    public Slider sliderMusica;
    public Slider sliderSFX;

    void Start()
    {
        // Ao iniciar, a barra assume o valor que j� est� no AudioManager
        if (AudioManager.Instance != null)
        {
            sliderMusica.value = AudioManager.Instance.MusicVolume;
            sliderSFX.value = AudioManager.Instance.SFXVolume;
        }

        // Adiciona um "ouvinte" que detecta quando voc� move a barra
        sliderMusica.onValueChanged.AddListener(SetVolumeMusica);
        sliderSFX.onValueChanged.AddListener(SetVolumeSFX);
    }

    public void SetVolumeMusica(float valor)
    {
        AudioManager.Instance.MusicVolume = valor;
    }

    public void SetVolumeSFX(float valor)
    {
        AudioManager.Instance.SFXVolume = valor;
    }
}