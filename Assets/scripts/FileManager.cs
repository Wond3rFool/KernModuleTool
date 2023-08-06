using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SFB;
using System;
using System.Collections;
using TMPro;
using System.IO;

public class FileManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI statusText;
    [SerializeField]
    private AudioSource audioSource;

    private WaveFormVisualizer visualizer;
    private PBSpeedFilter filter;
    private AudioTrimmer trimmer;
    private VolumeAdjuster adjuster;
    private EchoFilter eFilter;

    private void Awake()
    {
        visualizer = GetComponent<WaveFormVisualizer>();
        filter = GetComponent<PBSpeedFilter>();
        trimmer = GetComponent<AudioTrimmer>();
        adjuster = GetComponent<VolumeAdjuster>();
        eFilter = GetComponent<EchoFilter>();
    }

    public void LoadAudioFile()
    {
        ExtensionFilter[] extensions = new ExtensionFilter[] {
            new ExtensionFilter("Audio Files", "wav", "mp3", "ogg"),
            new ExtensionFilter("All Files", "*")
        };
        StandaloneFileBrowser.OpenFilePanelAsync("Open Audio File", "", extensions, false, LoadAudioFileCallback);
    }

    public void SaveAudioFile()
    {
        ExtensionFilter[] extensions = new ExtensionFilter[] {
            new ExtensionFilter("WAV", "wav"),
            new ExtensionFilter("MP3", "mp3"),
            new ExtensionFilter("OGG", "ogg")
        };
        StandaloneFileBrowser.SaveFilePanelAsync("Save Audio File", "", "audio", extensions, SaveAudioFileCallback);
    }


    private void LoadAudioFileCallback(string[] paths)
    {
        if (paths.Length > 0)
        {
            string filePath = paths[0];
            statusText.text = "Loading audio file: " + filePath;

            StartCoroutine(LoadAudioClip(filePath));
        }
        else
        {
            statusText.text = "Load audio file cancelled.";
        }
    }

    private IEnumerator LoadAudioClip(string path)
    {
        using (UnityWebRequest loader = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.UNKNOWN))
        {
            yield return loader.SendWebRequest();
            if (loader.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(loader.error);
            }
            else
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(loader);
                audioSource.Play();
                statusText.text = "Audio file loaded successfully.";

                eFilter.LoadAudioClip(audioSource.clip);
                adjuster.LoadAudioClip(audioSource.clip);
                trimmer.LoadAudioClip(audioSource.clip);
                filter.LoadAudioClip(audioSource.clip);
                visualizer.GenerateWaveform(audioSource.clip);
            }
        }
    }

    private void SaveAudioFileCallback(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            statusText.text = "Saving audio file: " + path;

            StartCoroutine(SaveAudioClip(audioSource.clip, path));
        }
        else
        {
            statusText.text = "Save audio file cancelled.";
        }
    }

    private IEnumerator SaveAudioClip(AudioClip audioClip, string path)
    {
        // Convert the AudioClip to a float array
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        // Convert the float array to a byte array
        byte[] byteArray = new byte[samples.Length * 2];
        int bytePos = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            short val = (short)(samples[i] * short.MaxValue);
            byteArray[bytePos++] = (byte)(val & 0xFF);
            byteArray[bytePos++] = (byte)((val >> 8) & 0xFF);
        }

        // Write the byte array to a file
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                // Write the WAV file header
                binaryWriter.Write("RIFF".ToCharArray());
                binaryWriter.Write((int)(byteArray.Length + 36));
                binaryWriter.Write("WAVE".ToCharArray());
                binaryWriter.Write("fmt ".ToCharArray());
                binaryWriter.Write(16);
                binaryWriter.Write((short)1);
                binaryWriter.Write((short)audioClip.channels);
                binaryWriter.Write(audioClip.frequency);
                binaryWriter.Write(audioClip.frequency * audioClip.channels * 2);
                binaryWriter.Write((short)(audioClip.channels * 2));
                binaryWriter.Write((short)16);
                binaryWriter.Write("data".ToCharArray());
                binaryWriter.Write(byteArray.Length);

                // Write the audio data
                binaryWriter.Write(byteArray);
            }
        }

        yield return null;
    }

    private void OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
    {
        StandaloneFileBrowser.OpenFilePanelAsync(title, directory, extensions, multiselect, cb);
    }
}