
using UnityEngine;

public class BlockCombat : MonoBehaviour
{
    public bool isBlocking { get; private set; }
    public PlayerStats stats;

    private void Update()
    {
        isBlocking = Input.GetMouseButton(1);

        stats.SetBlocking(
            isBlocking && Mathf.Floor(stats.currentStamina) > 0f
        );
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.SetBlocking(false);
    }
}
