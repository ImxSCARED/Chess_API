using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMovements : MonoBehaviour
{
    public Vector3 whitePosition; // Target camera position
    public Vector3 whiteRotation; // Target camera rotation (Euler angles)
    public Vector3 blackPosition; // Target camera position
    public Vector3 blackRotation; // Target camera rotation (Euler angles)
    public float moveSpeed = 1f;  // Speed of position transition
    public float rotateSpeed = 2f; // Speed of rotation transition

    private bool moveToTargetWhite = false;
    private bool moveToTargetBlack = false;

    void Update()
    {

        if (moveToTargetWhite)
        {
            // Smoothly move the camera to the target position
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, whitePosition, moveSpeed * Time.deltaTime);

            // Smoothly rotate the camera to the target rotation
            Quaternion targetQuat = Quaternion.Euler(whiteRotation);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetQuat, rotateSpeed * Time.deltaTime);

            // Stop moving when close enough to the target position and rotation
            if (Vector3.Distance(Camera.main.transform.position, whitePosition) < 0.1f &&
                Quaternion.Angle(Camera.main.transform.rotation, targetQuat) < 1f)
            {
                moveToTargetWhite = false;
            }
        }

        if (moveToTargetBlack)
        {
            // Smoothly move the camera to the target position
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, blackPosition, moveSpeed * Time.deltaTime);

            // Smoothly rotate the camera to the target rotation
            Quaternion targetQuat = Quaternion.Euler(blackRotation);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetQuat, rotateSpeed * Time.deltaTime);

            // Stop moving when close enough to the target position and rotation
            if (Vector3.Distance(Camera.main.transform.position, blackPosition) < 0.01f &&
                Quaternion.Angle(Camera.main.transform.rotation, targetQuat) < 1f)
            {
                moveToTargetBlack = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            TriggerMoveToTargetA();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            TriggerMoveToTargetB();
        }
    }

    // Trigger this method to start moving and rotating the camera
    public void TriggerMoveToTargetA()
    {
        if (moveToTargetBlack == false && moveToTargetWhite == false)
        {
            moveToTargetWhite = true;
        }

    }
    public void TriggerMoveToTargetB()
    {
        if (moveToTargetBlack == false && moveToTargetWhite == false)
        {
            moveToTargetBlack = true;
        } 
    }
}
