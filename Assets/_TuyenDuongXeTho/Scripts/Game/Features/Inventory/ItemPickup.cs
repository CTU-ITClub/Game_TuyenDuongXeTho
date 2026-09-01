using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    private InventorySystem _playerInventoryInRange;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (_playerInventoryInRange != null && Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var inventory = other.GetComponent<InventorySystem>();
        if (inventory == null)
        {
            Debug.LogWarning($"[ItemPickup] Object tag Player nhưng thiếu InventorySystem: {other.name}");
            return;
        }

        _playerInventoryInRange = inventory;
        InteractionPromptUI.Instance?.Show($"Nhấn [{pickupKey}] để nhặt {itemData.itemName}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInventoryInRange = null;
        InteractionPromptUI.Instance?.Hide();
    }

    private void TryPickup()
    {
        bool added = _playerInventoryInRange.AddItem(itemData, quantity);

        if (added)
        {
            InteractionPromptUI.Instance?.Hide();
            Destroy(gameObject);
        }
        else
        {
            InteractionPromptUI.Instance?.Show("Túi đồ đã đầy! Hãy vứt bớt đồ để nhặt thêm.");
        }
    }
}
