using UnityEngine;

/// Handles the player's blocking mechanic.
/// Allows the player to block attacks while they have stamina.
public class BlockCombat : MonoBehaviour
{
    public bool isBlocking { get; private set; }
    public PlayerStats stats;

    /// Checks every frame whether the player is holding the right mouse button.
    /// Updates the blocking state if the player has enough stamina.
    private void Update()
    {
        isBlocking = Input.GetMouseButton(1);

        stats.SetBlocking(
            isBlocking && Mathf.Floor(stats.currentStamina) > 0f
        );
    }

    /// Stops the player from blocking when this script is disabled.
    private void OnDisable()
    {
        if (stats != null)
            stats.SetBlocking(false);
    }
}
