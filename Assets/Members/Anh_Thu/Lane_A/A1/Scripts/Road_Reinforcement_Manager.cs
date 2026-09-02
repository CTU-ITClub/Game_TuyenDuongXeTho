using UnityEngine;

public class Road_Reinforcement_Manager : MonoBehaviour
{
    // ==========================================
    // CÁC ĐIỂM CẦN GIA CỐ
    // ==========================================

    [Header("Repair Points")]

    // Danh sách các điểm đường cần sửa.
    // Trong trường hợp của chúng ta là 3 điểm.
    public Road_Repair_Point[] repairPoints;


    // ==========================================
    // TRẠNG THÁI NHIỆM VỤ
    // ==========================================

    // false = chưa hoàn thành gia cố
    // true  = đã sửa tất cả các điểm
    private bool reinforcementCompleted = false;


    // Cho những nhiệm vụ tiếp theo có thể kiểm tra:
    // "Gia cố đường đã hoàn thành chưa?"
    public bool IsCompleted
    {
        get
        {
            return reinforcementCompleted;
        }
    }


    // ==========================================
    // UPDATE
    // ==========================================

    private void Update()
    {
        // Nếu đã hoàn thành rồi
        // thì không cần kiểm tra lại nữa.
        if (reinforcementCompleted)
            return;


        // Đếm số điểm đã được sửa.
        int repairedCount = 0;


        foreach (Road_Repair_Point point in repairPoints)
        {
            // Nếu point tồn tại
            // và point đó đã được sửa.
            if (point != null && point.IsRepaired)
            {
                repairedCount++;
            }
        }


        // ==========================================
        // KIỂM TRA ĐÃ SỬA HẾT CHƯA
        // ==========================================

        // Ví dụ:
        //
        // repairedCount = 3
        // repairPoints.Length = 3
        //
        // => Hoàn thành.
        if (repairedCount >= repairPoints.Length)
        {
            CompleteReinforcement();
        }
    }


    // ==========================================
    // HOÀN THÀNH NHIỆM VỤ
    // ==========================================

    private void CompleteReinforcement()
    {
        reinforcementCompleted = true;


        Debug.Log(
            "HOÀN THÀNH: Đã gia cố toàn bộ đoạn đường!"
        );


        Debug.Log(
            "Có thể chuyển sang nhiệm vụ đặt ván / mở lối."
        );
    }
}