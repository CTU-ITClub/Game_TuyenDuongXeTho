using UnityEngine;
using System.Collections;

public class Plane_Boom : MonoBehaviour
{
    [Header("Settings")]
    public GameObject boomPrefab;
    public float radius = 10f;
    public int boomCount = 5;
    public float throwTimeDelay = 3f;

    [Header("Movement")]
    public GameObject plane; // Đã sửa lỗi chính tả GameOject

    public Transform[] movePoints;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private int currentPointIndex = 0;
    private bool isDead = false;
    private bool isMove = false;

    void Start()
    {
        StartCoroutine(WaitToThrow());
    }

    void Update()
    {
        if (isMove && !isDead && plane != null)
        {
            MovePlane();
        }
    }

    IEnumerator WaitToThrow()
    {
        while (true)
        {
            yield return new WaitForSeconds(throwTimeDelay);
            if (plane != null)
            {
                ThrowBoom();
            }
        }
    }

    public void ThrowBoom()
    {
        if (!isMove || isDead) return;

        for (int i = 0; i < boomCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 spawnPos = plane.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            Instantiate(boomPrefab, spawnPos, Quaternion.identity);
        }
    }

    void MovePlane()
    {
        if (movePoints == null || movePoints.Length == 0) return;

        Transform targetPoint = movePoints[currentPointIndex];
        Vector3 direction = targetPoint.position - plane.transform.position;

        plane.transform.position = Vector3.MoveTowards(plane.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            plane.transform.rotation = Quaternion.Slerp(plane.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(plane.transform.position, targetPoint.position) < 0.2f)
        {
            if (currentPointIndex >= movePoints.Length - 1)
            {
                ReachedEndPoint();
            }
            else
            {
                currentPointIndex++;
            }
        }
    }

    void ReachedEndPoint()
    {
        isDead = true;
        Destroy(plane);
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (movePoints != null && movePoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < movePoints.Length; i++)
            {
                Vector3 current = movePoints[i].position;
                Gizmos.DrawSphere(current, 0.3f);

                if (i < movePoints.Length - 1)
                {
                    Vector3 next = movePoints[i + 1].position;
                    Gizmos.DrawLine(current, next);
                }
            }
        }

        if (plane != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(plane.transform.position, radius);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead || isMove) return;

        if (other.CompareTag("Player"))
        {
            isMove = true;
        }
    }
}