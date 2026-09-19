using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class Road_Repair_Point : MonoBehaviour
{
    // =========================================================
    // PLAYER
    // =========================================================

    [Header("Player")]

    // Nhân vật người chơi
    public Transform player;


    // =========================================================
    // TRẠNG THÁI ĐƯỜNG
    // =========================================================

    [Header("Road State")]

    // Đường hỏng - màu đỏ
    public GameObject brokenRoad;

    // Đường đã sửa - màu xanh
    public GameObject repairedRoad;


    // =========================================================
    // NHIỆM VỤ VẬN CHUYỂN VẬT LIỆU
    // =========================================================

    [Header("Material Transport")]

    // Script vận chuyển vật liệu
    // Phải giao đủ vật liệu mới được sửa đường
    public Material_Transport materialTransport;


    // =========================================================
    // KHOẢNG CÁCH TƯƠNG TÁC
    // =========================================================

    [Header("Interaction")]

    // Khoảng cách cho phép Player sửa điểm này
    public float interactionDistance = 150f;


    // =========================================================
    // TRẠNG THÁI NỘI BỘ
    // =========================================================

    // false = chưa sửa
    // true  = đã sửa
    private bool isRepaired = false;


    // Cho Road_Reinforcement_Manager kiểm tra
    // điểm này đã sửa hay chưa
    public bool IsRepaired
    {
        get
        {
            return isRepaired;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Nếu điểm này đã sửa rồi
        // thì không cần xử lý nữa
        if (isRepaired)
            return;


        // Chỉ xử lý khi bấm E
        if (PressedE())
        {
            TryRepair();
        }
    }


    // =========================================================
    // KIỂM TRA PHÍM E
    // =========================================================

    private bool PressedE()
    {
#if ENABLE_INPUT_SYSTEM

        return Keyboard.current != null &&
               Keyboard.current.eKey.wasPressedThisFrame;

#else

        return Input.GetKeyDown(KeyCode.E);

#endif
    }


    // =========================================================
    // THỬ SỬA ĐƯỜNG
    // =========================================================

    private void TryRepair()
    {
        // -----------------------------------------------------
        // 1. KIỂM TRA CÁC OBJECT CÓ ĐƯỢC GÁN KHÔNG
        // -----------------------------------------------------

        if (player == null || brokenRoad == null)
        {
            return;
        }


        // -----------------------------------------------------
        // 2. KIỂM TRA KHOẢNG CÁCH TRƯỚC
        // -----------------------------------------------------
        //
        // Đây là chỗ quan trọng nhất.
        //
        // Nếu Player không đứng gần RepairPoint này
        // thì script sẽ im lặng hoàn toàn.
        //
        // Vì vậy khi bấm E ở MaterialPile,
        // 3 RepairPoint ở xa sẽ KHÔNG spam Console nữa.

        Vector3 playerPosition = player.position;

        Vector3 repairPosition =
            brokenRoad.transform.position;


        // Không tính độ cao Y
        playerPosition.y = 0f;

        repairPosition.y = 0f;


        float distance =
            Vector3.Distance(
                playerPosition,
                repairPosition
            );


        // Player đứng xa điểm này
        // → thoát ngay
        // → không hiện bất kỳ thông báo nào
        if (distance > interactionDistance)
        {
            return;
        }


        // -----------------------------------------------------
        // 3. PLAYER ĐÃ ĐỨNG GẦN
        //    → BÂY GIỜ MỚI KIỂM TRA VẬT LIỆU
        // -----------------------------------------------------

        if (materialTransport == null)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Chưa gán Material_Transport!"
            );

            return;
        }


        // Chưa giao đủ 6 vật liệu
        if (!materialTransport.AllMaterialsDelivered)
        {
            Debug.Log(
                "Chưa đủ vật liệu để gia cố đường!"
            );

            return;
        }


        // -----------------------------------------------------
        // 4. ĐỦ ĐIỀU KIỆN
        //    → SỬA ĐƯỜNG
        // -----------------------------------------------------

        RepairRoad();
    }


    // =========================================================
    // SỬA ĐƯỜNG
    // =========================================================

    private void RepairRoad()
    {
        // Tắt đoạn đường hỏng màu đỏ
        if (brokenRoad != null)
        {
            brokenRoad.SetActive(false);
        }


        // Bật đoạn đường đã sửa màu xanh
        if (repairedRoad != null)
        {
            repairedRoad.SetActive(true);
        }


        // Đánh dấu điểm này đã hoàn thành
        isRepaired = true;


        // gameObject.name giúp Console hiện đúng:
        //
        // RepairPoint_01
        // RepairPoint_02
        // RepairPoint_03
        //
        // thay vì lúc nào cũng ghi Point 01.
        Debug.Log(
            "Đã gia cố " +
            gameObject.name +
            "!"
        );
    }
}