using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;

public class AudioTrimmer : MonoBehaviour
{
    [SerializeField]
    private Slider STSlider;
    [SerializeField]
    private Slider ETSlider;
    [SerializeField]
    private TMP_Text STText;
    [SerializeField]
    private TMP_Text ETText;

    private AudioSource audioSource;
    private bool isAudioLoaded = false;

    private PBSpeedFilter filter;
    private EchoFilter eFilter;
    private VolumeAdjuster adjuster;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        filter = GetComponent<PBSpeedFilter>();
        eFilter = GetComponent<EchoFilter>();
        adjuster = GetComponent<VolumeAdjuster>();
    }

    public void LoadAudioClip(AudioClip loadedClip)
    {
        // Load the audio clip when the load button is pressed
        audioSource.clip = loadedClip;
        isAudioLoaded = true;

        STSlider.minValue = 0;
        STSlider.maxValue = audioSource.clip.length / 2 - 0.01f;
        STSlider.value = STSlider.minValue;
        ETSlider.minValue = audioSource.clip.length / 2;
        ETSlider.maxValue = audioSource.clip.length;
        ETSlider.value = ETSlider.maxValue;

        // Add the listener to the playback speed slider only if the audio clip is loaded
        STSlider.onValueChanged.AddListener(UpdateST);
        ETSlider.onValueChanged.AddListener(UpdateET);
        
        //Display the numbers
        UpdateST(STSlider.value);
        UpdateET(ETSlider.value);
    }
    public void ApplyTrimming()
    {
        if (!isAudioLoaded) return;
        float startTime = STSlider.value;
        float endTime = ETSlider.value;

        // Clamp the start and end times within the audio clip's duration
        startTime = Mathf.Clamp(startTime, 0f, audioSource.clip.length);
        endTime = Mathf.Clamp(endTime, 0f, audioSource.clip.length);

        // Calculate the sample indices for the start and end times
        int startSample = Mathf.RoundToInt(startTime * audioSource.clip.frequency);
        int totalSamples = audioSource.clip.samples;
        int endSample = Mathf.RoundToInt(endTime * audioSource.clip.frequency);

        int sampleCutFromEnd = totalSamples - endSample;

        float[] clipSamples = new float[audioSource.clip.samples];
        audioSource.clip.GetData(clipSamples, 0);
        List<float> sampleList = new List<float>(clipSamples);

        sampleList.RemoveRange(0, startSample);
        sampleList.RemoveRange(sampleList.Count - sampleCutFromEnd, sampleCutFromEnd);
        

        float[] modifiedSamples = sampleList.ToArray();

        // Create a new audio clip using the trimmed samples
        AudioClip trimmedClip = AudioClip.Create("TrimmedAudio", modifiedSamples.Length, audioSource.clip.channels, audioSource.clip.frequency, false);
        trimmedClip.SetData(modifiedSamples, 0);

        // Update the audio source to use the trimmed audio clip
        audioSource.clip = trimmedClip;
        WaveFormVisualizer visualizer = GetComponent<WaveFormVisualizer>();
        visualizer.GenerateWaveform(audioSource.clip);
        LoadAudioClip(trimmedClip);
        adjuster.LoadAudioClip(audioSource.clip);
        eFilter.LoadAudioClip(audioSource.clip);
        filter.LoadAudioClip(audioSource.clip);
        audioSource.Play();
    }
    private void UpdateST(float newValue)
    {
        if (!isAudioLoaded) return;

        // Convert the slider value to minutes and seconds
        int minutes = Mathf.FloorToInt(newValue / 60);
        float seconds = newValue % 60;

        // Update the text component with the formatted time
        STText.text = string.Format("{0:00}:{1:00.00}", minutes, seconds); // Format as MM:SS.ss
    }

    private void UpdateET(float newValue)
    {
        if (!isAudioLoaded) return;

        // Convert the slider value to minutes and seconds
        int minutes = Mathf.FloorToInt(newValue / 60);
        float seconds = newValue % 60;

        // Update the text component with the formatted time
        ETText.text = string.Format("{0:00}:{1:00.00}", minutes, seconds); // Format as MM:SS.ss
    }
}
