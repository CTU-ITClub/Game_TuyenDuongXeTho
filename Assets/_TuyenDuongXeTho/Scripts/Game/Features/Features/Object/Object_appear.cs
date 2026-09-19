using UnityEngine;

public class Object_appear : MonoBehaviour
{
    public GameObject obj;
    bool isActive = false;

    void OnTriggerEnter(Collider other)
    {
        if (isActive) return;

        if (other.CompareTag("Player"))
        {
            obj.SetActive(true);
            isActive = true;
        }
    }
}
