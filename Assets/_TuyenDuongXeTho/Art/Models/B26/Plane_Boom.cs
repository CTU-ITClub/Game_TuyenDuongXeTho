using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class Plane_Boom : MonoBehaviourPun
{
    [Header("Boom Settings")]
    [SerializeField] private GameObject boomPrefab;
    [SerializeField] private int boomCount = 5;
    [SerializeField] private float throwTimeDelay = 3f;

    [Header("Bomb Area Points")]
    [SerializeField] private List<Transform> areaPoints = new();
    public int distanceEachBomb = 5;

    [Header("Plane Movement")]
    [SerializeField] private GameObject plane;
    [SerializeField] private List<Transform> movePoints = new();
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float reachDistance = 0.2f;

    private PhotonView pv;

    private bool isDead = false;
    private bool isMove = false;

    private Coroutine throwCoroutine;

    private int currentMoveIndex = 0;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        if (pv == null)
        {
            pv = gameObject.AddComponent<PhotonView>();
        }
    }

    private void Start()
    {
        if (plane != null)
        {
            plane.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (!isMove) return;
        if (plane == null) return;

        MovePlane();
    }

    #region Plane Movement

    private void MovePlane()
    {
        if (movePoints == null || movePoints.Count == 0)
            return;

        Transform targetPoint = movePoints[currentMoveIndex];

        if (targetPoint == null)
            return;

        Vector3 targetPos = targetPoint.position;

        // Move
        plane.transform.position = Vector3.MoveTowards(
            plane.transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // Rotate
        Vector3 direction = targetPos - plane.transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            plane.transform.rotation = Quaternion.Slerp(
                plane.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Reached point
        float distance = Vector3.Distance(
            plane.transform.position,
            targetPos
        );

        if (distance <= reachDistance)
        {
            currentMoveIndex++;

            // Loop lại từ đầu
            if (currentMoveIndex >= movePoints.Count)
            {
                currentMoveIndex = 0;
            }
        }
    }

    #endregion

    #region Bomb Spawn

    private IEnumerator WaitToThrow()
    {
        while (true)
        {
            yield return new WaitForSeconds(throwTimeDelay);

            if (plane == null) continue;
            if (!isMove) continue;
            if (isDead) continue;

            CalculateBoomPositions();
        }
    }

    private void CalculateBoomPositions()
    {
        List<Vector3> validPoints = new();

        for (int i = 0; i < boomCount; i++)
        {
            Vector3 ranPos = GetRandomPointInPolygon();
            bool validPoint = false;

            for (int j = 0; j < 100; j++)
            {
                bool tooClose = false;

                foreach (Vector3 point in validPoints)
                {
                    if (Vector3.Distance(ranPos, point) < distanceEachBomb)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    validPoint = true;
                    break;
                }
            }

            if (validPoint)
            {
                validPoints.Add(ranPos);
                pv.RPC(nameof(RPC_SpawnBoom), RpcTarget.All, ranPos);
            }
        }
    }

    private Vector3 GetRandomPointInPolygon()
    {
        Bounds bounds = GetPolygonBounds();

        for (int i = 0; i < 50; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                plane.transform.position.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (IsPointInsidePolygon(randomPoint))
            {
                return randomPoint;
            }
        }

        return areaPoints[0].position;
    }

    private Bounds GetPolygonBounds()
    {
        Bounds bounds = new Bounds(areaPoints[0].position, Vector3.zero);

        foreach (Transform point in areaPoints)
        {
            bounds.Encapsulate(point.position);
        }

        return bounds;
    }

    private bool IsPointInsidePolygon(Vector3 point)
    {
        bool inside = false;

        for (int i = 0, j = areaPoints.Count - 1; i < areaPoints.Count; j = i++)
        {
            Vector3 pi = areaPoints[i].position;
            Vector3 pj = areaPoints[j].position;

            bool intersect =
                ((pi.z > point.z) != (pj.z > point.z)) &&
                (point.x <
                 (pj.x - pi.x) * (point.z - pi.z) /
                 (pj.z - pi.z) + pi.x);

            if (intersect)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    [PunRPC]
    private void RPC_SpawnBoom(Vector3 spawnPos)
    {
        if (boomPrefab == null) return;

        Instantiate(
            boomPrefab,
            spawnPos,
            Quaternion.identity
        );
    }

    #endregion

    #region Trigger

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (isMove) return;

        if (!other.CompareTag("Player")) return;

        pv.RPC(nameof(RPC_StartPlaneMove), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartPlaneMove()
    {
        if (plane != null)
        {
            plane.SetActive(true);
        }

        isMove = true;

        currentMoveIndex = 0;

        if (PhotonNetwork.IsMasterClient)
        {
            if (throwCoroutine != null)
            {
                StopCoroutine(throwCoroutine);
            }

            throwCoroutine = StartCoroutine(WaitToThrow());
        }
    }

    #endregion

    public void ReachedEndPoint()
    {
        if (isDead) return;

        isDead = true;

        if (throwCoroutine != null)
        {
            StopCoroutine(throwCoroutine);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    #region Gizmos

    private void OnDrawGizmos()
    {
        // Bomb Area
        if (areaPoints != null && areaPoints.Count >= 2)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < areaPoints.Count; i++)
            {
                Transform current = areaPoints[i];
                Transform next = areaPoints[(i + 1) % areaPoints.Count];

                if (current != null && next != null)
                {
                    Gizmos.DrawLine(current.position, next.position);
                }
            }
        }

        // Move Path
        if (movePoints != null && movePoints.Count >= 2)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < movePoints.Count; i++)
            {
                Transform current = movePoints[i];
                Transform next = movePoints[(i + 1) % movePoints.Count];

                if (current != null && next != null)
                {
                    Gizmos.DrawLine(current.position, next.position);
                }
            }
        }
    }

    #endregion
}