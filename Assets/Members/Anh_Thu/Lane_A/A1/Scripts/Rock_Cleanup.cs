using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class Rock_Cleanup : MonoBehaviour
{
    // =========================================================
    // PLAYER VÀ CÁC CỤC ĐÁ
    // =========================================================

    [Header("Đối tượng")]

    // Nhân vật người chơi
    public Transform player;

    // Danh sách các cục đá cần dọn:
    // rock_01 -> rock_06
    public GameObject[] rocks;


    // =========================================================
    // KHOẢNG CÁCH TƯƠNG TÁC
    // =========================================================

    [Header("Tương tác")]

    // Player phải đứng trong khoảng cách này
    // mới có thể dọn được đá.
    //
    // Scene của bạn đang scale khá lớn
    // nên hiện tại để 150.
    public float interactionDistance = 150f;


    // =========================================================
    // TIẾN ĐỘ DỌN ĐÁ
    // =========================================================

    // Số cục đá đã dọn
    private int removedCount = 0;


    // Cho các nhiệm vụ tiếp theo biết
    // Player đã dọn hết đá hay chưa.
    //
    // Ví dụ:
    // 0/6 -> false
    // 3/6 -> false
    // 6/6 -> true
    public bool IsCompleted
    {
        get
        {
            return removedCount >= rocks.Length;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Nếu đã dọn hết tất cả đá
        // thì không cần nghe phím E nữa.
        if (IsCompleted)
        {
            return;
        }


        // Khi người chơi bấm E
        if (PressedE())
        {
            TryRemoveNearestRock();
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
    // THỬ DỌN CỤC ĐÁ GẦN NHẤT
    // =========================================================

    private void TryRemoveNearestRock()
    {
        // Nếu Player chưa được gán trong Inspector
        // thì không làm gì.
        if (player == null)
        {
            return;
        }


        // Cục đá gần Player nhất
        GameObject nearestRock = null;


        // Khoảng cách tới cục đá gần nhất
        float nearestDistance = Mathf.Infinity;


        // =====================================================
        // 1. TÌM CỤC ĐÁ GẦN PLAYER NHẤT
        // =====================================================

        foreach (GameObject rock in rocks)
        {
            // Nếu Element này chưa được gán
            if (rock == null)
            {
                continue;
            }


            // Nếu cục đá đã bị dọn
            // thì bỏ qua.
            if (!rock.activeSelf)
            {
                continue;
            }


            // Lấy vị trí Player
            Vector3 playerPosition = player.position;


            // Lấy vị trí cục đá
            Vector3 rockPosition = rock.transform.position;


            // -------------------------------------------------
            // Không tính độ cao Y
            // -------------------------------------------------
            //
            // Player chỉ cần đứng gần cục đá
            // trên mặt đất.
            //
            // Không cần quan tâm Player cao hơn
            // hoặc thấp hơn cục đá một chút.

            playerPosition.y = 0f;

            rockPosition.y = 0f;


            // Tính khoảng cách
            float distance =
                Vector3.Distance(
                    playerPosition,
                    rockPosition
                );


            // Nếu cục này gần hơn cục trước
            if (distance < nearestDistance)
            {
                nearestDistance = distance;

                nearestRock = rock;
            }
        }


        // =====================================================
        // 2. KHÔNG CÒN ĐÁ
        // =====================================================

        if (nearestRock == null)
        {
            return;
        }


        // =====================================================
        // 3. PLAYER ĐANG Ở XA ĐÁ
        // =====================================================
        //
        // Đây là phần sửa lỗi quan trọng.
        //
        // Nếu Player đứng xa đống đá:
        //
        //     → return
        //
        // KHÔNG Debug.Log gì cả.
        //
        // Nhờ vậy khi Player đang ở:
        //
        // MaterialPile
        // RepairPoint
        // PlankPile
        //
        // và bấm E thì Rock_Cleanup
        // sẽ hoàn toàn im lặng.

        if (nearestDistance > interactionDistance)
        {
            return;
        }


        // =====================================================
        // 4. PLAYER ĐANG ĐỨNG GẦN ĐÁ
        //    → DỌN CỤC ĐÁ
        // =====================================================

        // Lưu tên trước khi tắt object
        string rockName = nearestRock.name;


        // Cho cục đá biến mất
        nearestRock.SetActive(false);


        // Tăng tiến độ
        removedCount++;


        // Chỉ hiện Console khi Player
        // THỰC SỰ dọn được một cục đá.
        Debug.Log(
            "Đã dọn "
            + rockName
            + " - "
            + removedCount
            + "/"
            + rocks.Length
        );


        // =====================================================
        // 5. KIỂM TRA ĐÃ DỌN HẾT CHƯA
        // =====================================================

        if (IsCompleted)
        {
            Debug.Log(
                "HOÀN THÀNH: Đã dọn hết đất đá!"
            );
        }
    }
}