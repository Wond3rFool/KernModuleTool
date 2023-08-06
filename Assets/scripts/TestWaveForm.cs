using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWaveForm : MonoBehaviour
{
    [SerializeField]
    private GameObject waveForm;

    private GameObject[] cubes = new GameObject[1024];

    void Start()
    {
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

    void Update()
    {
        
    }

    public void GenerateWaveForm() 
    {
        for (int i = 0; i < cubes.Length; i++) 
        {
            GameObject go = Instantiate(waveForm);

            //go.transform.position = new Vector3(x,y,z);
        }
    }
}
