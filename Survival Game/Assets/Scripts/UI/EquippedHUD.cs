using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Handles the equipped item HUD.
/// Shows the equipped item's colour and name, highlights the slot while
/// something is equipped and pulses briefly on change, because
/// playtesters were unsure whether equipping had worked.
public class EquippedHUD : MonoBehaviour
{
    [SerializeField] private EquipmentManager equipment;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    [Header("Feedback")]
    [SerializeField] private Image background;
    [SerializeField] private Color emptyColour = new Color(0.12f, 0.12f, 0.12f, 0.8f);
    [SerializeField] private Color equippedColour = new Color(0.85f, 0.65f, 0.2f, 0.9f);
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseTime = 0.2f;

    private Coroutine pulse;

    /// Listens for equipment changes and shows the current state straight away.
    private void OnEnable()
    {
        equipment.Changed += OnEquipmentChanged;
        Refresh();
    }

    /// Stops listening while disabled, and resets any half-finished pulse.
    private void OnDisable()
    {
        equipment.Changed -= OnEquipmentChanged;
        transform.localScale = Vector3.one;
    }

    /// Reacts only to real equip or unequip actions, not to changes in the backpack or other inventory.
    /// so the pulse and message appear when the player actually changes item.
    private void OnEquipmentChanged()
    {
        Refresh();

        PromptUI.Show(equipment.HasEquipped
            ? "Equipped " + ItemShapes.GetName(equipment.Equipped)
            : "Unequipped");

        if (pulse != null) StopCoroutine(pulse);
        pulse = StartCoroutine(Pulse());
    }

    /// Shows the equipped item's colour and name, or an empty slot message.
    private void Refresh()
    {
        if (equipment.HasEquipped)
        {
            icon.enabled = true;
            icon.color = ItemShapes.GetColor(equipment.Equipped);
            label.text = ItemShapes.GetName(equipment.Equipped);
        }
        else
        {
            icon.enabled = false;
            label.text = "Nothing equipped";
        }

        if (background != null)
            background.color = equipment.HasEquipped ? equippedColour : emptyColour;
    }

    /// Briefly enlarges the slot then eases back, to draw the eye to it.
    /// Uses unscaled time so it still plays if the game is paused.
    private IEnumerator Pulse()
    {
        for (float t = 0f; t < pulseTime; t += Time.unscaledDeltaTime)
        {
            float s = Mathf.Lerp(pulseScale, 1f, t / pulseTime);
            transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}