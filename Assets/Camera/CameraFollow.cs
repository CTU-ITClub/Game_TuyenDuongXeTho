using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public string targetTag = "Player";

<<<<<<< HEAD
    [Header("Camera Settings")]
    public float distance = 7f;
    public float height = 3f;
=======
    public float distance = 7f;
    public float height = 3f;

>>>>>>> b522a2d7c29611c786f9ef154aed8931b583fa94
    public float mouseSensitivity = 100f;
    public float minY = -30f;
    public float maxY = 60f;

<<<<<<< HEAD
    [Header("Collision Settings")]
    public LayerMask collisionLayers; // Ch?n layer T??ng/Sàn ? ?ây
    public float cameraRadius = 0.2f; // ?? dày c?a camera ?? không b? l?m vào t??ng
    public float minDistance = 0.5f;  // Kho?ng cách g?n nh?t camera có th? ti?n t?i nhân v?t

    float yaw = 0f;
    float pitch = 20f;
    float currentDistance;

    void Start()
    {
        currentDistance = distance;
    }
=======
    float yaw = 0f;
    float pitch = 20f;
>>>>>>> b522a2d7c29611c786f9ef154aed8931b583fa94

    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

<<<<<<< HEAD
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

        // ?i?m g?c ?? tính toán (th??ng là ngang ??u nhân v?t)
        Vector3 focusPoint = target.position + Vector3.up * 1.5f;

        // H??ng t? ?i?m t?p trung ra phía sau camera
        Vector3 direction = rotation * new Vector3(0, 0, -distance);
        // V? trí "lý t??ng" n?u không có v?t c?n
        Vector3 desiredPosition = focusPoint + direction + (rotation * Vector3.up * (height - 1.5f));

        // X? lý va ch?m b?ng Raycast
        RaycastHit hit;
        Vector3 rayDirection = desiredPosition - focusPoint;

        // B?n m?t tia t? nhân v?t t?i v? trí camera
        if (Physics.SphereCast(focusPoint, cameraRadius, rayDirection.normalized, out hit, distance, collisionLayers))
        {
            // N?u ??ng t??ng, gán kho?ng cách b?ng kho?ng cách t?i ?i?m va ch?m (tr? ?i 1 chút sai s?)
            currentDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }
        else
        {
            // N?u không ??ng gì, tr? v? kho?ng cách m?c ??nh
            currentDistance = distance;
        }

        // C?p nh?t v? trí cu?i cùng d?a trên kho?ng cách ?ã tính toán
        transform.position = focusPoint + rayDirection.normalized * currentDistance;
        transform.LookAt(focusPoint);
=======
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minY, maxY);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 offset = rotation * new Vector3(0, height, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * 1.5f);
>>>>>>> b522a2d7c29611c786f9ef154aed8931b583fa94
    }

    void FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(targetTag);
<<<<<<< HEAD
        foreach (GameObject player in players)
        {
            Photon.Pun.PhotonView pv = player.GetComponent<Photon.Pun.PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PlayerMove pl = player.GetComponent<PlayerMove>();
=======

        foreach (GameObject player in players)
        {
            Photon.Pun.PhotonView pv = player.GetComponent<Photon.Pun.PhotonView>();

            if (pv != null && pv.IsMine)
            {
                PlayerMove pl = player.GetComponent<PlayerMove>();

>>>>>>> b522a2d7c29611c786f9ef154aed8931b583fa94
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