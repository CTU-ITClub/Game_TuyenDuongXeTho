using UnityEngine;
using System.Collections;
using Photon.Pun;

public class Plane_Boom : MonoBehaviour
{
    [Header("Settings")]
    public GameObject boomPrefab; // Phải được lưu trong thư mục Resources nếu dùng PhotonNetwork.Instantiate, hoặc giữ nguyên nếu dùng RPC
    public float radius = 10f;
    public int boomCount = 5;
    public float throwTimeDelay = 3f;

    [Header("Movement")]
    public GameObject plane;

    public Transform[] movePoints;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private int currentPointIndex = 0;
    private bool isDead = false;
    private bool isMove = false;

    private PhotonView pv;

    void Start()
    {
        // Lấy PhotonView từ script này hoặc từ đối tượng Plane
        pv = GetComponent<PhotonView>();
        if (pv == null && plane != null)
        {
            pv = plane.GetComponent<PhotonView>();
        }

        // Chỉ có Máy chủ (Host/MasterClient) mới chạy bộ đếm thời gian thả bom và tính toán đường đi
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitToThrow());
        }
    }

    void Update()
    {
        // Chỉ MasterClient xử lý việc di chuyển máy bay, vị trí sẽ tự đồng bộ qua PhotonTransformView
        if (PhotonNetwork.IsMasterClient && isMove && !isDead && plane != null)
        {
            MovePlane();
        }
    }

    IEnumerator WaitToThrow()
    {
        while (true)
        {
            yield return new WaitForSeconds(throwTimeDelay);
            if (plane != null && isMove && !isDead)
            {
                CalculateBoomPositions();
            }
        }
    }

    // Master Client sẽ tính toán vị trí ngẫu nhiên của bom
    void CalculateBoomPositions()
    {
        for (int i = 0; i < boomCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 spawnPos = plane.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Gửi vị trí chính xác này đến TẤT CẢ các máy player khác thông qua RPC
            pv.RPC("RPC_SpawnBoom", RpcTarget.All, spawnPos);
        }
    }

    // Hàm RPC nhận vị trí từ Master Client và sinh bom đồng bộ ở mọi máy chơi
    [PunRPC]
    void RPC_SpawnBoom(Vector3 spawnPos)
    {
        if (boomPrefab != null)
        {
            Instantiate(boomPrefab, spawnPos, Quaternion.identity);
        }
    }

    void MovePlane()
    {
        if (movePoints == null || movePoints.Length == 0) return;

        Transform targetPoint = movePoints[currentPointIndex];
        Vector3 direction = targetPoint.position - plane.transform.position;

        plane.transform.position = Vector3.MoveTowards(plane.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            plane.transform.rotation = Quaternion.Slerp(plane.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(plane.transform.position, targetPoint.position) < 0.2f)
        {
            if (currentPointIndex >= movePoints.Length - 1)
            {
                ReachedEndPoint();
            }
            else
            {
                currentPointIndex++;
            }
        }
    }

    void ReachedEndPoint()
    {
        isDead = true;

        // Hủy đối tượng đồng bộ trên mạng qua Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (movePoints != null && movePoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < movePoints.Length; i++)
            {
                Vector3 current = movePoints[i].position;
                Gizmos.DrawSphere(current, 0.3f);

                if (i < movePoints.Length - 1)
                {
                    Vector3 next = movePoints[i + 1].position;
                    Gizmos.DrawLine(current, next);
                }
            }
        }

        if (plane != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(plane.transform.position, radius);
        }
    }

    // Kích hoạt máy bay bay khi Player chạm vào vùng Trigger
    void OnTriggerEnter(Collider other)
    {
        if (isDead || isMove) return;

        if (other.CompareTag("Player"))
        {
            // Đồng bộ trạng thái kích hoạt cho toàn bộ phòng chơi
            if (PhotonNetwork.IsMasterClient)
            {
                pv.RPC("RPC_StartPlaneMove", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void RPC_StartPlaneMove()
    {
        plane.SetActive(true);
        isMove = true;
    }
}