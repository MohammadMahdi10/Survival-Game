using UnityEngine;

/// Handles the player's blocking mechanic.
/// Allows the player to block attacks while they have stamina.
public class BlockCombat : MonoBehaviour
{
    public bool isBlocking { get; private set; }
    public PlayerStats stats;


    /// Checks whether the player is holding the right mouse button.
    /// Only allows blocking when stamina is available, preventing
    /// the player from blocking indefinitely without stamina.

    private void Update()
    {
        isBlocking = Input.GetMouseButton(1);

        stats.SetBlocking(
            isBlocking && Mathf.Floor(stats.currentStamina) > 0f
        );
    }


    /// Clears the blocking state when this script is disabled.
    /// This prevents the player from remaining in a blocking state
    /// after the blocking component stops running.

    private void OnDisable()
    {
        if (stats != null)
            stats.SetBlocking(false);
    }
}
