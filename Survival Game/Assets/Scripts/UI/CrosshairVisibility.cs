using UnityEngine;
using UnityEngine.UI;

public class CrosshairVisibility : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        image.enabled = Cursor.lockState == CursorLockMode.Locked;
    }
}