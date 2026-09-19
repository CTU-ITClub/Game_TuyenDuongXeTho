using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Plane_Patrol : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform targetBike;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 4f;

    [SerializeField] private float stopDistance = 2f;

    [Header("Height")]
    [SerializeField] private float fixedHeight = 10f;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (targetBike == null) FindTarget();

        if (!photonView.IsMine) return;

        FollowTarget();
    }

    private void FollowTarget()
    {
        if (targetBike == null) return;

        Vector3 desiredPosition = targetBike.position + Vector3.up * fixedHeight;

        // Chỉ tính hướng di chuyển theo mặt phẳng XZ.
        Vector3 horizontalDirection = desiredPosition - transform.position;

        horizontalDirection.y = 0f;

        float horizontalDistance = horizontalDirection.magnitude;

        Vector3 nextPosition = transform.position;

        if (horizontalDistance > stopDistance)
        {
            horizontalDirection.Normalize();

            nextPosition += horizontalDirection * speed * Time.deltaTime;

            // Chỉ xoay ngang, tránh máy bay chúi lên hoặc chúi xuống.
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Luôn giữ máy bay cao hơn xe đúng fixedHeight.
        nextPosition.y = targetBike.position.y + fixedHeight;

        transform.position = nextPosition;
    }

    private void FindTarget()
    {
        GameObject bike = GameObject.FindGameObjectWithTag("XeTho");

        if (bike != null) targetBike = bike.transform;
    }
}