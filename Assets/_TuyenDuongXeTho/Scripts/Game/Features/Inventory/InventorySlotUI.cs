using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject equippedHighlight;

    private int _slotIndex;
    private InventoryUI _owner;

    public void Setup(int index, InventoryUI owner)
    {
        _slotIndex = index;
        _owner = owner;
    }

    public void Refresh(InventorySlot slot, bool isEquipped)
    {
        if (slot.IsEmpty)
        {
            iconImage.enabled = false;
            quantityText.text = "";
            equippedHighlight.SetActive(false);
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = slot.itemData.icon;
        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
        equippedHighlight.SetActive(isEquipped);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _owner.OnSlotLeftClicked(_slotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _owner.OnSlotRightClicked(_slotIndex);
        }
    }
}
