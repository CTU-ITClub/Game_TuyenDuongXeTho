using UnityEngine;
using UnityEngine.AI;
using Game.Features.Vehicle;
using System.Collections;
using Game.Features.Player;
using Photon.Pun;

public class NPCMove : MonoBehaviourPun
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Vehicle")]
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private Transform bikeResetPoint;

    [Header("Settings")]
    [SerializeField] private float returnDistance = 1f;
    [SerializeField] private float waitBeforeReturn = 2f;
    [SerializeField] private PlayerController playerController;

    private NavMeshAgent agent;

    private Vector3 startPosition;
    private Quaternion startRotation;

    public bool isMoving = false;
    public bool canCall = false;

    private bool hasArrived = false;
    private bool returningHome = false;

    public Animator animator;
    public Collider collider;

    public DialogueTrigger dialogueTrigger;
    public GameObject particle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        collider = GetComponent<Collider>();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        if (vehicleController == null)
        {
            vehicleController = FindObjectOfType<VehicleController>();
        }

        if (target == null && vehicleController != null)
        {
            target = vehicleController.transform;
        }

        if (target == null)
        {
            Debug.LogWarning("Target is NULL!");
            return;
        }

        // CALL NPC
        if (canCall && !isMoving && Input.GetKeyDown(KeyCode.C))
        {
            photonView.RPC(nameof(MoveRPC), RpcTarget.All);
        }

        // Không có path thì bỏ qua
        if (!isMoving || agent.pathPending)
            return;

        // Đã tới destination
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // NPC đang quay về vị trí cũ
            if (returningHome)
            {
                if (Vector3.Distance(transform.position, startPosition) <= returnDistance)
                {
                    Stop();
                }

                return;
            }

            // Tránh gọi nhiều lần
            if (hasArrived)
                return;

            hasArrived = true;

            // Không tới được hoàn toàn
            if (agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Debug.Log("Không thể tới target");

                ResetVehicle();

                StartCoroutine(WaitAndReturn());
            }
            // Tới được target
            else if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Debug.Log("Đã tới nơi");

                ResetVehicle();

                StartCoroutine(WaitAndReturn());
            }
        }
    }

    // MOVE TO TARGET
    private void Move()
    {
        if (target == null)
        {
            Debug.LogWarning("Target is NULL!");
            return;
        }

        if (animator != null)
        {
            animator.SetBool("Run", true);
        }

        if (particle != null)
        {
            particle.SetActive(false);
        }

        isMoving = true;
        hasArrived = false;
        returningHome = false;

        agent.isStopped = false;

        collider.enabled = false;
        dialogueTrigger.isReach = false;

        Vector3 pos = new Vector3(0f, 0f, -3f);
        agent.SetDestination(target.position + pos);
    }

    [PunRPC]
    private void MoveRPC()
    {
        Move();
        playerController.ChangeNotice("");
    }

    // STOP NPC
    private void Stop()
    {
        if (animator != null)
        {
            animator.SetBool("Run", false);
        }

        agent.ResetPath();

        collider.enabled = true;

        isMoving = false;
        hasArrived = false;
        returningHome = false;

        agent.isStopped = true;
        // Đặt lại hướng ban đầu
        transform.rotation = startRotation;

        if (particle != null)
        {
            particle.SetActive(true);
        }

        Debug.Log("NPC đã quay về vị trí cũ");
    }

    // RETURN TO START POSITION
    private void ReturnToStart()
    {
        if (animator != null)
        {
            animator.SetBool("Run", true);
        }

        returningHome = true;

        agent.isStopped = false;

        agent.SetDestination(startPosition);
    }

    // RESET VEHICLE
    private void ResetVehicle()
    {
        // Dừng NPC lại
        agent.isStopped = true;

        if (vehicleController != null && bikeResetPoint != null)
        {
            vehicleController.ResetVehicleRPC(bikeResetPoint);
        }
    }

    // WAIT THEN RETURN
    private IEnumerator WaitAndReturn()
    {
        if (animator != null)
        {
            animator.SetBool("Run", false);
        }

        yield return new WaitForSeconds(waitBeforeReturn);

        ReturnToStart();
    }

    // PLAYER ENTER AREA
    private void OnTriggerEnter(Collider other)
    {
        PhotonView playerPV = other.GetComponent<PhotonView>();

        if (other.CompareTag("Player") && !isMoving && playerPV != null && playerPV.IsMine && !canCall)
        {
            playerController = other.GetComponent<PlayerController>();
            canCall = true;
        }
    }

    // PLAYER EXIT AREA
    private void OnTriggerExit(Collider other)
    {
        PhotonView playerPV = other.GetComponent<PhotonView>();

        if (other.CompareTag("Player") && playerPV != null && playerPV.IsMine)
        {
            canCall = false;
        }
    }
}