using UnityEngine;

// ======================================================
// PLAYER MOVEMENT
// ======================================================
// Điều khiển nhân vật theo hướng của CAMERA.
//
// W = đi về phía trên màn hình
// S = đi về phía dưới màn hình
// A = sang trái màn hình
// D = sang phải màn hình
//
// Script này gắn vào Player.
// Player cần có Rigidbody.
// ======================================================

public class Player_Movement : MonoBehaviour
{
    [Header("Di chuyển")]

    // Tốc độ di chuyển của Player.
    public float moveSpeed = 5f;


    // Rigidbody của Player.
    private Rigidbody rb;


    // Camera chính của game.
    private Transform mainCamera;


    // Hướng mà Player sẽ di chuyển.
    private Vector3 moveDirection;


    private void Start()
    {
        // Lấy Rigidbody trên Player.
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(
                "Player chưa có Rigidbody!"
            );
        }


        // Tìm Main Camera.
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError(
                "Không tìm thấy Main Camera!"
            );
        }
    }


    private void Update()
    {
        // ==========================================
        // 1. ĐỌC PHÍM WASD
        // ==========================================

        float horizontal = 0f;
        float vertical = 0f;


        // A = trái
        if (Input.GetKey(KeyCode.A))
        {
            horizontal -= 1f;
        }


        // D = phải
        if (Input.GetKey(KeyCode.D))
        {
            horizontal += 1f;
        }


        // S = xuống
        if (Input.GetKey(KeyCode.S))
        {
            vertical -= 1f;
        }


        // W = lên
        if (Input.GetKey(KeyCode.W))
        {
            vertical += 1f;
        }


        // Nếu không tìm thấy Camera thì dừng.
        if (mainCamera == null)
        {
            return;
        }


        // ==========================================
        // 2. LẤY HƯỚNG CỦA CAMERA
        // ==========================================

        // Hướng phía trước của Camera.
        Vector3 cameraForward = mainCamera.forward;

        // Hướng bên phải của Camera.
        Vector3 cameraRight = mainCamera.right;


        // Vì nhân vật chỉ đi trên mặt đất,
        // bỏ thành phần Y.
        cameraForward.y = 0f;
        cameraRight.y = 0f;


        // Chuẩn hóa vector.
        cameraForward.Normalize();
        cameraRight.Normalize();


        // ==========================================
        // 3. TÍNH HƯỚNG DI CHUYỂN
        // ==========================================

        // W/S sử dụng hướng trước/sau của Camera.
        //
        // A/D sử dụng hướng trái/phải của Camera.
        moveDirection =
            cameraForward * vertical +
            cameraRight * horizontal;


        // Không cho đi chéo nhanh hơn đi thẳng.
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }


    private void FixedUpdate()
    {
        // Rigidbody nên được di chuyển trong FixedUpdate.
        if (rb == null)
        {
            return;
        }


        // Tính vị trí mới.
        Vector3 newPosition =
            rb.position +
            moveDirection *
            moveSpeed *
            Time.fixedDeltaTime;


        // Di chuyển Player.
        rb.MovePosition(newPosition);
    }
}