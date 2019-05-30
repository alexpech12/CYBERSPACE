using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameController : MonoBehaviour
{
    public FirstPersonController firstPersonController;
    public MusicPlayer musicPlayer;
    public CityGenerator cityGenerator;
    public Canvas canvas;
    
    void Awake()
    {
        firstPersonController.enabled = false;
        musicPlayer.enabled = false;
        cityGenerator.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        firstPersonController.enabled = true;
        musicPlayer.enabled = true;
        cityGenerator.enabled = true;
        canvas.enabled = false;
    }

}
