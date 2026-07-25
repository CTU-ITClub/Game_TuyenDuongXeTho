using UnityEngine;

// Script này dùng để điều khiển máy bay bay tuần tra
// qua các điểm Point_01, Point_02, Point_03, Point_04.
public class Plane_Patrol : MonoBehaviour
{
    // Danh sách các điểm mà máy bay sẽ lần lượt bay tới.
    // Sau khi gắn script vào Plane, chúng ta sẽ kéo
    // các Point_01...Point_04 vào mảng này trong Inspector.
    public Transform[] patrolPoints;

    // Tốc độ di chuyển của máy bay.
    // Giá trị càng lớn thì máy bay bay càng nhanh.
    public float speed = 5f;

    // Tốc độ xoay của máy bay về hướng điểm tiếp theo.
    // Giá trị càng lớn thì máy bay xoay hướng càng nhanh.
    public float rotationSpeed = 4f;

    // Khoảng cách được xem là máy bay đã đến điểm tuần tra.
    // Không cần máy bay chạm chính xác tuyệt đối vào điểm.
    public float reachDistance = 0.5f;

    // Chỉ số của điểm tuần tra hiện tại.
    // 0 là Point_01, 1 là Point_02,
    // 2 là Point_03, 3 là Point_04.
    private int currentPoint = 0;

    // Update được Unity gọi liên tục ở mỗi khung hình.
    // Vì vậy phần di chuyển máy bay được đặt trong hàm này.
    private void Update()
    {
        // Kiểm tra xem mảng patrolPoints đã được tạo
        // và đã được gán ít nhất một điểm hay chưa.
        // Nếu chưa có điểm thì dừng, tránh lỗi.
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        // Lấy điểm mà máy bay đang cần bay tới.
        Transform targetPoint = patrolPoints[currentPoint];

        // Tính hướng từ vị trí hiện tại của máy bay
        // đến vị trí của điểm tuần tra.
        Vector3 direction =
            targetPoint.position - transform.position;

        // Di chuyển máy bay từ vị trí hiện tại
        // đến vị trí của điểm tuần tra.
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,

            // Time.deltaTime giúp tốc độ di chuyển ổn định,
            // không phụ thuộc máy chạy nhanh hay chậm.
            speed * Time.deltaTime
        );

        // Chỉ xoay máy bay khi hướng di chuyển có độ dài đủ lớn.
        // Điều này giúp tránh lỗi khi máy bay đã ở sát điểm đích.
        if (direction.sqrMagnitude > 0.001f)
        {
            // Tạo góc xoay để mặt trước của máy bay
            // hướng về phía điểm tuần tra.
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            // Xoay từ từ về hướng mới thay vì quay lập tức.
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Kiểm tra khoảng cách giữa máy bay và điểm hiện tại.
        // Nếu nhỏ hơn hoặc bằng reachDistance
        // thì xem như máy bay đã tới điểm.
        if (Vector3.Distance(
                transform.position,
                targetPoint.position) <= reachDistance)
        {
            // Chuyển sang điểm tuần tra tiếp theo.
            currentPoint++;

            // Nếu đã đi qua điểm cuối cùng,
            // quay lại điểm đầu tiên để tiếp tục bay vòng.
            if (currentPoint >= patrolPoints.Length)
            {
                currentPoint = 0;
            }
        }
    }
}