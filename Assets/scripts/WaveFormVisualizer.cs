using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaveFormVisualizer : MonoBehaviour
{
    private int resolution = 1024;
    private float[] samples;
    private Camera mainCamera;
    private float screenHeight;

    [SerializeField]
    private GameObject waveForm;

    private GameObject[] cubes = new GameObject[1024];

    private void Awake()
    {
        samples = new float[resolution];
    }

    void Start()
    {
        mainCamera = Camera.main;
        screenHeight = mainCamera.orthographicSize * 1f; // You can adjust this factor as needed

        float x = -512, y = 0, z = 0;

        float xIncrement = waveForm.transform.localScale.x;

        for (int i = 0; i < cubes.Length; i++)
        {
            GameObject go = Instantiate(waveForm);

            go.transform.position = new Vector3(x, y, z);

            x += xIncrement;

            go.name = "waveform" + i;

            go.transform.parent = this.transform;

            cubes[i] = go;
        }
    }

    public void GenerateWaveform(AudioClip audioClip)
    {
        float[] clipSamples = new float[audioClip.samples];
        audioClip.GetData(clipSamples, 0);

        for (int i = 0; i < resolution; i++)
        {
            int sampleIndex = Mathf.RoundToInt((float)i / resolution * audioClip.samples);
            samples[i] = clipSamples[sampleIndex];
        }

        float maxSample = Mathf.NegativeInfinity;
        for (int i = 0; i < samples.Length; i++)
        {
            maxSample = Mathf.Max(maxSample, Mathf.Abs(samples[i]));
        }

        float verticalScaleFactor = screenHeight / (maxSample * 0.05f);


        for (int i = 0; i < resolution; i++)
        {
            float x = cubes[i].transform.position.x;
            float y = (samples[i] * verticalScaleFactor) + 1.5f;

            cubes[i].transform.localScale = new Vector3(1f, y*2, 1f);
        }
    }

}
