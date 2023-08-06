using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeAdjuster : MonoBehaviour
{
    [SerializeField]
    private Slider adjustVolumeSlider;
    [SerializeField]
    private TMP_Text valueText;

    private AudioSource audioSource;

    private float[] samples;
    private bool isAudioLoaded = false;

    private PBSpeedFilter filter;
    private AudioTrimmer trimmer;
    private EchoFilter eFilter;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        filter = GetComponent<PBSpeedFilter>();
        trimmer = GetComponent<AudioTrimmer>();
        eFilter = GetComponent<EchoFilter>();
    }

    public void AdjustVolume() 
    {
        if (!isAudioLoaded) return;

        float volumeValue = adjustVolumeSlider.value;


        if (samples == null)
        {
            samples = new float[audioSource.clip.samples];
            audioSource.clip.GetData(samples, 0);
        }

        
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= volumeValue;
        }

        AudioClip modifiedClip = AudioClip.Create("ModifiedVolume", samples.Length, audioSource.clip.channels, audioSource.clip.frequency, false);
        modifiedClip.SetData(samples, 0);

        // Play the modified audio
        audioSource.clip = modifiedClip;
        WaveFormVisualizer visualizer = GetComponent<WaveFormVisualizer>();
        visualizer.GenerateWaveform(audioSource.clip);
        LoadAudioClip(modifiedClip);
        eFilter.LoadAudioClip(audioSource.clip);
        trimmer.LoadAudioClip(audioSource.clip);
        filter.LoadAudioClip(audioSource.clip);
        audioSource.Play();
    }

    public void LoadAudioClip(AudioClip loadedClip)
    {
        // Load the audio clip when the load button is pressed
        audioSource.clip = loadedClip;
        isAudioLoaded = true;

        adjustVolumeSlider.value = 1.0f;

        // Add the listener to the playback speed slider only if the audio clip is loaded
        adjustVolumeSlider.onValueChanged.AddListener(UpdateValueText);

        UpdateValueText(adjustVolumeSlider.value);
    }
    private void UpdateValueText(float newValue)
    {
        if (!isAudioLoaded) return;

        // Update the text component with the slider's value
        valueText.text = newValue.ToString("F2"); // Format the value to show 2 decimal places
    }
}
