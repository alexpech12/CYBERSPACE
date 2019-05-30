using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MusicPlayer : MonoBehaviour
{
    public List<AudioClip> audioList;

    int current = -1;

    AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!source.isPlaying || Input.GetKeyDown(KeyCode.N))
        {
            if (current == -1 || current >= audioList.Count-1)
            {
                current = 0;
            }
            else
            {
                current++;
            }
            source.Stop();
            source.PlayOneShot(audioList[current]);
        }
    }
}
