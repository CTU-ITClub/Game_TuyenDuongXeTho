using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public string targetTag = "Player";

    [Header("Camera Settings")]
    public float distance = 7f;
    public float height = 3f;

    public float mouseSensitivity = 100f;
    public float minY = -30f;
    public float maxY = 60f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers; // Chọn layer Tường/Sàn ở đây
    public float cameraRadius = 0.2f; // Độ dày của camera để không bị lún vào tường
    public float minDistance = 0.5f;  // Khoảng cách gần nhất camera có thể tiến tới nhân vật

    float yaw = 0f;
    float pitch = 20f;
    float currentDistance;

    void Start()
    {
        currentDistance = distance;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        HandleInput();
        CalculatePosition();
    }

    void HandleInput()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        yaw += mouseDelta.x * mouseSensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minY, maxY);
    }

    void CalculatePosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Điểm gốc để tính toán (thường là ngang đầu nhân vật)
        Vector3 focusPoint = target.position + Vector3.up * 1.5f;

        // Hướng từ điểm tập trung ra phía sau camera
        Vector3 direction = rotation * new Vector3(0, 0, -distance);
        // Vị trí "lý tưởng" nếu không có vật cản
        Vector3 desiredPosition = focusPoint + direction + (rotation * Vector3.up * (height - 1.5f));

        // Xử lý va chạm bằng Raycast
        RaycastHit hit;
        Vector3 rayDirection = desiredPosition - focusPoint;

        // Bắn một tia từ nhân vật tới vị trí camera
        if (Physics.SphereCast(focusPoint, cameraRadius, rayDirection.normalized, out hit, distance, collisionLayers))
        {
            // Nếu đụng tường, gán khoảng cách bằng khoảng cách tới điểm va chạm
            currentDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }
        else
        {
            // Nếu không đụng gì, trở về khoảng cách mặc định
            currentDistance = distance;
        }

        // Cập nhật vị trí cuối cùng dựa trên khoảng cách đã tính toán
        transform.position = focusPoint + rayDirection.normalized * currentDistance;
        transform.LookAt(focusPoint);
    }

    void FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject player in players)
        {
            Photon.Pun.PhotonView pv = player.GetComponent<Photon.Pun.PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PlayerMove pl = player.GetComponent<PlayerMove>();
                if (pl != null)
                {
                    pl.cameraTransform = transform;
                    target = player.transform;
                    break;
                }
            }
        }
    }
}