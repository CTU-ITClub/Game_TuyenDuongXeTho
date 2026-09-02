using UnityEngine;
using Photon.Pun; // Thêm thư viện PUN

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviourPun // Đổi thành MonoBehaviourPun
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
            if (_playerInventoryInRange.GetComponent<PhotonView>().IsMine)
            {
                TryPickup();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var inventory = other.GetComponent<InventorySystem>();
        if (inventory == null) return;

        if (inventory.GetComponent<PhotonView>().IsMine)
        {
            _playerInventoryInRange = inventory;
            InteractionPromptUI.Instance?.Show($"Nhấn [{pickupKey}] để nhặt {itemData.itemName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.GetComponent<PhotonView>().IsMine)
        {
            _playerInventoryInRange = null;
            InteractionPromptUI.Instance?.Hide();
        }
    }

    private void TryPickup()
{
    if (itemData == null)
    {
        Debug.LogError($"[ItemPickup] Chưa gán ItemData trên object: {gameObject.name}");
        return;
    }

    if (_playerInventoryInRange == null) return;

    bool added = _playerInventoryInRange.AddItem(itemData, quantity);
    if (added)
    {
        InteractionPromptUI.Instance?.Hide();
        //check online thì báo master đồng bộ
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && photonView != null)
        {
            photonView.RPC(nameof(RPC_DestroyWorldItem), RpcTarget.MasterClient);
        }
            //check test editor thì chạy thẳng
            else
            {
            Destroy(gameObject);
        }
    }
    else
    {
        InteractionPromptUI.Instance?.Show("Túi đồ đã đầy! Hãy vứt bớt đồ để nhặt thêm.");
    }
}

    [PunRPC]
    private void RPC_DestroyWorldItem()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}