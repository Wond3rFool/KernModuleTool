using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PBSpeedFilter : MonoBehaviour
{
    [SerializeField]
    private Slider playBackSpeedSlider;
    [SerializeField]
    private TMP_Text valueText;

    private AudioSource audioSource;

    private float[] originalSamples;
    private float[] modifiedSamples;
    private bool isAudioLoaded = false;

    private EchoFilter eFilter;
    private AudioTrimmer trimmer;
    private VolumeAdjuster adjuster;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        eFilter = GetComponent<EchoFilter>();
        trimmer = GetComponent<AudioTrimmer>();
        adjuster = GetComponent<VolumeAdjuster>();
    }

    public void ApplyPlayBackSpeed() 
    {
        if (!isAudioLoaded) return;
        float playBackShifter = playBackSpeedSlider.value;

        // Get the original audio samples
        if (originalSamples == null)
        {
            originalSamples = new float[audioSource.clip.samples];
            audioSource.clip.GetData(originalSamples, 0);
        }
        // Calculate the new number of samples based on pitch shift factor
        int newSampleLength = Mathf.RoundToInt(originalSamples.Length / playBackShifter);
        modifiedSamples = new float[newSampleLength];

        // Perform time-domain pitch shifting
        for (int i = 0; i < modifiedSamples.Length; i++)
        {
            float originalSampleIndex = i * playBackShifter;
            int index1 = Mathf.FloorToInt(originalSampleIndex);
            int index2 = Mathf.CeilToInt(originalSampleIndex);

            float fraction = originalSampleIndex - index1;
            modifiedSamples[i] = Mathf.Lerp(originalSamples[index1], originalSamples[index2], fraction);
        }

        // Create a new AudioClip with the modified samples
        AudioClip modifiedClip = AudioClip.Create("ModifiedClip", modifiedSamples.Length, audioSource.clip.channels, audioSource.clip.frequency, false);
        modifiedClip.SetData(modifiedSamples, 0);

        // Play the modified audio
        audioSource.clip = modifiedClip;
        WaveFormVisualizer visualizer = GetComponent<WaveFormVisualizer>();
        visualizer.GenerateWaveform(audioSource.clip);
        LoadAudioClip(modifiedClip);
        eFilter.LoadAudioClip(audioSource.clip);
        adjuster.LoadAudioClip(audioSource.clip);
        trimmer.LoadAudioClip(audioSource.clip);
        audioSource.Play();
    }
    public void LoadAudioClip(AudioClip loadedClip)
    {
        // Load the audio clip when the load button is pressed
        audioSource.clip = loadedClip;
        isAudioLoaded = true;

        // Add the listener to the playback speed slider only if the audio clip is loaded
        playBackSpeedSlider.onValueChanged.AddListener(UpdateValueText);

        UpdateValueText(playBackSpeedSlider.value);
    }
    private void UpdateValueText(float newValue)
    {
        if (!isAudioLoaded) return;

        // Update the text component with the slider's value
        valueText.text = newValue.ToString("F2"); // Format the value to show 2 decimal places
    }
}
