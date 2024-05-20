using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class EyeStare1 : MonoBehaviour
{
    public float rotationSpeed = 1.0f; // Speed of the rotation\
    public Camera targetCamera; // The camera that the object should face

    void Update()
    {
        if (targetCamera == null)
        {
            // If no camera is specified, use the main camera
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            // Make the object face the camera
            transform.LookAt(Input.mousePosition);     
        }
    }
}
    
