using UnityEngine;
using Photon.Pun; // Thêm PUN

public class EquipmentController : MonoBehaviourPun // Đổi thành MonoBehaviourPun
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
        if (photonView != null && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && !photonView.IsMine)
            return;

        string itemName = "";
        if (slotIndex >= 0 && slotIndex < inventorySystem.Slots.Count && !inventorySystem.Slots[slotIndex].IsEmpty)
        {
            if (inventorySystem.Slots[slotIndex].itemData != null)
                itemName = inventorySystem.Slots[slotIndex].itemData.name;
        }

        //check online thì báo master đồng bộ
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && photonView != null)
        {
            photonView.RPC(nameof(RPC_EquipHandItem), RpcTarget.All, slotIndex, itemName);
        }
        //check test editor thì chạy thẳng
        else
        {
            RPC_EquipHandItem(slotIndex, itemName);
        }
    }

    [PunRPC]
    private void RPC_EquipHandItem(int slotIndex, string itemName)
    {
        ClearHand();
        if (slotIndex < 0 || string.IsNullOrEmpty(itemName)) return;

        // Dùng Resources để load lại Data tương ứng dựa theo string name (Vì _slots đã syns, client sẽ hiểu data này)
        ItemData syncedData = Resources.Load<ItemData>($"Items/{itemName}");

        if (syncedData == null || syncedData.handPrefab == null) return;

        _currentHeldObject = Instantiate(syncedData.handPrefab, handSocket);
        _currentHeldObject.transform.localPosition = Vector3.zero;
        _currentHeldObject.transform.localRotation = Quaternion.identity;
    }

    private void ClearHand()
    {
        if (_currentHeldObject == null) return;
        Destroy(_currentHeldObject); // Đây chỉ là cục visual trên tay nên Destroy bình thường
        _currentHeldObject = null;
    }
}