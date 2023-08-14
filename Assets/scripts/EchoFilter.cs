using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoFilter : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isAudioLoaded;
    List<float> originalSampleList;

    private PBSpeedFilter filter;
    private AudioTrimmer trimmer;
    private VolumeAdjuster adjuster;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource= GetComponent<AudioSource>();
        filter = GetComponent<PBSpeedFilter>();
        trimmer = GetComponent<AudioTrimmer>();
        adjuster = GetComponent<VolumeAdjuster>();
    }

    public void ApplyEchoFilter() 
    {
        if (!isAudioLoaded) return;

        // Define echo parameters
        int echoDelay = 44100; // Example: 1-second delay for a 44100 Hz sample rate
        float echoVolume = 2f; // Adjust the volume of the echo
        float feedback = 0.2f;
        // Create a new list to hold the samples with echo
        List<float> samplesWithEcho = new List<float>();

        float[] delayBuffer = new float[echoDelay];

        // Iterate through the original list of samples
        for (int i = 0; i < originalSampleList.Count; i++)
        {
            // Calculate the index for the delayed sample
            int delayedIndex = i - echoDelay;

            // Calculate the delayed and adjusted sample value
            float delayedSample = (delayedIndex >= 0) ? delayBuffer[delayedIndex % echoDelay] : 0;

            // Apply the feedback to the delayed sample
            delayedSample *= feedback;

            // Add the original sample to the delayed sample
            delayedSample += originalSampleList[i];

            // Store the delayed sample in the buffer
            delayBuffer[i % echoDelay] = delayedSample;

            // Add the mixed sample (original + delayed) to the new list
            samplesWithEcho.Add(originalSampleList[i] + delayedSample * echoVolume);
        }

        float[] modifiedSamples = samplesWithEcho.ToArray();
        AudioClip trimmedClip = AudioClip.Create("FilteredAudio", modifiedSamples.Length, audioSource.clip.channels, audioSource.clip.frequency, false);
        trimmedClip.SetData(modifiedSamples, 0);

        // Update the audio source to use the trimmed audio clip
        audioSource.clip = trimmedClip;
        WaveFormVisualizer visualizer = GetComponent<WaveFormVisualizer>();
        visualizer.GenerateWaveform(audioSource.clip);
        LoadAudioClip(trimmedClip);
        adjuster.LoadAudioClip(audioSource.clip);
        trimmer.LoadAudioClip(audioSource.clip);
        filter.LoadAudioClip(audioSource.clip);
        audioSource.Play();
    }

    public void UndoEchoFilter()
    {
        if (!isAudioLoaded) return;
        float[] originalSamples = originalSampleList.ToArray();

        AudioClip originalClip = AudioClip.Create("OriginalAudio", originalSamples.Length, audioSource.clip.channels, audioSource.clip.frequency, false);
        originalClip.SetData(originalSamples, 0);

        // Update the audio source to use the trimmed audio clip
        audioSource.clip = originalClip;
        WaveFormVisualizer visualizer = GetComponent<WaveFormVisualizer>();
        visualizer.GenerateWaveform(audioSource.clip);
        LoadAudioClip(originalClip);
        adjuster.LoadAudioClip(audioSource.clip);
        trimmer.LoadAudioClip(audioSource.clip);
        filter.LoadAudioClip(audioSource.clip);
        audioSource.Play();
    }

    public void LoadAudioClip(AudioClip loadedClip)
    {
        // Load the audio clip when the load button is pressed
        audioSource.clip = loadedClip;
        isAudioLoaded = true;

        float[] clipSamples = new float[audioSource.clip.samples];
        audioSource.clip.GetData(clipSamples, 0);
        originalSampleList = new List<float>(clipSamples);

        // Add the listener to the playback speed slider only if the audio clip is loaded

        //Display the numbers
    }

}
