using UnityEngine;
using Photon.Pun;

public class EquipmentController : MonoBehaviourPun
{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private Transform handSocket;

    public GameObject _currentHeldObject;
    public GameObject _currentUsingObject;
    public KeyCode key = KeyCode.U;
    public ItemData data;

    private void Update()
    {
        // Chỉ Player local mới được điều khiển UI
        if (PhotonNetwork.InRoom && !photonView.IsMine)
            return;

        if (_currentHeldObject == null || data == null)
        {
            return;
        }
        // Consumable
        if (data.itemType == ItemType.Consumable)
        {
            if (isNearObjectUsingItem(data))
            {
                InteractionPromptUI.Instance?.Show(
                    $"Nhấn [U] để sử dụng {data.itemName}"
                );

                if (Input.GetKeyDown(key))
                {
                    Debug.Log(
                        $"[EquipmentController] Sử dụng {data.itemName} trên {_currentUsingObject.name}"
                    );

                    if (inventorySystem != null) inventorySystem.UseItem(0);

                    // tag là "XeTho"
                    if (_currentUsingObject.CompareTag("XeTho"))
                    {
                        Vehicle_durability vehicleDurability = _currentUsingObject.GetComponent<Vehicle_durability>();

                        if (vehicleDurability != null)
                        {
                            vehicleDurability.UpdateDurability(10f);
                        }
                    }
                    // Tag là "Injured_NPC"
                    else if (_currentUsingObject.CompareTag("Injured_NPC"))
                    {
                        State_NPC_Injured stateNPCInjured = _currentUsingObject.GetComponent<State_NPC_Injured>();

                        if (stateNPCInjured != null)
                        {
                            stateNPCInjured.RequestGetMedicalAid();
                        }
                    }
                    // Tag là "XeTho_leaf"
                    else if (_currentUsingObject.CompareTag("XeTho_leaf"))
                    {
                        Leaf_Cover leafCover = _currentUsingObject.GetComponent<Leaf_Cover>();
                        if (leafCover != null)
                        {
                            leafCover.AddLeaf();
                        }
                    }
                }
            }
            else
            {
                InteractionPromptUI.Instance?.Hide();
            }
        }
    }

    private void OnEnable()
    {
        if (inventorySystem != null)
            inventorySystem.OnEquippedSlotChanged += HandleEquippedSlotChanged;
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnEquippedSlotChanged -= HandleEquippedSlotChanged;
    }

    private void HandleEquippedSlotChanged(int slotIndex)
    {
        // Chỉ owner của Player mới gửi yêu cầu equip
        if (PhotonNetwork.InRoom && !photonView.IsMine)
            return;

        string itemName = "";

        if (slotIndex >= 0 &&
            slotIndex < inventorySystem.Slots.Count &&
            !inventorySystem.Slots[slotIndex].IsEmpty)
        {
            ItemData itemData =
                inventorySystem.Slots[slotIndex].itemData;

            if (itemData != null)
                itemName = itemData.name;
        }

        if (PhotonNetwork.IsConnectedAndReady &&
            PhotonNetwork.InRoom)
        {
            Debug.Log(
                $"[EquipmentController] Equip '{itemName}' slot {slotIndex}"
            );

            photonView.RPC(
                nameof(RPC_EquipHandItem),
                RpcTarget.All,
                slotIndex,
                itemName
            );
        }
        else
        {
            RPC_EquipHandItem(slotIndex, itemName);
        }
    }

    [PunRPC]
    private void RPC_EquipHandItem(int slotIndex, string itemName)
    {
        ClearHand();

        if (slotIndex < 0 || string.IsNullOrEmpty(itemName))
            return;

        ItemData syncedData =
            Resources.Load<ItemData>($"Items/{itemName}");

        if (syncedData == null)
        {
            Debug.LogError(
                $"Không tìm thấy ItemData: Resources/Items/{itemName}"
            );
            return;
        }

        if (syncedData.handPrefab == null)
        {
            Debug.LogError(
                $"{syncedData.name} không có handPrefab"
            );
            return;
        }

        data = syncedData;

        _currentHeldObject =
            Instantiate(syncedData.handPrefab, handSocket);

        _currentHeldObject.transform.localPosition =
            Vector3.zero;

        _currentHeldObject.transform.localRotation =
            Quaternion.identity;
    }

    private void ClearHand()
    {
        if (_currentHeldObject != null)
        {
            Destroy(_currentHeldObject);
            _currentHeldObject = null;
        }

        // Reset data của item cũ
        data = null;

        InteractionPromptUI.Instance?.Hide();
    }

    public bool isNearObjectUsingItem(ItemData itemData)
    {
        if (itemData == null ||
            string.IsNullOrEmpty(itemData.objectUsing))
            return false;

        Collider[] cols =
            Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider col in cols)
        {
            if (col.CompareTag(itemData.objectUsing))
            {
                _currentUsingObject = col.gameObject;

                return true;
            }
        }

        return false;
    }
}