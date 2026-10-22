using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Reference to the main camera
    private Transform mainCamera;

    void Start()
    {
        // Get the main camera's transform
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        // Ensure the canvas always faces the camera so that the UI elements are always visible and readable
        if (mainCamera != null)
        {
            // Rotate the canvas to face the camera smoothly
            transform.LookAt(transform.position + mainCamera.forward);
        }
    }
}