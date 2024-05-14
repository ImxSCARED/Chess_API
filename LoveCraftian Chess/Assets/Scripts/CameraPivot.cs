using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public Transform pivotPoint;
    public CameraMovements ocamScript;

    public float maxRotationAngle = 90f;
    public float minRotationAngle = -90f;

    private float currentRotation = 0f;



    private void Awake()
    {
        GameObject camera = GameObject.Find("Main Camera");
        ocamScript = camera.GetComponent<CameraMovements>();
    }
    void Update()
    {
        if(ocamScript.moveToTargetWhite == false && ocamScript.moveToTargetBlack == false)
        {
            // Check if right mouse button is held down
            if (Input.GetMouseButton(1)) // 1 corresponds to the right mouse button
            {
                float mouseX = Input.GetAxis("Mouse X");

                // Calculate the new rotation angle
                currentRotation += rotationSpeed * mouseX;
                currentRotation = Mathf.Clamp(currentRotation, minRotationAngle, maxRotationAngle);

                // Apply the rotation
                pivotPoint.localRotation = Quaternion.Euler(0f, currentRotation, 0f);
            }
        }


        


    }
}
