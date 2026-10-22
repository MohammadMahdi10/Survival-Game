using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquippedHUD : MonoBehaviour
{
    [SerializeField] private EquipmentManager equipment;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        equipment.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        equipment.Changed -= Refresh;
    }

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