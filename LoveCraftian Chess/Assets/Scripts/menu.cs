using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    public Animator fadeAnimator;
    public AudioSource backgroundMusic;
    public float fadeDuration = 1.0f; // Duration for fading out the music
    public AudioSource AudioPlayer;


   
    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlaySound()
    {
        AudioPlayer.Play();
      //  Player = gameObject.GetComponent<Animator>();
      //  Player.Play("Cam");
    }

    public void StartGame()
    {
       
        SceneManager.LoadScene("Brian's Scene");
    }
}
