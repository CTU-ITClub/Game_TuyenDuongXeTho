using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public GameObject effect;

    public void Success()
    {
        Debug.Log("Success!");
        // Add your success logic here (e.g., load next level, show success UI, etc.)

        // T?t hi?u ?ng
        if (effect != null)
        {
            effect.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Success();
        }
    }
}
