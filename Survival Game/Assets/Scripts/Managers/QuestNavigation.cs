using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Handles quest navigation when the player presses V.
/// Finds the current quest objective and calculates its
/// direction and distance relative to the player.
public class QuestNavigation : MonoBehaviour {

    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Transform player;

    [Header("Navigation Settings")]
    [SerializeField] private KeyCode navigationKey = KeyCode.V;

    private bool navigationActive;
    private Transform currentTarget;

    /// Activates or deactivates navigation when V is pressed.
    /// Updates the target automatically when the quest changes
    /// and calculates the distance to the current objective.
    private void Update() {
        if (Input.GetKeyDown(navigationKey)) {
            navigationActive = !navigationActive;
        }

        if (!navigationActive || questManager == null || player == null) {
            return;
        }

        currentTarget = questManager.GetCurrentQuestTarget();

        if (currentTarget == null) {
            return;
        }

        float distance = Vector3.Distance(player.position, currentTarget.position);
        Vector3 direction = (currentTarget.position - player.position).normalized;
    }

    /// Returns whether quest navigation is currently active.
    public bool IsNavigationActive() {
        return navigationActive;
    }

    /// Returns the active objective's position for the
    /// waypoint UI to use when displaying the marker.
    public Transform GetNavigationTarget() {
        return navigationActive ? currentTarget : null;
    }
}
