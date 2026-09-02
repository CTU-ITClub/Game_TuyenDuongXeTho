using UnityEngine;

// Script điều khiển máy bay tuần tra qua các Patrol Point.
//
// Đường bay:
//
// Point_01
//    ↓
// Point_02
//    ↓
// Point_03
//    ↓
// Point_04
//    ↓
// Point_03
//    ↓
// Point_02
//    ↓
// Point_01
//
// Sau đó tiếp tục lặp lại.
//
// Kiểu di chuyển này gọi là Ping-Pong Patrol.
public class Plane_Patrol : MonoBehaviour
{
    // =========================================================
    // PATROL POINTS
    // =========================================================

    [Header("Patrol Points")]

    // Danh sách các điểm máy bay sẽ bay qua.
    //
    // Trong Inspector:
    //
    // Element 0 = Point_01
    // Element 1 = Point_02
    // Element 2 = Point_03
    // Element 3 = Point_04
    public Transform[] patrolPoints;


    // =========================================================
    // THÔNG SỐ DI CHUYỂN
    // =========================================================

    [Header("Movement")]

    // Tốc độ bay của máy bay.
    //
    // Có thể chỉnh trực tiếp trong Inspector
    // sau khi chạy thử gameplay.
    public float speed = 2f;


    // Tốc độ máy bay xoay về phía
    // Patrol Point tiếp theo.
    public float rotationSpeed = 4f;


    // Máy bay không cần chạm chính xác
    // vào Patrol Point.
    //
    // Khi khoảng cách nhỏ hơn giá trị này
    // thì xem như đã tới điểm.
    public float reachDistance = 0.5f;


    // =========================================================
    // BIẾN NỘI BỘ
    // =========================================================

    // Patrol Point hiện tại.
    //
    // 0 = Point_01
    // 1 = Point_02
    // 2 = Point_03
    // 3 = Point_04
    private int currentPoint = 0;


    // Hướng đang đi trong mảng Patrol Point.
    //
    // 1  = đang đi:
    // Point_01 → Point_04
    //
    // -1 = đang quay lại:
    // Point_04 → Point_01
    private int patrolDirection = 1;


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Nếu chưa có Patrol Point
        // thì không làm gì.
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }


        // Đảm bảo currentPoint luôn nằm
        // trong phạm vi hợp lệ của mảng.
        if (currentPoint < 0 ||
            currentPoint >= patrolPoints.Length)
        {
            currentPoint = 0;
        }


        // Lấy Patrol Point hiện tại.
        Transform targetPoint =
            patrolPoints[currentPoint];


        // Nếu Element này chưa được gán
        // thì bỏ qua để tránh NullReferenceException.
        if (targetPoint == null)
        {
            MoveToNextPoint();
            return;
        }


        // =====================================================
        // 1. TÍNH HƯỚNG BAY
        // =====================================================

        Vector3 direction =
            targetPoint.position -
            transform.position;


        // =====================================================
        // 2. DI CHUYỂN MÁY BAY
        // =====================================================

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                speed * Time.deltaTime
            );


        // =====================================================
        // 3. XOAY MÁY BAY
        // =====================================================

        // Chỉ xoay khi còn cách mục tiêu
        // một khoảng đủ lớn.
        if (direction.sqrMagnitude > 0.001f)
        {
            // Tính hướng máy bay cần quay tới.
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized
                );


            // Xoay từ từ,
            // không quay tức thời.
            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }


        // =====================================================
        // 4. KIỂM TRA ĐÃ TỚI PATROL POINT CHƯA
        // =====================================================

        float distanceToTarget =
            Vector3.Distance(
                transform.position,
                targetPoint.position
            );


        if (distanceToTarget <= reachDistance)
        {
            MoveToNextPoint();
        }
    }


    // =========================================================
    // CHUYỂN SANG PATROL POINT TIẾP THEO
    // =========================================================

    private void MoveToNextPoint()
    {
        // Nếu chỉ có 1 Patrol Point,
        // máy bay không cần chuyển điểm.
        if (patrolPoints == null ||
            patrolPoints.Length <= 1)
        {
            return;
        }


        // =====================================================
        // ĐANG BAY TỚI POINT CUỐI
        // =====================================================

        // Ví dụ:
        //
        // currentPoint = 3
        // patrolPoints.Length = 4
        //
        // Nghĩa là đã tới Point_04.
        if (currentPoint >=
            patrolPoints.Length - 1)
        {
            // Đổi hướng để quay lại.
            patrolDirection = -1;
        }


        // =====================================================
        // ĐANG BAY VỀ POINT ĐẦU
        // =====================================================

        // Nếu đã tới Point_01
        // thì đổi hướng để lại bay tới Point_04.
        else if (currentPoint <= 0)
        {
            patrolDirection = 1;
        }


        // =====================================================
        // CHUYỂN INDEX
        // =====================================================

        currentPoint += patrolDirection;
    }
}