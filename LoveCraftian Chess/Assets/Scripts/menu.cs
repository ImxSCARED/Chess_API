using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    public void StartGame()
    {

        SceneManager.LoadScene("Brian's Scene");
       // Cursor.lockState = CursorLockMode.None;


    }
}
