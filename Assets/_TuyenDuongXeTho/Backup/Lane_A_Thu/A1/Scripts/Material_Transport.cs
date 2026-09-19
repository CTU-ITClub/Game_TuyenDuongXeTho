using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class Material_Transport : MonoBehaviour
{
    // =========================================================
    // PLAYER
    // =========================================================

    [Header("Player")]

    // Nhân vật người chơi
    public Transform player;

    // Điểm cầm vật liệu - right_hand
    public Transform carryPoint;


    // =========================================================
    // ĐIỀU KIỆN NHIỆM VỤ
    // =========================================================

    [Header("Requirement")]

    // Phải dọn hết đá trước
    // mới được phép nhặt vật liệu.
    public Rock_Cleanup rockCleanup;


    // =========================================================
    // VẬT LIỆU
    // =========================================================

    [Header("Vật liệu")]

    // material_01 -> material_06
    public GameObject[] materials;


    // =========================================================
    // REPAIR ZONE
    // =========================================================

    [Header("Repair Zone")]

    // Khu vực giao vật liệu
    public Transform repairZone;

    // 6 vị trí vật liệu sẽ được đặt xuống
    //
    // Element 0 = Slot_01
    // Element 1 = Slot_02
    // ...
    // Element 5 = Slot_06
    public Transform[] deliverySlots;


    // =========================================================
    // KHOẢNG CÁCH TƯƠNG TÁC
    // =========================================================

    [Header("Khoảng cách")]

    // Khoảng cách được phép nhặt vật liệu
    public float pickupDistance = 150f;

    // Khoảng cách được phép giao vật liệu
    public float deliveryDistance = 150f;


    // =========================================================
    // BIẾN NỘI BỘ
    // =========================================================

    // Vật liệu Player đang cầm
    private GameObject carriedMaterial = null;

    // Index của vật liệu đang cầm
    private int carriedMaterialIndex = -1;

    // Đánh dấu vật liệu nào đã giao
    private bool[] delivered;

    // Số vật liệu đã giao
    private int deliveredCount = 0;


    // =========================================================
    // TRẠNG THÁI HOÀN THÀNH
    // =========================================================

    // Cho Road_Repair_Point kiểm tra
    // đã giao đủ vật liệu hay chưa.
    public bool AllMaterialsDelivered
    {
        get
        {
            return materials != null &&
                   materials.Length > 0 &&
                   deliveredCount >= materials.Length;
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Tạo mảng trạng thái
        // tương ứng với số lượng vật liệu.
        if (materials != null)
        {
            delivered = new bool[materials.Length];
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Đã giao đủ 6 vật liệu
        // → Material_Transport không cần nghe E nữa.
        if (AllMaterialsDelivered)
        {
            return;
        }


        // Chỉ xử lý khi bấm E
        if (!PressedE())
        {
            return;
        }


        // Nếu đang cầm vật liệu
        // → thử giao vật liệu.
        if (carriedMaterial != null)
        {
            TryDeliverMaterial();
        }

        // Nếu chưa cầm gì
        // → thử nhặt vật liệu.
        else
        {
            TryPickupMaterial();
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
    // NHẶT VẬT LIỆU
    // =========================================================

    private void TryPickupMaterial()
    {
        // Chưa có Player
        if (player == null)
        {
            return;
        }


        // Chưa có danh sách vật liệu
        if (materials == null || materials.Length == 0)
        {
            return;
        }


        GameObject nearestMaterial = null;

        int nearestIndex = -1;

        float nearestDistance = Mathf.Infinity;


        // =====================================================
        // 1. TÌM VẬT LIỆU GẦN PLAYER NHẤT
        // =====================================================

        for (int i = 0; i < materials.Length; i++)
        {
            GameObject material = materials[i];


            // Object chưa được gán
            if (material == null)
            {
                continue;
            }


            // Vật liệu này đã giao rồi
            if (delivered != null && delivered[i])
            {
                continue;
            }


            Vector3 playerPosition =
                player.position;


            Vector3 materialPosition =
                material.transform.position;


            // Không tính chiều cao Y
            playerPosition.y = 0f;

            materialPosition.y = 0f;


            float distance =
                Vector3.Distance(
                    playerPosition,
                    materialPosition
                );


            // Tìm vật liệu gần nhất
            if (distance < nearestDistance)
            {
                nearestDistance = distance;

                nearestMaterial = material;

                nearestIndex = i;
            }
        }


        // Không tìm thấy vật liệu
        if (nearestMaterial == null)
        {
            return;
        }


        // =====================================================
        // 2. KIỂM TRA KHOẢNG CÁCH TRƯỚC
        // =====================================================
        //
        // Đây là phần quan trọng.
        //
        // Nếu Player đang ở:
        //
        // - đống đá
        // - RepairPoint
        // - đống ván
        //
        // và bấm E
        //
        // nhưng đang xa MaterialPile
        // thì Material_Transport sẽ im lặng.

        if (nearestDistance > pickupDistance)
        {
            return;
        }


        // =====================================================
        // 3. ĐỨNG GẦN VẬT LIỆU RỒI
        //    MỚI KIỂM TRA ĐÃ DỌN ĐÁ CHƯA
        // =====================================================

        if (rockCleanup == null)
        {
            Debug.LogWarning(
                "Chưa gán Rock_Cleanup!"
            );

            return;
        }


        if (!rockCleanup.IsCompleted)
        {
            Debug.Log(
                "Phải dọn hết đất đá trước khi vận chuyển vật liệu!"
            );

            return;
        }


        // =====================================================
        // 4. NHẶT VẬT LIỆU
        // =====================================================

        carriedMaterial = nearestMaterial;

        carriedMaterialIndex = nearestIndex;


        // Cho vật liệu trở thành con của right_hand
        carriedMaterial.transform.SetParent(carryPoint);


        // Đưa vật liệu về đúng vị trí tay
        carriedMaterial.transform.localPosition =
            Vector3.zero;


        carriedMaterial.transform.localRotation =
            Quaternion.identity;


        // -----------------------------------------------------
        // TẮT COLLIDER KHI ĐANG CẦM
        // -----------------------------------------------------

        Collider materialCollider =
            carriedMaterial.GetComponent<Collider>();


        if (materialCollider != null)
        {
            materialCollider.enabled = false;
        }


        // -----------------------------------------------------
        // NẾU CÓ RIGIDBODY
        // -----------------------------------------------------

        Rigidbody materialRb =
            carriedMaterial.GetComponent<Rigidbody>();


        if (materialRb != null)
        {
            materialRb.isKinematic = true;
        }


        Debug.Log(
            "Đã nhặt "
            + carriedMaterial.name
            + ". Mang tới RepairZone."
        );
    }


    // =========================================================
    // GIAO VẬT LIỆU
    // =========================================================

    private void TryDeliverMaterial()
    {
        // Chưa có Player hoặc RepairZone
        if (player == null || repairZone == null)
        {
            return;
        }


        // Không còn Slot
        if (deliverySlots == null ||
            deliveredCount >= deliverySlots.Length)
        {
            return;
        }


        // =====================================================
        // 1. KIỂM TRA KHOẢNG CÁCH TỚI REPAIR ZONE
        // =====================================================

        Vector3 playerPosition =
            player.position;


        Vector3 zonePosition =
            repairZone.position;


        // Không tính chiều cao
        playerPosition.y = 0f;

        zonePosition.y = 0f;


        float distance =
            Vector3.Distance(
                playerPosition,
                zonePosition
            );


        // Nếu đang ở xa RepairZone
        // thì im lặng.
        //
        // Như vậy khi đang mang vật liệu
        // mà bấm E ở một nơi không liên quan,
        // Console cũng không bị spam.
        if (distance > deliveryDistance)
        {
            return;
        }


        // =====================================================
        // 2. LẤY SLOT TIẾP THEO
        // =====================================================

        Transform targetSlot =
            deliverySlots[deliveredCount];


        if (targetSlot == null)
        {
            Debug.LogWarning(
                "Delivery Slot chưa được gán!"
            );

            return;
        }


        // =====================================================
        // 3. ĐẶT VẬT LIỆU XUỐNG
        // =====================================================

        // Chuyển vật liệu từ tay sang Slot
        carriedMaterial.transform.SetParent(targetSlot);


        // Đặt đúng vị trí Slot
        carriedMaterial.transform.localPosition =
            Vector3.zero;


        carriedMaterial.transform.localRotation =
            Quaternion.identity;


        // Bật lại Collider
        Collider materialCollider =
            carriedMaterial.GetComponent<Collider>();


        if (materialCollider != null)
        {
            materialCollider.enabled = true;
        }


        // Nếu có Rigidbody
        // thì giữ vật liệu cố định tại Slot
        Rigidbody materialRb =
            carriedMaterial.GetComponent<Rigidbody>();


        if (materialRb != null)
        {
            materialRb.isKinematic = true;
        }


        // =====================================================
        // 4. ĐÁNH DẤU ĐÃ GIAO
        // =====================================================

        if (delivered != null &&
            carriedMaterialIndex >= 0 &&
            carriedMaterialIndex < delivered.Length)
        {
            delivered[carriedMaterialIndex] = true;
        }


        string materialName =
            carriedMaterial.name;


        string slotName =
            targetSlot.name;


        // Tăng số vật liệu đã giao
        deliveredCount++;


        // Player không còn cầm vật liệu
        carriedMaterial = null;

        carriedMaterialIndex = -1;


        Debug.Log(
            "Đã đặt "
            + materialName
            + " vào "
            + slotName
            + "."
        );


        Debug.Log(
            "Tiến độ vận chuyển vật liệu: "
            + deliveredCount
            + "/"
            + materials.Length
        );


        // =====================================================
        // 5. KIỂM TRA HOÀN THÀNH
        // =====================================================

        if (AllMaterialsDelivered)
        {
            Debug.Log(
                "HOÀN THÀNH: Đã vận chuyển đủ vật liệu!"
            );
        }
    }
}