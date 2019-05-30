using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BikeSoundController : MonoBehaviour
{
    [Range(0.5f,3.0f)]
    public float speed;

    AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
        source.volume = Mathf.Lerp(0.2f, 1.0f, Mathf.Clamp01(speed));
        source.pitch = speed;
    }
}
