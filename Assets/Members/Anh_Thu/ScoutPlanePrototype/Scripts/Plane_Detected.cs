using UnityEngine;

// Script này được gắn vào máy bay.
//
// Nhiệm vụ:
// 1. Kiểm tra xe có nằm trong khoảng cách quét hay không.
// 2. Kiểm tra xe có nằm trong góc chiếu của đèn hay không.
// 3. Kiểm tra mức ngụy trang và trạng thái Shelter.
// 4. Thông báo khi máy bay phát hiện hoặc mất dấu xe.
public class Plane_Detector : MonoBehaviour
{
    [Header("Đối tượng cần gán")]

    // Điểm bắt đầu và hướng của vùng quét.
    // Trong Unity, kéo object Scan_Light vào ô này.
    public Transform scanOrigin;

    // Xe thồ mà máy bay cần tìm.
    // Trong Unity, kéo Xe_Tho vào ô này.
    public Bike_Camouflage targetBike;

    [Header("Thông số vùng quét")]

    // Khoảng cách tối đa máy bay có thể nhìn thấy xe.
    public float scanDistance = 30f;

    // Tổng độ rộng của góc quét.
    // Ví dụ 60 nghĩa là quét 30 độ bên trái
    // và 30 độ bên phải hướng của Scan_Light.
    [Range(1f, 180f)]
    public float scanAngle = 60f;

    // Lưu trạng thái của lần kiểm tra trước.
    // Dùng để tránh in thông báo liên tục mỗi frame.
    private bool wasDetected = false;

    private void Start()
    {
        // Kiểm tra xem người dùng đã gán Scan_Light chưa.
        if (scanOrigin == null)
        {
            Debug.LogError(
                "Plane_Detector: Chưa gán Scan Origin."
            );
        }

        // Kiểm tra xem người dùng đã gán Xe_Tho chưa.
        if (targetBike == null)
        {
            Debug.LogError(
                "Plane_Detector: Chưa gán Target Bike."
            );
        }
    }

    // Update được Unity gọi liên tục ở mỗi khung hình.
    // Vì máy bay phải quét liên tục nên ta kiểm tra ở đây.
    private void Update()
    {
        CheckBikeDetection();
    }

    // Hàm kiểm tra máy bay có phát hiện được xe hay không.
    private void CheckBikeDetection()
    {
        // Nếu chưa gán đèn hoặc xe thì không kiểm tra tiếp.
        if (scanOrigin == null || targetBike == null)
        {
            return;
        }

        // Lấy một điểm gần giữa thân xe
        // thay vì chỉ lấy điểm dưới chân xe.
        Vector3 bikePosition =
            targetBike.transform.position + Vector3.up * 0.5f;

        // Tính hướng từ đèn quét đến xe.
        Vector3 directionToBike =
            bikePosition - scanOrigin.position;

        // Tính khoảng cách từ đèn đến xe.
        float distanceToBike = directionToBike.magnitude;

        // Kiểm tra xe có nằm trong khoảng cách quét không.
        bool isInsideDistance =
            distanceToBike <= scanDistance;

        // Tính góc giữa:
        // - Hướng chiếu của đèn.
        // - Hướng từ đèn đến xe.
        float angleToBike = Vector3.Angle(
            scanOrigin.forward,
            directionToBike.normalized
        );

        // Chia scanAngle cho 2 vì góc quét được chia đều
        // sang bên trái và bên phải.
        bool isInsideAngle =
            angleToBike <= scanAngle / 2f;

        // CanBeDetected nằm trong Bike_Camouflage.
        //
        // Nó chỉ trả về true khi:
        // - Camouflage thấp hơn Detection Threshold.
        // - Xe không nằm trong Shelter.
        bool camouflageAllowsDetection =
            targetBike.CanBeDetected;

        // Máy bay chỉ phát hiện xe khi đủ cả 3 điều kiện:
        // 1. Xe nằm trong khoảng cách quét.
        // 2. Xe nằm trong góc quét.
        // 3. Xe không còn đủ ngụy trang và không ở Shelter.
        bool isDetected =
            isInsideDistance &&
            isInsideAngle &&
            camouflageAllowsDetection;

        // Nếu lần trước chưa thấy nhưng lần này thấy,
        // in thông báo phát hiện một lần.
        if (isDetected && !wasDetected)
        {
            Debug.Log(
                "Máy bay đã phát hiện xe thồ!"
            );
        }

        // Nếu lần trước đang thấy nhưng lần này không thấy,
        // thông báo máy bay đã mất dấu xe.
        if (!isDetected && wasDetected)
        {
            Debug.Log(
                "Máy bay đã mất dấu xe thồ."
            );
        }

        // Lưu kết quả hiện tại để dùng ở frame tiếp theo.
        wasDetected = isDetected;
    }

    // Hàm này cho các script khác biết xe đang bị phát hiện không.
    // Sau này script thả bom sẽ sử dụng hàm này.
    public bool IsBikeDetected()
    {
        return wasDetected;
    }

    // Vẽ vùng hỗ trợ trong Scene View khi chọn máy bay.
    // Phần này chỉ để quan sát, không ảnh hưởng gameplay.
    private void OnDrawGizmosSelected()
    {
        if (scanOrigin == null)
        {
            return;
        }

        // Vẽ phạm vi quét tối đa.
        Gizmos.DrawWireSphere(
            scanOrigin.position,
            scanDistance
        );

        // Tính đường biên bên trái của góc quét.
        Vector3 leftDirection =
            Quaternion.AngleAxis(
                -scanAngle / 2f,
                scanOrigin.up
            ) * scanOrigin.forward;

        // Tính đường biên bên phải của góc quét.
        Vector3 rightDirection =
            Quaternion.AngleAxis(
                scanAngle / 2f,
                scanOrigin.up
            ) * scanOrigin.forward;

        // Vẽ hai đường giới hạn của góc quét.
        Gizmos.DrawRay(
            scanOrigin.position,
            leftDirection * scanDistance
        );

        Gizmos.DrawRay(
            scanOrigin.position,
            rightDirection * scanDistance
        );
    }
}