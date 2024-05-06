using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{

    public AudioSource AudioPlayer;


    public void StartGame()
    {   
        SceneManager.LoadScene("Brian's Scene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlaySound()
    {
        AudioPlayer.Play();
    }
}
