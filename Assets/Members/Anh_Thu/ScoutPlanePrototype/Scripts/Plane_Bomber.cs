using UnityEngine;

// =============================================================
// PLANE BOMBER
// =============================================================
//
// Cơ chế:
//
// 1. Máy bay phát hiện Xe_Tho.
// 2. Nếu phát hiện liên tục đủ detectionTimeRequired giây:
//      -> thả đúng 1 quả bom.
//
// 3. Bom xuất hiện tại BombDropPoint dưới máy bay.
//
// 4. Khi vừa được thả, bom được tính vận tốc
//    để bay tới vị trí của Xe_Tho tại thời điểm đó.
//
// 5. Bom KHÔNG tự đuổi theo Xe_Tho.
//    Vì vậy người chơi vẫn có thể chạy để né.
//
// 6. Trong cùng một lần phát hiện:
//      -> chỉ thả 1 quả.
//
// 7. Máy bay phải mất dấu rồi phát hiện lại
//    mới có thể thả quả tiếp theo.
//
public class Plane_Bomber : MonoBehaviour
{
    // =========================================================
    // ĐỐI TƯỢNG
    // =========================================================

    [Header("Đối tượng cần gán")]

    // Script phát hiện Xe_Tho.
    public Plane_Detector planeDetector;

    // Prefab của quả bom.
    public GameObject bombPrefab;

    // Điểm thả bom nằm dưới máy bay.
    public Transform bombDropPoint;


    // =========================================================
    // PHÁT HIỆN
    // =========================================================

    [Header("Thời gian phát hiện")]

    // Xe phải bị nhìn thấy liên tục bao nhiêu giây
    // trước khi máy bay thả bom.
    public float detectionTimeRequired = 3f;


    // =========================================================
    // BOM
    // =========================================================

    [Header("Thông số bom")]

    // Thời gian dự kiến để bom bay từ máy bay
    // tới vị trí mục tiêu.
    //
    // Nhỏ hơn -> bom bay nhanh hơn.
    // Lớn hơn -> bom rơi chậm hơn.
    public float bombTravelTime = 1.5f;

    // Nhắm hơi cao hơn pivot của Xe_Tho.
    //
    // Nếu pivot của xe nằm sát Ground,
    // giá trị này giúp bom hướng vào thân xe.
    public float targetHeightOffset = 0.5f;


    // =========================================================
    // BIẾN NỘI BỘ
    // =========================================================

    private float detectionTimer = 0f;

    // Đã thả bom trong lần phát hiện hiện tại chưa?
    private bool bombDropped = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (planeDetector == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Plane Detector."
            );
        }

        if (bombPrefab == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Bomb Prefab."
            );
        }

        if (bombDropPoint == null)
        {
            Debug.LogError(
                "Plane_Bomber: Chưa gán Bomb Drop Point."
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (planeDetector == null)
        {
            return;
        }

        bool isBikeDetected =
            planeDetector.IsBikeDetected();


        // =====================================================
        // XE ĐANG BỊ PHÁT HIỆN
        // =====================================================

        if (isBikeDetected)
        {
            // Đã thả 1 quả trong lần phát hiện này
            // thì không thả thêm.
            if (bombDropped)
            {
                return;
            }

            // Đếm thời gian xe bị phát hiện.
            detectionTimer += Time.deltaTime;

            // Đủ thời gian -> thả bom.
            if (detectionTimer >= detectionTimeRequired)
            {
                DropBomb();
            }
        }

        // =====================================================
        // MẤT DẤU XE
        // =====================================================

        else
        {
            // Reset để lần phát hiện tiếp theo
            // có thể thả thêm 1 quả.
            detectionTimer = 0f;
            bombDropped = false;
        }
    }


    // =========================================================
    // THẢ BOM
    // =========================================================

    private void DropBomb()
    {
        // Kiểm tra Bomb Prefab.
        if (bombPrefab == null)
        {
            Debug.LogError(
                "Plane_Bomber: Bomb Prefab đang bị null."
            );

            return;
        }


        // Kiểm tra BombDropPoint.
        if (bombDropPoint == null)
        {
            Debug.LogError(
                "Plane_Bomber: Bomb Drop Point đang bị null."
            );

            return;
        }


        // Kiểm tra Target Bike.
        if (planeDetector.targetBike == null)
        {
            Debug.LogError(
                "Plane_Bomber: Plane Detector chưa có Target Bike."
            );

            return;
        }


        // =====================================================
        // 1. TẠO BOM TẠI MÁY BAY
        // =====================================================

        GameObject bomb =
            Instantiate(
                bombPrefab,
                bombDropPoint.position,
                bombDropPoint.rotation
            );


        // =====================================================
        // 2. LẤY RIGIDBODY CỦA BOM
        // =====================================================

        Rigidbody bombRb =
            bomb.GetComponent<Rigidbody>();

        if (bombRb == null)
        {
            Debug.LogError(
                "Plane_Bomber: Bomb Prefab chưa có Rigidbody."
            );

            Destroy(bomb);

            return;
        }


        // Bom cần Gravity để tạo quỹ đạo rơi.
        bombRb.useGravity = true;


        // =====================================================
        // 3. XÁC ĐỊNH VỊ TRÍ XE TẠI THỜI ĐIỂM THẢ
        // =====================================================

        Vector3 targetPosition =
            planeDetector.targetBike.transform.position
            + Vector3.up * targetHeightOffset;


        // =====================================================
        // 4. TÍNH VẬN TỐC ĐỂ BOM BAY TỚI MỤC TIÊU
        // =====================================================

        // Tránh bombTravelTime bằng 0.
        float travelTime =
            Mathf.Max(
                bombTravelTime,
                0.1f
            );


        Vector3 startPosition =
            bombDropPoint.position;


        // Công thức chuyển động có Gravity:
        //
        // target =
        // start
        // + velocity * time
        // + 1/2 * gravity * time²
        //
        // Từ đó tính velocity ban đầu.

        Vector3 initialVelocity =
            (
                targetPosition
                - startPosition
                - 0.5f
                * Physics.gravity
                * travelTime
                * travelTime
            )
            / travelTime;


        // Unity 6:
        // gán vận tốc ban đầu cho Rigidbody.
        bombRb.linearVelocity =
            initialVelocity;


        // =====================================================
        // 5. ĐÁNH DẤU ĐÃ THẢ BOM
        // =====================================================

        bombDropped = true;


        Debug.Log(
            "Máy bay thả 1 quả bom về vị trí Xe_Tho!"
        );
    }
}