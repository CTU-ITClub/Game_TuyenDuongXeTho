using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Thêm thư viện PUN

public class InventorySystem : MonoBehaviourPun // Đổi thành MonoBehaviourPun
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
        // 1. Check IsMine trước khi cho phép bấm phím Drop
        if (!photonView.IsMine) return;

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
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.IsEmpty || slot.itemData != item) continue;
                if (slot.quantity >= item.maxStackSize) continue;

                int space = item.maxStackSize - slot.quantity;
                int addAmount = Mathf.Min(space, amount);
                slot.quantity += addAmount;
                amount -= addAmount;

                // 2. Syns cập nhật số lượng slot này cho client khác
                photonView.RPC("RPC_SyncSlot", RpcTarget.Others, i, item.name, slot.quantity);

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

            // 2. Syns slot mới thêm cho client khác
            photonView.RPC("RPC_SyncSlot", RpcTarget.Others, i, item.name, slot.quantity);

            if (amount <= 0)
            {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        OnInventoryChanged?.Invoke();
        return amount <= 0;
    }

    // RPC để đồng bộ Inventory (Add & Clear)
    [PunRPC]
    private void RPC_SyncSlot(int index, string itemName, int quantity)
    {
        if (string.IsNullOrEmpty(itemName) || quantity <= 0)
        {
            _slots[index].Clear();
        }
        else
        {
            // Giả định ItemData nằm trong thư mục Resources/Items/
            ItemData data = Resources.Load<ItemData>($"Items/{itemName}");
            _slots[index].itemData = data;
            _slots[index].quantity = quantity;
        }
        OnInventoryChanged?.Invoke();
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
        if (photonView != null && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && !photonView.IsMine)
            return;

        var slot = _slots[slotIndex];
        if (slot.IsEmpty) return;

        if (slot.itemData.worldPrefab != null)
        {
            Vector3 spawnPosition = GetDropPosition();
            Quaternion spawnRotation = Quaternion.LookRotation(transform.forward);
            //check online thì báo master đồng bộ
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && photonView != null)
            {
                photonView.RPC(nameof(RPC_RequestMasterSpawn), RpcTarget.MasterClient, slot.itemData.worldPrefab.name, spawnPosition, spawnRotation);
            }
            //check test editor thì chạy thẳng
            else
            {
                // Test offline: Instantiate trực tiếp không qua mạng
                Instantiate(slot.itemData.worldPrefab, spawnPosition, spawnRotation);
            }
        }

        if (EquippedSlotIndex == slotIndex)
            ClearEquipped();

        slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    [PunRPC]
    private void RPC_RequestMasterSpawn(string prefabName, Vector3 pos, Quaternion rot)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Cần đặt các worldPrefab vào thư mục Resources/
            PhotonNetwork.Instantiate(prefabName, pos, rot);
        }
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