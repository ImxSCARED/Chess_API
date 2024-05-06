using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{

    public AudioSource AudioPlayer;
    public Animation Player;


   
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
    private void PlayAnimation()
    {
        
    }
}
