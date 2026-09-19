using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bike_Camouflage : MonoBehaviourPunCallbacks
{
    [Header("Detection")]
    [Range(0f, 100f)]
    [SerializeField] private float detectionThreshold = 30f;

    [Header("Camouflage")]
    [SerializeField] private float camouflageIndexMax = 100f;
    [SerializeField] private float camouflageIndexCurrent = 100f;

    [Header("Reduction")]
    [SerializeField] private float reductionRate = 10f;
    [SerializeField] private float updateInterval = 1f;

    private Leaf_Cover leafCover;
    private Coroutine reductionCoroutine;
    private bool reductionPaused;

    private void Awake()
    {
        leafCover = GetComponent<Leaf_Cover>();
    }

    private void Start()
    {
        // Hiển thị đúng số lá ngay khi bắt đầu.
        UpdateLeafVisuals();

        if (PhotonNetwork.IsMasterClient)
        {
            StartReducingIfNeeded();
        }
    }

    void Update()
    {
        //test
        if (Input.GetKeyDown(KeyCode.K))
        {
            UpdateCamouflageIndex(-10f);
        }
    }

    private void StartReducingIfNeeded()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (reductionPaused)
            return;

        if (reductionCoroutine != null)
            return;

        if (camouflageIndexCurrent <= 0f)
            return;

        reductionCoroutine =
            StartCoroutine(ReduceCamouflageRoutine());
    }

    private IEnumerator ReduceCamouflageRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (camouflageIndexCurrent > 0f)
        {
            yield return wait;

            if (camouflageIndexCurrent <= 0f)
                break;

            float reduction = reductionRate * updateInterval;

            ApplyCamouflageChangeOnMaster(-reduction);
        }

        reductionCoroutine = null;
    }

    public float GetCamouflageIndex()
    {
        return camouflageIndexCurrent;
    }

    public float GetCamouflageIndexMax()
    {
        return camouflageIndexMax;
    }

    public void UpdateCamouflageIndex(float delta)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ApplyCamouflageChangeOnMaster(delta);
        }
        else
        {
            photonView.RPC(
                nameof(RPC_RequestUpdateCamouflage),
                RpcTarget.MasterClient,
                delta
            );
        }
    }

    [PunRPC]
    private void RPC_RequestUpdateCamouflage(float delta, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        ApplyCamouflageChangeOnMaster(delta);
    }

    private void ApplyCamouflageChangeOnMaster(float delta)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        float newValue = Mathf.Clamp(camouflageIndexCurrent + delta, 0f, camouflageIndexMax);

        // Không thay đổi thì không cần gửi RPC.
        if (Mathf.Approximately(camouflageIndexCurrent, newValue)) return;

        camouflageIndexCurrent = newValue;

        UpdateLeafVisuals();

        photonView.RPC(
            nameof(RPC_SetCamouflageIndex),
            RpcTarget.Others,
            camouflageIndexCurrent
        );

        StartReducingIfNeeded();
    }

    [PunRPC]
    private void RPC_SetCamouflageIndex(float value)
    {
        camouflageIndexCurrent = Mathf.Clamp(value, 0f, camouflageIndexMax);

        UpdateLeafVisuals();
    }

    private void UpdateLeafVisuals()
    {
        leafCover?.RefreshLeaves(camouflageIndexCurrent, camouflageIndexMax);
    }

    public void StopReducing()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        reductionPaused = true;

        if (reductionCoroutine == null) return;

        StopCoroutine(reductionCoroutine);
        reductionCoroutine = null;
    }

    public void ContinueReducing()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        reductionPaused = false;
        StartReducingIfNeeded();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Gửi giá trị hiện tại cho người mới vào phòng.
        photonView.RPC(
            nameof(RPC_SetCamouflageIndex),
            newPlayer,
            camouflageIndexCurrent
        );
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (reductionCoroutine != null)
            {
                StopCoroutine(reductionCoroutine);
                reductionCoroutine = null;
            }

            return;
        }

        StartReducingIfNeeded();
    }

    public bool CanBeDetected()
    {
        return camouflageIndexCurrent <= detectionThreshold;
    }

    public void TakeDamage(float damage)
    {
        damage = Mathf.Max(damage, 0f);
    }
}