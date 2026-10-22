using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Handles the equipped item HUD.
/// Displays the name and colour of the currently equipped item.
public class EquippedHUD : MonoBehaviour
{
    [SerializeField] private EquipmentManager equipment;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    /// Waits for equipment changes and refreshes the HUD when enabled.
    /// Updates the display to show the currently equipped item.
    private void OnEnable()
    {
        equipment.Changed += Refresh;
        Refresh();
    }

    /// Stops waiting from equipment changes when the HUD is disabled.
    /// Prevents the HUD from receiving updates while inactive.
    private void OnDisable()
    {
        equipment.Changed -= Refresh;
    }

    /// Refreshes the equipped item display.
    /// Shows the item's icon colour and name when an item is equipped.
    /// Hides the icon and displays a message when nothing is equipped.
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
    }
}
