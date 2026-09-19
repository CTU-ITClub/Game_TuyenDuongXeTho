using UnityEngine;
using Photon.Pun;

public class Plane_Bomber : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Đối tượng cần gán")]
    [SerializeField] private Plane_Detector planeDetector;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombDropPoint;


    // =========================================================
    // DETECTION SETTINGS
    // =========================================================

    [Header("Thời gian phát hiện")]

    [Tooltip("Thời gian phải phát hiện liên tục trước quả bom đầu tiên.")]
    [Min(0f)]
    [SerializeField] private float detectionTimeRequired = 3f;


    // =========================================================
    // BOMB INTERVAL
    // =========================================================

    [Header("Tần suất thả bom")]

    [Tooltip("Khoảng thời gian giữa những quả bom tiếp theo.")]
    [Min(0.1f)]
    [SerializeField] private float bombInterval = 5f;


    // =========================================================
    // BOMB SETTINGS
    // =========================================================

    [Header("Thông số bom")]

    [Tooltip("Thời gian bom bay tới vị trí mục tiêu.")]
    [Min(0.1f)]
    [SerializeField] private float bombTravelTime = 1.5f;

    [Tooltip("Vị trí mục tiêu cao hơn pivot của xe.")]
    [SerializeField] private float targetHeightOffset = 0.5f;


    // =========================================================
    // INTERNAL STATE
    // =========================================================

    private float detectionTimer;
    private float bombIntervalTimer;
    private bool firstBombDropped;

    private bool hadAuthorityLastFrame;


    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        ValidateReferences();

        hadAuthorityLastFrame = HasAuthority();

        ResetBombing();
    }

    private void Update()
    {
        bool hasAuthority = HasAuthority();

        // Nếu vừa đổi Master Client thì reset timer.
        if (hasAuthority != hadAuthorityLastFrame)
        {
            ResetBombing();
            hadAuthorityLastFrame = hasAuthority;
        }

        // Trong Photon Room chỉ Master được thả bom.
        if (!hasAuthority)
            return;

        if (planeDetector == null)
            return;

        bool isBikeDetected =
            planeDetector.IsBikeDetected();

        if (!isBikeDetected)
        {
            ResetBombing();
            return;
        }

        if (!firstBombDropped)
        {
            HandleFirstBomb();
        }
        else
        {
            HandleNextBombs();
        }
    }


    // =========================================================
    // AUTHORITY
    // =========================================================

    private bool HasAuthority()
    {
        // Cho phép test khi chưa vào Photon Room.
        if (!PhotonNetwork.InRoom)
            return true;

        return PhotonNetwork.IsMasterClient;
    }


    // =========================================================
    // FIRST BOMB
    // =========================================================

    private void HandleFirstBomb()
    {
        detectionTimer += Time.deltaTime;

        if (detectionTimer < detectionTimeRequired)
            return;

        bool dropSuccessful = DropBomb();

        if (!dropSuccessful)
            return;

        firstBombDropped = true;
        detectionTimer = 0f;
        bombIntervalTimer = 0f;
    }


    // =========================================================
    // NEXT BOMBS
    // =========================================================

    private void HandleNextBombs()
    {
        bombIntervalTimer += Time.deltaTime;

        if (bombIntervalTimer < bombInterval)
            return;

        bool dropSuccessful = DropBomb();

        if (dropSuccessful)
        {
            bombIntervalTimer = 0f;
        }
    }


    // =========================================================
    // RESET
    // =========================================================

    private void ResetBombing()
    {
        detectionTimer = 0f;
        bombIntervalTimer = 0f;
        firstBombDropped = false;
    }


    // =========================================================
    // DROP BOMB
    // =========================================================

    private bool DropBomb()
    {
        if (!HasAuthority())
            return false;

        if (!CanDropBomb())
            return false;

        // Lưu vị trí xe đúng tại thời điểm bắn.
        // Sau đó bom không tiếp tục đuổi theo xe.
        Vector3 targetPosition =
            planeDetector.targetBike.transform.position
            + Vector3.up * targetHeightOffset;

        Vector3 spawnPosition =
            bombDropPoint.position;

        Quaternion spawnRotation =
            bombDropPoint.rotation;

        GameObject bomb = SpawnBomb(
            spawnPosition,
            spawnRotation
        );

        if (bomb == null)
        {
            Debug.LogError(
                "Plane_Bomber: Không thể tạo Bomb.",
                this
            );

            return false;
        }

        Rigidbody bombRb =
            bomb.GetComponent<Rigidbody>();

        if (bombRb == null)
        {
            Debug.LogError(
                "Plane_Bomber: Bomb Prefab chưa có Rigidbody.",
                bomb
            );

            DestroyBomb(bomb);
            return false;
        }

        // Tắt Gravity để bom bay thẳng, không bay cong.
        bombRb.useGravity = false;

        // Không cho lực cản làm bom chậm dần.
        bombRb.linearDamping = 0f;

        float travelTime =
            Mathf.Max(bombTravelTime, 0.1f);

        Vector3 direction =
            targetPosition - spawnPosition;

        if (direction.sqrMagnitude <= 0.001f)
        {
            Debug.LogWarning(
                "Plane_Bomber: Vị trí bom quá gần mục tiêu.",
                this
            );

            DestroyBomb(bomb);
            return false;
        }

        /*
         * Công thức chuyển động thẳng:
         *
         * velocity = distance / time
         *
         * direction ở đây chứa cả hướng và khoảng cách.
         */
        Vector3 straightVelocity =
            direction / travelTime;

        // Unity 6.
        bombRb.linearVelocity =
            straightVelocity;

        // Xoay đầu bom theo hướng bay.
        bomb.transform.rotation =
            Quaternion.LookRotation(
                direction.normalized
            );

        // Vẽ đường bay trong Scene để kiểm tra.
        Debug.DrawLine(
            spawnPosition,
            targetPosition,
            Color.red,
            travelTime
        );

        Debug.Log(
            $"Máy bay bắn bom thẳng đến: {targetPosition}"
        );

        return true;
    }


    // =========================================================
    // SPAWN BOMB
    // =========================================================

    private GameObject SpawnBomb(
        Vector3 position,
        Quaternion rotation)
    {
        if (PhotonNetwork.InRoom)
        {
            if (!PhotonNetwork.IsMasterClient)
                return null;

            // Prefab cần nằm trong Assets/Resources.
            return PhotonNetwork.InstantiateRoomObject(
                bombPrefab.name,
                position,
                rotation
            );
        }

        // Cho phép test khi chưa vào Photon Room.
        return Instantiate(
            bombPrefab,
            position,
            rotation
        );
    }


    // =========================================================
    // DESTROY BOMB
    // =========================================================

    private void DestroyBomb(GameObject bomb)
    {
        if (bomb == null)
            return;

        PhotonView bombPhotonView =
            bomb.GetComponent<PhotonView>();

        if (PhotonNetwork.InRoom &&
            PhotonNetwork.IsMasterClient &&
            bombPhotonView != null)
        {
            PhotonNetwork.Destroy(bomb);
        }
        else
        {
            Destroy(bomb);
        }
    }


    // =========================================================
    // CHECK REFERENCES
    // =========================================================

    private bool CanDropBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Bomb Prefab.",
                this
            );

            return false;
        }

        if (bombDropPoint == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Bomb Drop Point.",
                this
            );

            return false;
        }

        if (planeDetector == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Plane Detector.",
                this
            );

            return false;
        }

        if (planeDetector.targetBike == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa tìm thấy Target Bike.",
                this
            );

            return false;
        }

        return true;
    }

    private void ValidateReferences()
    {
        if (planeDetector == null)
        {
            planeDetector =
                GetComponent<Plane_Detector>();
        }

        if (planeDetector == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Plane Detector.",
                this
            );
        }

        if (bombPrefab == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Bomb Prefab.",
                this
            );
        }

        if (bombDropPoint == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Bomb Drop Point.",
                this
            );
        }
    }


    // =========================================================
    // INSPECTOR VALIDATION
    // =========================================================

    private void OnValidate()
    {
        detectionTimeRequired =
            Mathf.Max(0f, detectionTimeRequired);

        bombInterval =
            Mathf.Max(0.1f, bombInterval);

        bombTravelTime =
            Mathf.Max(0.1f, bombTravelTime);
    }
}