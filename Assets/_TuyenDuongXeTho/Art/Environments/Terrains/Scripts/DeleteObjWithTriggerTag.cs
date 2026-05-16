using UnityEngine;

public class DeleteObjWithTriggerTag : MonoBehaviour
{
    public string tag;
    public float timeToDestroy = 0f;
    private bool isDestroyed = false;

    public GameObject plane;

    private void OnTriggerStay(Collider other)
    {
        if (isDestroyed) return;

        if (other.CompareTag(tag))
        {
            Debug.Log("Destroying" );

            isDestroyed = true;
            if (plane != null)
                plane.SetActive(false);
            Destroy(this.gameObject, timeToDestroy);
        }
    }
}
