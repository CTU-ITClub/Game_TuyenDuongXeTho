using UnityEngine;
using System.Collections;

public class BezierThrow : MonoBehaviour
{
    [Header("Objects")]
    public GameObject bombPrefab;
    public Transform startPoint;
    public Transform endPoint;

    [Header("Bezier Settings")]
    public float curveHeight = 10f;
    public float duration = 2f;

    private bool isThrown = false;

    public Animator animator;

    void Update()
    {
        // Phím Space để test ném
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Throw();
        }
    }

    public void Throw()
    {
        if (isThrown) return; 
        if (bombPrefab == null || startPoint == null || endPoint == null) return;

        StartCoroutine(WaitThrowAnimate());
    }

    IEnumerator WaitThrowAnimate()
    {
        animator.enabled = true;

        yield return new WaitForSeconds(3.5f);

        Vector3 midPoint = (startPoint.position + endPoint.position) / 2f;
        Vector3 controlPoint = midPoint + Vector3.up * curveHeight;

        GameObject thrownBomb = Instantiate(bombPrefab, startPoint.position, Quaternion.identity);
        StartCoroutine(AnimateBezier(thrownBomb, startPoint.position, controlPoint, endPoint.position));
    }
    IEnumerator AnimateBezier(GameObject bomb, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Tính toán giá trị t (từ 0.0 đến 1.0)
            float t = elapsedTime / duration;

            // --- CÔNG THỨC BEZIER  ---
            // B(t) = (1-t)^2 * P0 + 2 * (1-t) * t * P1 + t^2 * P2
            Vector3 position = Mathf.Pow(1 - t, 2) * p0 +
                              2 * (1 - t) * t * p1 +
                              Mathf.Pow(t, 2) * p2;

            bomb.transform.position = position;

            Vector3 lookDirection = (p2 - position).normalized;
            if (lookDirection != Vector3.zero)
            {
                bomb.transform.forward = lookDirection;
            }

            yield return null; 
        }

        bomb.transform.position = p2;
    }

    void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Vector3 midPoint = (startPoint.position + endPoint.position) / 2f;
            Vector3 controlPoint = midPoint + Vector3.up * curveHeight;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPoint.position, controlPoint);
            Gizmos.DrawLine(controlPoint, endPoint.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Throw();
            isThrown = true;
        }
    }
}