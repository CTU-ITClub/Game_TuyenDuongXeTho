using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private Transform handSocket;

    private GameObject _currentHeldObject;

    private void OnEnable()
    {
        inventorySystem.OnEquippedSlotChanged += HandleEquippedSlotChanged;
    }

    private void OnDisable()
    {
        inventorySystem.OnEquippedSlotChanged -= HandleEquippedSlotChanged;
    }

    private void HandleEquippedSlotChanged(int slotIndex)
    {
        ClearHand();

        if (slotIndex < 0) return;

        var slot = inventorySystem.Slots[slotIndex];
        if (slot.IsEmpty || slot.itemData.handPrefab == null) return;

        _currentHeldObject = Instantiate(slot.itemData.handPrefab, handSocket);
        _currentHeldObject.transform.localPosition = Vector3.zero;
        _currentHeldObject.transform.localRotation = Quaternion.identity;
    }

    private void ClearHand()
    {
        if (_currentHeldObject == null) return;
        Destroy(_currentHeldObject);
        _currentHeldObject = null;
    }
}
