using UnityEngine;

public class Scan_Sweep : MonoBehaviour
{
    [Header("Scan Settings")]

    // Góc đèn lia về trước và sau.
    // Ví dụ 25 nghĩa là:
    // -25 độ <-> +25 độ.
    public float sweepAngle = 25f;

    // Tốc độ quét.
    public float sweepSpeed = 2f;

    // Góc ban đầu của Scan.
    private Quaternion startRotation;

    private void Start()
    {
        // Lưu rotation ban đầu.
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        // Tạo giá trị chạy liên tục:
        // -1 -> 0 -> 1 -> 0 -> -1...
        float wave = Mathf.Sin(
            Time.time * sweepSpeed
        );

        // Góc quay hiện tại.
        float currentAngle =
            wave * sweepAngle;

        // Quét tới và lui theo trục X.
        transform.localRotation =
            startRotation *
            Quaternion.Euler(
                currentAngle,
                0f,
                0f
            );
    }
}