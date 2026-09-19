using UnityEngine;
using Photon.Pun;
using Game.Features.Vehicle;
using UnityEngine.UI;
using Game.Features.Player;

public class Vehicle_durability : MonoBehaviourPun
{
    public float maxDurability = 100f;
    public float currentDurability;
    public float testCount = 10f;

    [Header("Break")]
    public string bicyclePartsPrefabName = "Bicycle_parts";

    private VehicleController vehicleController;
    private VehicleVisuals vehicleVisuals;
    private PlayerController player;

    [Header("UI")]
    public GameObject notiDuraBar;
    public Image currentDura;

    void Start()
    {
        currentDurability = maxDurability;

        vehicleController = GetComponent<VehicleController>();
        vehicleVisuals = GetComponent<VehicleVisuals>();
    }

    void Update()
    {
        if (currentDurability <= 0)
            return;

        // Test
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Test: Decrease Durability by " + testCount);

            UpdateDurability(testCount);
        }
    }

    public void UpdateDurability(float amount)
    {
        photonView.RPC(
            "RPC_UpdateDurability",
            RpcTarget.MasterClient,
            amount
        );
    }

    [PunRPC]
    private void RPC_UpdateDurability(float amount)
    {
        // Chỉ Master xử lý durability thật
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log("RPC_UpdateDurability called with amount: " + amount);

        currentDurability += amount;

        currentDurability = Mathf.Clamp(
            currentDurability,
            0f,
            maxDurability
        );

        // Sync durability cho tất cả player
        photonView.RPC(
            "RPC_SyncDurability",
            RpcTarget.All,
            currentDurability
        );

        // Xe hỏng
        if (currentDurability <= 0)
        {
            photonView.RPC(
                "RPC_BreakDown",
                RpcTarget.All
            );
        }
        else if (currentDurability <= 10)
        {
            if (vehicleController != null)
            {
                vehicleController.enabled = false;
            }
        }
        else
        {
            if (vehicleController != null)
            {
                vehicleController.enabled = true;
            }
        }
    }

    [PunRPC]
    private void RPC_SyncDurability(float durability)
    {
        currentDurability = durability;

        if (currentDura != null)
        {
            currentDura.fillAmount =
                currentDurability / maxDurability;
        }
    }

    [PunRPC]
    private void RPC_BreakDown()
    {
        // Tắt điều khiển
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        if (vehicleVisuals != null)
        {
            vehicleVisuals.enabled = false;
        }

        OnNotiDura(false);

        // Chỉ MasterClient được spawn và destroy
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnBicyclePartsAndDestroy();
        }
    }

    private void SpawnBicyclePartsAndDestroy()
    {
        // Cho player rời xe
        if (player != null)
        {
            player.StartRagdollWithBomb(transform.position, 15f, 5f);
        }

        GameObject bicycleParts = PhotonNetwork.Instantiate(
            bicyclePartsPrefabName,
            transform.position,
            transform.rotation
        );

        Debug.Log("Spawn bicycle_parts: " + bicycleParts.name);

        // Xóa chiếc xe hiện tại
        PhotonNetwork.Destroy(gameObject);
    }

    // =========================
    // UI
    // =========================

    void OnNotiDura(bool x)
    {
        if (notiDuraBar != null)
        {
            notiDuraBar.SetActive(x);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();

            if (playerView == null)
                return;

            if (!playerView.IsMine)
                return;

            player = other.GetComponent<PlayerController>();

            OnNotiDura(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();

            if (playerView == null)
                return;

            if (!playerView.IsMine)
                return;

            OnNotiDura(false);
        }
    }
}