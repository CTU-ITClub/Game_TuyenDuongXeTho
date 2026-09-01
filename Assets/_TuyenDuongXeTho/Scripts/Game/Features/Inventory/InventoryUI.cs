using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private Transform slotParent;
    [SerializeField] private InventorySlotUI slotPrefab;

    [Header("Panel Drop/Use (đơn giản, hiện khi click phải vào slot)")]
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private UnityEngine.UI.Button dropButton;
    [SerializeField] private UnityEngine.UI.Button useButton;

    private readonly List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();
    private int _rightClickedSlotIndex = -1;

    private void Start()
    {
        BuildSlots();

        inventorySystem.OnInventoryChanged += RefreshUI;
        inventorySystem.OnEquippedSlotChanged += _ => RefreshUI();

        dropButton.onClick.AddListener(HandleDropButton);
        useButton.onClick.AddListener(HandleUseButton);

        actionPanel.SetActive(false);
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (inventorySystem == null) return;
        inventorySystem.OnInventoryChanged -= RefreshUI;
    }

    private void BuildSlots()
    {
        for (int i = 0; i < inventorySystem.Slots.Count; i++)
        {
            var ui = Instantiate(slotPrefab, slotParent);
            ui.Setup(i, this);
            _slotUIs.Add(ui);
        }
    }

    private void RefreshUI()
    {
        for (int i = 0; i < _slotUIs.Count; i++)
        {
            bool isEquipped = inventorySystem.EquippedSlotIndex == i;
            _slotUIs[i].Refresh(inventorySystem.Slots[i], isEquipped);
        }
    }

    public void OnSlotLeftClicked(int slotIndex)
    {
        var slot = inventorySystem.Slots[slotIndex];
        if (slot.IsEmpty) return;

        if (inventorySystem.EquippedSlotIndex == slotIndex)
            inventorySystem.ClearEquipped(); 
        else
            inventorySystem.SetEquipped(slotIndex); 
    }

    public void OnSlotRightClicked(int slotIndex)
    {
        var slot = inventorySystem.Slots[slotIndex];
        if (slot.IsEmpty) return;

        _rightClickedSlotIndex = slotIndex;

        useButton.gameObject.SetActive(slot.itemData.itemType == ItemType.Consumable);

        actionPanel.SetActive(true);
    }

    private void HandleDropButton()
    {
        if (_rightClickedSlotIndex < 0) return;

        inventorySystem.DropItem(_rightClickedSlotIndex);
        actionPanel.SetActive(false);
        _rightClickedSlotIndex = -1;
    }

    private void HandleUseButton()
    {
        if (_rightClickedSlotIndex < 0) return;

        inventorySystem.UseItem(_rightClickedSlotIndex);
        actionPanel.SetActive(false);
        _rightClickedSlotIndex = -1;
    }
}
