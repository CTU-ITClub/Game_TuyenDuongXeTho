using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public Plane_Boom plane;

    bool isReached = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("XeTho") && !isReached)
        {
            plane.ReachedEndPoint();
            isReached = true;
        }
    }
}
