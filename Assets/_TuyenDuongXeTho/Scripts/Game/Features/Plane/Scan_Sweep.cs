using UnityEngine;

// ============================================================
// SCAN SWEEP
// ============================================================
//
// Script điều khiển vòng Warning quét tới và lui.
//
// Vì Warning là con của Plane:
// - Plane di chuyển -> Warning tự đi theo Plane.
// - Plane đứng yên  -> Warning vẫn tiếp tục quét.
// - Warning KHÔNG xoay, chỉ thay đổi vị trí.
// ============================================================

public class Scan_Sweep : MonoBehaviour
{
    [Header("Scan Settings")]

    // Khoảng cách vòng quét đi về mỗi phía.
    //
    // Ví dụ:
    // sweepDistance = 5
    //
    // Warning sẽ chạy:
    // -5 <-> +5
    //
    // so với vị trí ban đầu.
    public float sweepDistance = 10f;


    // Tốc độ quét.
    public float sweepSpeed = 3f;


    [Header("Sweep Axis")]

    // TRUE:
    // quét theo trục Z của Plane
    // trước <-> sau.
    //
    // FALSE:
    // quét theo trục X của Plane
    // trái <-> phải.
    public bool sweepAlongZ = true;


    // Vị trí ban đầu của Warning
    // so với Plane.
    private Vector3 startLocalPosition;


    // Rotation ban đầu của Warning.
    //
    // Dùng để đảm bảo vòng luôn nằm phẳng,
    // không bị nghiêng trong lúc quét.
    private Quaternion startLocalRotation;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        // Lưu vị trí ban đầu.
        startLocalPosition =
            transform.localPosition;


        // Lưu rotation ban đầu.
        startLocalRotation =
            transform.localRotation;
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        // Tạo chuyển động:
        //
        // 0 -> 1 -> 0 -> -1 -> 0 ...
        //
        float wave =
            Mathf.Sin(
                Time.time * sweepSpeed
            );


        // Khoảng dịch chuyển hiện tại.
        float offset =
            wave * sweepDistance;


        // Bắt đầu từ vị trí gốc.
        Vector3 newPosition =
            startLocalPosition;


        // ========================================================
        // QUÉT
        // ========================================================

        if (sweepAlongZ)
        {
            // Quét trước <-> sau.
            newPosition.z += offset;
        }
        else
        {
            // Quét trái <-> phải.
            newPosition.x += offset;
        }


        // Chỉ thay đổi vị trí.
        transform.localPosition =
            newPosition;


        // Giữ nguyên góc của vòng.
        //
        // Warning sẽ không bị nghiêng/cắm xuống Ground.
        transform.localRotation =
            startLocalRotation;
    }
}