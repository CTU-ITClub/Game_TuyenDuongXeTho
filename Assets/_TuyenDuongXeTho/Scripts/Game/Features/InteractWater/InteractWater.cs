using UnityEngine;

public class InteractWater : MonoBehaviour
{
    public GameObject bombPrefab;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Instantiate the bomb prefab at the player's position
            Vector3 spawnPosition = other.transform.position;
            spawnPosition.y += 10f;
            Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
