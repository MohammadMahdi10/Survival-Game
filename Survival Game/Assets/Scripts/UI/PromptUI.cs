using TMPro;
using UnityEngine;

/// Shows short on-screen messages (e.g. what was picked up) that fade out.
/// Uses a static instance so any script can report feedback with one call,
/// without every system needing its own reference to the UI.
public class PromptUI : MonoBehaviour
{
    public static PromptUI Instance { get; private set; }

    [SerializeField] private TMP_Text text;
    [SerializeField] private float showTime = 2f;
    [SerializeField] private float fadeTime = 0.5f;

    private float timer;

    private void Awake()
    {
        Instance = this;
        text.alpha = 0f;
    }

    /// Displays a message, replacing any message already showing,
    /// so quick repeated pickups don't stack messages on screen.
    public static void Show(string message)
    {
        // Feedback is optional: if the UI is missing, gameplay should carry on.
        if (Instance == null) return;

        Instance.text.text = message;
        Instance.timer = Instance.showTime + Instance.fadeTime;
        Instance.text.alpha = 1f;
    }

    /// Fades the message out over the last part of its display time.
    /// Uses unscaled time so messages still fade while the game is paused.
    private void Update()
    {
        if (timer <= 0f) return;

        timer -= Time.unscaledDeltaTime;
        text.alpha = Mathf.Clamp01(timer / fadeTime);
    }
}