[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;

    public bool IsEmpty => itemData == null || quantity <= 0;

    public InventorySlot(ItemData data, int qty)
    {
        itemData = data;
        quantity = qty;
    }

    public void Clear()
    {
        itemData = null;
        quantity = 0;
    }
}
