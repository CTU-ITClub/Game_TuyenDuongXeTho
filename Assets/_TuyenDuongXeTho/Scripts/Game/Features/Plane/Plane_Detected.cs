using UnityEngine;
using Photon.Pun;

public class Plane_Detector : MonoBehaviour
{
    [Header("Đối tượng cần gán")]
    [SerializeField] private Light scanLight;
    public Bike_Camouflage targetBike;

    [Header("Target Settings")]

    [Tooltip("Điểm kiểm tra cao hơn pivot của xe.")]
    [SerializeField] private float targetHeightOffset = 0.5f;

    [Header("Kiểm tra vật cản")]
    [SerializeField] private bool checkObstacles = false;
    [SerializeField] private LayerMask obstacleLayer;

    private bool wasDetected;

    private void Start()
    {
        ValidateReferences();
    }

    private void Update()
    {
        // Khi chưa vào Photon Room thì vẫn cho phép test.
        if (PhotonNetwork.InRoom &&
            !PhotonNetwork.IsMasterClient)
        {
            wasDetected = false;
            return;
        }

        if (targetBike == null)
        {
            FindTargetBike();
        }

        CheckBikeDetection();
    }

    private void CheckBikeDetection()
    {
        if (scanLight == null ||
            targetBike == null)
        {
            SetDetectionState(false);
            return;
        }

        if (!scanLight.enabled || !scanLight.gameObject.activeInHierarchy)
        {
            SetDetectionState(false);
            return;
        }

        if (scanLight.type != LightType.Spot)
        {
            SetDetectionState(false);
            return;
        }

        Vector3 lightPosition = scanLight.transform.position;

        Vector3 targetPosition = targetBike.transform.position + Vector3.up * targetHeightOffset;

        Vector3 directionToBike = targetPosition - lightPosition;

        float distanceToBike = directionToBike.magnitude;

        if (distanceToBike <= 0.001f)
        {
            SetDetectionState(false);
            return;
        }

        Vector3 normalizedDirection = directionToBike.normalized;

        bool isInsideRange = distanceToBike <= scanLight.range;

        float angleToBike = Vector3.Angle(
            scanLight.transform.forward,
            normalizedDirection
        );

        float halfSpotAngle =
            scanLight.spotAngle * 0.5f;

        bool isInsideCone =
            angleToBike <= halfSpotAngle;

        bool camouflageAllowsDetection =
            targetBike.CanBeDetected();

        bool hasLineOfSight = true;

        if (checkObstacles &&
            isInsideRange &&
            isInsideCone)
        {
            bool isBlocked = Physics.Raycast(
                lightPosition,
                normalizedDirection,
                distanceToBike,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );

            hasLineOfSight = !isBlocked;
        }

        bool isDetected =
            isInsideRange &&
            isInsideCone &&
            camouflageAllowsDetection &&
            hasLineOfSight;

        SetDetectionState(isDetected);
    }

    private void SetDetectionState(bool isDetected)
    {
        if (isDetected && !wasDetected)
        {
            Debug.Log(
                "Máy bay đã phát hiện xe thồ!"
            );
        }
        else if (!isDetected && wasDetected)
        {
            Debug.Log(
                "Máy bay đã mất dấu xe thồ."
            );
        }

        wasDetected = isDetected;
    }

    public bool IsBikeDetected()
    {
        return wasDetected;
    }

    private void FindTargetBike()
    {
        GameObject bike =
            GameObject.FindGameObjectWithTag("XeTho_leaf");

        if (bike == null)
            return;

        targetBike =
            bike.GetComponent<Bike_Camouflage>();

        if (targetBike == null)
        {
            Debug.LogError(
                "Object XeTho chưa có Bike_Camouflage.",
                bike
            );
        }
    }

    private void ValidateReferences()
    {
        if (scanLight == null)
        {
            scanLight =
                GetComponentInChildren<Light>();
        }

        if (scanLight == null)
        {
            Debug.LogError(
                "Plane_Detector: Chưa gán Scan Light.",
                this
            );
        }
        else if (scanLight.type != LightType.Spot)
        {
            Debug.LogError(
                "Plane_Detector: Scan Light phải là Spot Light.",
                scanLight
            );
        }

        if (targetBike == null)
        {
            FindTargetBike();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (scanLight == null ||
            scanLight.type != LightType.Spot)
        {
            return;
        }

        Gizmos.color = Color.red;

        Transform lightTransform =
            scanLight.transform;

        Vector3 origin =
            lightTransform.position;

        Vector3 forward =
            lightTransform.forward;

        Vector3 right =
            lightTransform.right;

        Vector3 up =
            lightTransform.up;

        float range =
            scanLight.range;

        float halfAngle =
            scanLight.spotAngle
            * 0.5f
            * Mathf.Deg2Rad;

        float forwardDistance =
            Mathf.Cos(halfAngle) * range;

        float radius =
            Mathf.Sin(halfAngle) * range;

        Vector3 circleCenter =
            origin + forward * forwardDistance;

        const int segments = 32;

        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle =
                i / (float)segments
                * Mathf.PI
                * 2f;

            Vector3 radialDirection =
                right * Mathf.Cos(angle)
                + up * Mathf.Sin(angle);

            Vector3 currentPoint =
                circleCenter
                + radialDirection * radius;

            if (i > 0)
            {
                Gizmos.DrawLine(
                    previousPoint,
                    currentPoint
                );
            }

            // Vẽ một số đường từ đỉnh tới đáy nón.
            if (i % 8 == 0)
            {
                Gizmos.DrawLine(
                    origin,
                    currentPoint
                );
            }

            previousPoint = currentPoint;
        }
    }
}