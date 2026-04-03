using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    public static PlayerMove instance;

    [Header("Movement")]
    public float moveForce = 20f;
    public float maxSpeed = 6f;
    public float drag = 2f;

    [Header("Rotation")]
    public float rotateSpeed = 10f;

    [Header("Camera")]
    public Transform cameraTransform;

    PhotonView pv;
    Rigidbody rb;

    Vector2 input;

    void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb != null)
        {
            rb.linearDamping = drag;
            rb.angularDamping = 5f;
        }
    }

    void Update()
    {
        if (!pv.IsMine) return;
        if (cameraTransform == null) return;
        if (!RoomManager.instance.ready) return;

        // Input WASD
        input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input.y = 1;
        if (Keyboard.current.sKey.isPressed) input.y = -1;
        if (Keyboard.current.aKey.isPressed) input.x = -1;
        if (Keyboard.current.dKey.isPressed) input.x = 1;
    }

    void FixedUpdate()
    {
        if (!pv.IsMine) return;
        if (!RoomManager.instance.ready) return;

        Move();
    }

    void Move()
    {
        if (input.magnitude == 0) return;

        // Lấy hướng camera (bỏ trục Y)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // Tính hướng di chuyển
        Vector3 moveDir = camForward * input.y + camRight * input.x;

        // Add force
        rb.AddForce(moveDir * moveForce, ForceMode.Acceleration);

        // Giới hạn tốc độ
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Xoay theo hướng di chuyển
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }
}