using UnityEngine;

public enum ItemType
{
    Weapon,     
    Consumable, 
    KeyItem     
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string itemId;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType = ItemType.Weapon;

    [Header("Stack (chỉ áp dụng cho Consumable như đạn/thuốc)")]
    public bool isStackable = false;
    public int maxStackSize = 1;

    [Header("Model khi cầm trên tay (gắn vào hand socket)")]
    public GameObject handPrefab;

    [Header("Model khi rơi ngoài map / khi bị Drop")]
    public GameObject worldPrefab;

    [Header("Hiệu ứng khi Use (chỉ dùng cho Consumable)")]
    public int healAmount = 0;
}
