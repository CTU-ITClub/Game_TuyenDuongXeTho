using UnityEngine;

public class Object_moving : MonoBehaviour
{
    [Header("Set Up")]
    public float speed = 5f;
    public Transform[] points;
    public string stopPointEachDistance = "";
    private int[] stopPoints;

    public bool isMoving = false;
    public int currentPointIndex = 0;
    public int currentStopPointIndex = 0;

    public void ActiveMove()
    {
        if (isMoving)
            return;

        if (currentPointIndex >= points.Length)
            return;

        isMoving = true;
    }

    void Start()
    {
        FindPointList();
        GetStopPointIndex();
    }

    void Update()
    {
        // test
        if (Input.GetKeyDown(KeyCode.M))
        {
            ActiveMove();
        }

        if (!isMoving)
            return;

        Move();
    }

    void Move()
    {
        if (currentPointIndex >= points.Length)
        {
            isMoving = false;
            return;
        }

        Transform targetPoint = points[currentPointIndex];

        if (targetPoint == null)
        {
            currentPointIndex++;
            isMoving = false;
            return;
        }
        
        Vector3 direction = targetPoint.position - transform.position;

        Vector3 rotateDirection = new Vector3(direction.x, 0f, direction.z);

        if (rotateDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rotateDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                5f * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
        {
            transform.position = targetPoint.position;
            currentPointIndex++;

            if (currentPointIndex == stopPoints[currentStopPointIndex])
            {
                if(currentStopPointIndex < stopPoints.Length - 1)
                    currentStopPointIndex++;

                // tăng speed khi đi qua điểm dừng
                speed += 20f;

                isMoving = false;
            }
        }
    }

    void GetStopPointIndex()
    {
        if (string.IsNullOrEmpty(stopPointEachDistance))
            return;
        string[] stopPointStrings = stopPointEachDistance.Split(',');
        stopPoints = new int[stopPointStrings.Length];
        for (int i = 0; i < stopPointStrings.Length; i++)
        {
            if (int.TryParse(stopPointStrings[i], out int stopPoint))
            {
                stopPoints[i] = stopPoint;
            }
            else
            {
                Debug.LogWarning($"Invalid stop point value: {stopPointStrings[i]}");
            }
        }
    }

    void FindPointList()
    {
        // tìm theo tên file trên scene
        string objectName = "Point_lists_Injured_NPC";

        GameObject pointListObject = GameObject.Find(objectName);
        if (pointListObject != null)
        {
            points = new Transform[pointListObject.transform.childCount];
            for (int i = 0; i < pointListObject.transform.childCount; i++)
            {
                points[i] = pointListObject.transform.GetChild(i);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActiveMove();
        }
    }

    void OnDrawGizmos()
    {
        if (points == null || points.Length == 0)
            return;
        Gizmos.color = Color.red;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
            {
                Gizmos.DrawSphere(points[i].position, 0.1f);
            }
        }
    }
}