using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private int inventorySize = 8;

    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 1.2f;
    [SerializeField] private float dropHeight = 1f;    
    [SerializeField] private LayerMask groundLayer;  
    [SerializeField] private float groundRaycastDistance = 3f;
    [SerializeField] private KeyCode dropKey = KeyCode.F; 

    private List<InventorySlot> _slots;

    public event Action OnInventoryChanged;
    public event Action<int> OnEquippedSlotChanged;

    public IReadOnlyList<InventorySlot> Slots => _slots;

    public int EquippedSlotIndex { get; private set; } = -1;

    private void Awake()
    {
        _slots = new List<InventorySlot>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
            _slots.Add(new InventorySlot(null, 0));
    }

    private void Update()
    {
        if (EquippedSlotIndex >= 0 && Input.GetKeyDown(dropKey))
        {
            DropItem(EquippedSlotIndex);
        }
    }

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;

        if (item.isStackable)
        {
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty || slot.itemData != item) continue;
                if (slot.quantity >= item.maxStackSize) continue;

                int space = item.maxStackSize - slot.quantity;
                int addAmount = Mathf.Min(space, amount);
                slot.quantity += addAmount;
                amount -= addAmount;

                if (amount <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (!slot.IsEmpty) continue;

            int addAmount = item.isStackable ? Mathf.Min(item.maxStackSize, amount) : 1;
            slot.itemData = item;
            slot.quantity = addAmount;
            amount -= addAmount;

            if (amount <= 0)
            {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        OnInventoryChanged?.Invoke();
        return amount <= 0;
    }

    public void SetEquipped(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count || _slots[slotIndex].IsEmpty) return;

        EquippedSlotIndex = slotIndex;
        OnEquippedSlotChanged?.Invoke(EquippedSlotIndex);
    }

    public void ClearEquipped()
    {
        EquippedSlotIndex = -1;
        OnEquippedSlotChanged?.Invoke(EquippedSlotIndex);
    }

    public void DropItem(int slotIndex)
    {
        var slot = _slots[slotIndex];
        if (slot.IsEmpty) return;

        if (slot.itemData.worldPrefab != null)
        {
            Vector3 spawnPosition = GetDropPosition();
            Quaternion spawnRotation = Quaternion.LookRotation(transform.forward);
            Instantiate(slot.itemData.worldPrefab, spawnPosition, spawnRotation);
        }

        if (EquippedSlotIndex == slotIndex)
            ClearEquipped();

        slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    private Vector3 GetDropPosition()
    {
        Vector3 forwardPoint = transform.position + transform.forward * dropDistance;
        Vector3 rayStart = forwardPoint + Vector3.up * dropHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundLayer))
        {
            return hit.point;
        }

        return forwardPoint;
    }

    public void UseItem(int slotIndex)
    {
        var slot = _slots[slotIndex];
        if (slot.IsEmpty) return;
        if (slot.itemData.itemType != ItemType.Consumable)
        {
            Debug.Log($"{slot.itemData.itemName} không phải Consumable, không thể Use.");
            return;
        }

        Debug.Log($"Sử dụng {slot.itemData.itemName}, hồi {slot.itemData.healAmount} máu.");

        slot.quantity -= 1;
        if (slot.quantity <= 0)
        {
            if (EquippedSlotIndex == slotIndex)
                ClearEquipped();
            slot.Clear();
        }

        OnInventoryChanged?.Invoke();
    }
}