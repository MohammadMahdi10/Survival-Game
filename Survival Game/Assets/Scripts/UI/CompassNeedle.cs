using UnityEngine;

/// Handles the compass needle's rotation.
/// Updates the needle based on the player's horizontal rotation to indicate their direction.
public class CompassNeedle : MonoBehaviour
{
    [SerializeField] private Transform player;

    /// Updates the compass needle's rotation every frame.
    /// Uses the assigned player transform or the main camera if no player is assigned.
    void Update()
    {
        Transform t = player != null ? player : Camera.main.transform;
        transform.localEulerAngles = new Vector3(0f, 0f, t.eulerAngles.y);
    }
}