using UnityEngine;

public class Mine : MonoBehaviour
{
    [Header("Settings")]
    public float explosionRadius = 5f;
    public LayerMask mineLayer;
    public ParticleSystem particle;
    public GameObject visualModel; 

    [Header("State")]
    public bool isExploded = false;

    public void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        Debug.Log(gameObject.name + " đã nổ!");

        if (particle != null)
        {
            particle.transform.SetParent(null); 
            particle.Play();
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, mineLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            Mine otherMine = hitCollider.GetComponent<Mine>();

            if (otherMine != null && !otherMine.isExploded)
            {
                otherMine.Explode();
            }
        }

        if (visualModel != null)
        {
            visualModel.SetActive(false);
        }

        Destroy(gameObject, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Explode();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}