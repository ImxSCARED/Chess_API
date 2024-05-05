using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStare : MonoBehaviour
{
    public Camera targetCamera; // The camera that the object should face
    public float rotationSpeed = 1.0f; // Speed of the rotation

    void Update()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main; // If no camera is specified, use the main camera
        }

        if (targetCamera != null)
        {
            Vector3 direction = targetCamera.transform.position - transform.position; // Direction to the camera
            direction.y = 0; // Keep only the horizontal direction

            Quaternion targetRotation = Quaternion.LookRotation(direction); // Desired rotation towards the camera
            transform.rotation = Quaternion.Slerp(
                transform.rotation, // Current rotation
                targetRotation, // Target rotation
                rotationSpeed * Time.deltaTime // Speed multiplier
            );
        }
    }
}
