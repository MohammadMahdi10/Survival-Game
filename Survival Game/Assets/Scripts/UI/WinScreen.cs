using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Handles the win screen UI.
/// Allows the player to exit the game after winning.
public class WinScreenUI : MonoBehaviour {
    /// Closes the game when the player selects the exit button.
    /// Stops Play mode when running the game inside the Unity Editor.
    public void ExitGame() {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
