using UnityEngine;
using UnityEngine.UI;

/// Handles the visibility of the player's crosshair.
/// Shows the crosshair when the cursor is locked and hides it when the cursor is unlocked.
public class CrosshairVisibility : MonoBehaviour
{
    private Image image;

    /// Gets the Image component attached to the crosshair.
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    /// Updates the crosshair's visibility based on the cursor's lock state.
    /// Hides the crosshair when the player unlocks the cursor.
    private void Update()
    {
        image.enabled = Cursor.lockState == CursorLockMode.Locked;
    }
}
