using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class Plank_Transport : MonoBehaviour
{
    // =========================================================
    // PLAYER
    // =========================================================

    [Header("Player")]

    // Nhân vật người chơi
    public Transform player;

    // Điểm cầm ván
    // Hiện tại chúng ta dùng right_hand
    public Transform carryPoint;


    // =========================================================
    // CÁC TẤM VÁN
    // =========================================================

    [Header("Planks")]

    // Danh sách:
    // plank_01
    // plank_02
    // plank_03
    public GameObject[] planks;


    // =========================================================
    // KHU VỰC ĐẶT VÁN
    // =========================================================

    [Header("Plank Zone")]

    // PlankZone
    // Dùng để quản lý khu vực đặt ván trong Hierarchy
    public Transform plankZone;

    // Các vị trí đặt ván:
    //
    // Element 0 = Slot_01
    // Element 1 = Slot_02
    // Element 2 = Slot_03
    public Transform[] plankSlots;


    // =========================================================
    // ĐIỀU KIỆN NHIỆM VỤ
    // =========================================================

    [Header("Requirement")]

    // Phải gia cố đủ 3 RepairPoint
    // mới được phép bắt đầu vận chuyển ván.
    public Road_Reinforcement_Manager reinforcementManager;


    // =========================================================
    // KHOẢNG CÁCH TƯƠNG TÁC
    // =========================================================

    [Header("Interaction")]

    // Khoảng cách cho phép nhặt ván
    public float pickupDistance = 150f;

    // Khoảng cách cho phép đặt ván
    public float placeDistance = 150f;


    // =========================================================
    // BIẾN NỘI BỘ
    // =========================================================

    // Tấm ván Player hiện đang cầm
    private GameObject carriedPlank = null;


    // Index của tấm ván đang cầm
    //
    // plank_01 = 0
    // plank_02 = 1
    // plank_03 = 2
    private int carriedPlankIndex = -1;


    // Ghi nhớ những tấm ván đã được đặt
    private bool[] placedPlanks;


    // Số lượng ván đã đặt xuống
    private int placedCount = 0;


    // Đã hoàn thành nhiệm vụ đặt ván chưa?
    private bool completed = false;


    // =========================================================
    // TRẠNG THÁI HOÀN THÀNH
    // =========================================================

    // Cho script khác kiểm tra sau này:
    //
    // false = chưa đặt đủ ván
    // true  = đã đặt đủ ván
    public bool IsCompleted
    {
        get
        {
            return completed;
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Tạo mảng trạng thái tương ứng
        // với số lượng ván.
        if (planks != null)
        {
            placedPlanks = new bool[planks.Length];
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Nếu đã hoàn thành nhiệm vụ
        // thì Plank_Transport không nghe phím E nữa.
        if (completed)
        {
            return;
        }


        // Chỉ xử lý khi người chơi bấm E
        if (!PressedE())
        {
            return;
        }


        // =====================================================
        // PLAYER ĐANG CẦM VÁN?
        // =====================================================

        if (carriedPlank != null)
        {
            // Đang cầm ván
            // → thử đặt vào Slot.
            //
            // TryPlacePlank() sẽ tự kiểm tra khoảng cách.
            TryPlacePlank();
        }
        else
        {
            // Chưa cầm ván
            // → thử nhặt.
            //
            // TryPickupPlank() sẽ kiểm tra khoảng cách
            // TRƯỚC KHI kiểm tra nhiệm vụ gia cố.
            TryPickupPlank();
        }
    }


    // =========================================================
    // KIỂM TRA PHÍM E
    // =========================================================

    private bool PressedE()
    {
#if ENABLE_INPUT_SYSTEM

        // Input System mới
        return Keyboard.current != null &&
               Keyboard.current.eKey.wasPressedThisFrame;

#else

        // Input System cũ
        return Input.GetKeyDown(KeyCode.E);

#endif
    }


    // =========================================================
    // NHẶT VÁN
    // =========================================================

    private void TryPickupPlank()
    {
        // Nếu Player chưa được gán
        if (player == null)
        {
            return;
        }


        // Nếu chưa có danh sách ván
        if (planks == null || planks.Length == 0)
        {
            return;
        }


        GameObject nearestPlank = null;

        int nearestIndex = -1;

        float nearestDistance = Mathf.Infinity;


        // =====================================================
        // 1. TÌM TẤM VÁN GẦN PLAYER NHẤT
        // =====================================================

        for (int i = 0; i < planks.Length; i++)
        {
            GameObject plank = planks[i];


            // Object chưa được gán
            if (plank == null)
            {
                continue;
            }


            // Tấm ván này đã được đặt xuống
            // → không được nhặt lại.
            if (placedPlanks != null && placedPlanks[i])
            {
                continue;
            }


            Vector3 playerPosition =
                player.position;


            Vector3 plankPosition =
                plank.transform.position;


            // Không tính độ cao Y
            playerPosition.y = 0f;

            plankPosition.y = 0f;


            float distance =
                Vector3.Distance(
                    playerPosition,
                    plankPosition
                );


            // Tìm tấm gần nhất
            if (distance < nearestDistance)
            {
                nearestDistance = distance;

                nearestPlank = plank;

                nearestIndex = i;
            }
        }


        // Không còn tấm ván nào
        if (nearestPlank == null)
        {
            return;
        }


        // =====================================================
        // 2. KIỂM TRA KHOẢNG CÁCH TRƯỚC
        // =====================================================
        //
        // Đây là phần quan trọng nhất.
        //
        // Nếu Player đang đứng ở:
        //
        // - Rocks
        // - MaterialPile
        // - RepairPoint
        //
        // và bấm E
        //
        // nhưng đang xa đống ván
        // → Plank_Transport sẽ im lặng hoàn toàn.

        if (nearestDistance > pickupDistance)
        {
            return;
        }


        // =====================================================
        // 3. ĐỨNG GẦN VÁN RỒI
        //    MỚI KIỂM TRA ĐÃ GIA CỐ ĐƯỜNG CHƯA
        // =====================================================

        if (reinforcementManager == null)
        {
            Debug.LogWarning(
                "Chưa gán Road_Reinforcement_Manager!"
            );

            return;
        }


        // Chưa sửa đủ 3 RepairPoint
        if (!reinforcementManager.IsCompleted)
        {
            Debug.Log(
                "Phải gia cố đường trước khi đặt ván!"
            );

            return;
        }


        // =====================================================
        // 4. NHẶT VÁN
        // =====================================================

        carriedPlank = nearestPlank;

        carriedPlankIndex = nearestIndex;


        // Cho ván trở thành con của right_hand
        carriedPlank.transform.SetParent(carryPoint);


        // Đưa ván tới vị trí tay
        carriedPlank.transform.localPosition =
            Vector3.zero;


        carriedPlank.transform.localRotation =
            Quaternion.identity;


        // =====================================================
        // 5. TẮT COLLIDER KHI ĐANG CẦM
        // =====================================================

        Collider plankCollider =
            carriedPlank.GetComponent<Collider>();


        if (plankCollider != null)
        {
            plankCollider.enabled = false;
        }


        // =====================================================
        // 6. NẾU VÁN CÓ RIGIDBODY
        // =====================================================

        Rigidbody plankRb =
            carriedPlank.GetComponent<Rigidbody>();


        if (plankRb != null)
        {
            plankRb.isKinematic = true;
        }


        Debug.Log(
            "Đã nhặt "
            + carriedPlank.name
            + ". Mang tới vị trí đặt ván."
        );
    }


    // =========================================================
    // ĐẶT VÁN
    // =========================================================

    private void TryPlacePlank()
    {
        // Player chưa được gán
        if (player == null)
        {
            return;
        }


        // Không có danh sách Slot
        if (plankSlots == null ||
            plankSlots.Length == 0)
        {
            return;
        }


        // Không còn Slot để đặt
        if (placedCount >= plankSlots.Length)
        {
            return;
        }


        // =====================================================
        // 1. LẤY SLOT TIẾP THEO
        // =====================================================
        //
        // placedCount = 0 → Slot_01
        // placedCount = 1 → Slot_02
        // placedCount = 2 → Slot_03

        Transform targetSlot =
            plankSlots[placedCount];


        if (targetSlot == null)
        {
            Debug.LogWarning(
                "Slot đặt ván chưa được gán!"
            );

            return;
        }


        // =====================================================
        // 2. KIỂM TRA KHOẢNG CÁCH PLAYER → SLOT
        // =====================================================

        Vector3 playerPosition =
            player.position;


        Vector3 slotPosition =
            targetSlot.position;


        // Không tính chiều cao Y
        playerPosition.y = 0f;

        slotPosition.y = 0f;


        float distance =
            Vector3.Distance(
                playerPosition,
                slotPosition
            );


        // =====================================================
        // 3. ĐANG ĐỨNG XA SLOT
        // =====================================================
        //
        // Nếu đang cầm ván nhưng bấm E
        // ở một nơi khác:
        //
        // → không thả ván
        // → không hiện Console
        // → chỉ return.

        if (distance > placeDistance)
        {
            return;
        }


        // =====================================================
        // 4. ĐẶT VÁN VÀO SLOT
        // =====================================================

        // Chuyển ván từ right_hand sang Slot
        carriedPlank.transform.SetParent(targetSlot);


        // Đặt chính xác vào vị trí Slot
        carriedPlank.transform.localPosition =
            Vector3.zero;


        // Cho ván xoay theo hướng của Slot
        carriedPlank.transform.localRotation =
            Quaternion.identity;


        // =====================================================
        // 5. BẬT LẠI COLLIDER
        // =====================================================

        Collider plankCollider =
            carriedPlank.GetComponent<Collider>();


        if (plankCollider != null)
        {
            plankCollider.enabled = true;
        }


        // =====================================================
        // 6. GIỮ VÁN CỐ ĐỊNH
        // =====================================================

        Rigidbody plankRb =
            carriedPlank.GetComponent<Rigidbody>();


        if (plankRb != null)
        {
            plankRb.isKinematic = true;
        }


        // =====================================================
        // 7. ĐÁNH DẤU TẤM VÁN ĐÃ ĐƯỢC ĐẶT
        // =====================================================

        if (placedPlanks != null &&
            carriedPlankIndex >= 0 &&
            carriedPlankIndex < placedPlanks.Length)
        {
            placedPlanks[carriedPlankIndex] = true;
        }


        string plankName =
            carriedPlank.name;


        string slotName =
            targetSlot.name;


        // Tăng tiến độ
        placedCount++;


        // Player không còn cầm ván
        carriedPlank = null;

        carriedPlankIndex = -1;


        Debug.Log(
            "Đã đặt "
            + plankName
            + " vào "
            + slotName
            + "."
        );


        Debug.Log(
            "Tiến độ đặt ván: "
            + placedCount
            + "/"
            + planks.Length
        );


        // =====================================================
        // 8. KIỂM TRA HOÀN THÀNH
        // =====================================================

        if (placedCount >= planks.Length)
        {
            completed = true;


            Debug.Log(
                "HOÀN THÀNH: Đã đặt đủ ván và mở lối!"
            );
        }
    }
}