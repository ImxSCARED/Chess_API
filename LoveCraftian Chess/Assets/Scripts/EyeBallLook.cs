using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBallLook : MonoBehaviour
{
    public Camera targetCamera; // The camera that the object should face
    public float rotationSpeed = 1.0f; // Speed of the rotation

    void Update()
    {
        if (targetCamera == null)
        {
            // If no camera is specified, use the main camera
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            Vector3 direction = targetCamera.transform.position - transform.position; // Direction to the camera
            

            Quaternion targetRotation = Quaternion.LookRotation(direction); // Desired rotation towards the camera
            transform.rotation = Quaternion.Slerp(
                transform.rotation, // Current rotation
                targetRotation, // Target rotation
                rotationSpeed * Time.deltaTime); // Speed multiplier
        }
    }
}
