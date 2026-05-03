using UnityEngine;
using System.Collections;

public class Plane_Boom : MonoBehaviour
{
    [Header("Settings")]
    public GameObject boomPrefab;
    public float radius = 10f;
    public int boomCount = 5;
    public int throwTimeDelay = 3;
    public int timeLive = 15;

    [Header("Movement")]
    public Transform[] movePoints;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private int currentPointIndex = 0;

    void Start()
    {
        StartCoroutine(WaitToThrow());
        Destroy(gameObject, timeLive);
    }

    void Update()
    {
        Move();
    }

    IEnumerator WaitToThrow()
    {
        while (true)
        {
            yield return new WaitForSeconds(throwTimeDelay);
            ThrowBoom();
        }
    }

    public void ThrowBoom()
    {
        for (int i = 0; i < boomCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            Instantiate(boomPrefab, spawnPos, Quaternion.identity);
        }
    }

    void Move()
    {
        if (movePoints == null || movePoints.Length == 0) return;

        Transform targetPoint = movePoints[currentPointIndex];
        Vector3 direction = targetPoint.position - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPointIndex = (currentPointIndex + 1) % movePoints.Length;
        }
    }

    void OnDrawGizmos()
    {
        if (movePoints != null && movePoints.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < movePoints.Length; i++)
            {
                Vector3 current = movePoints[i].position;
                Vector3 next = movePoints[(i + 1) % movePoints.Length].position;
                Gizmos.DrawLine(current, next);
                Gizmos.DrawSphere(current, 0.3f);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}