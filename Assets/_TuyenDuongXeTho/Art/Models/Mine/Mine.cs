using UnityEngine;
using Game.Features.Player;

public class Mine : MonoBehaviour
{
    [Header("Settings")]
    public float explosionRadius = 5f;
    public LayerMask mineLayer;
    public ParticleSystem particle;
    public GameObject visualModel; 

    [Header("State")]
    public bool isExploded = false;
    public float checkRadius = 5f;

    public void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        CheckPlayer();

        Debug.Log(gameObject.name + " đã nổ!");

        // Tách particle khỏi mine để độc lập
        if (particle != null)
        {
            particle.transform.SetParent(null); 
            particle.Play();
        }

        // Tìm tất cả các mine khác trong phạm vi và kích hoạt chúng => hiệu ứng dây chuyền
        // Chỉ tìm các collider có layer giá trị mineLayer
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

    private void CheckPlayer()
    {
        //Check Tag player
        Collider[] collider = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("Player"));

        foreach (Collider cl in collider)
        {
            PlayerController player = cl.GetComponent<PlayerController>();
            player.StartRagdollWithBomb(transform.position, 15f, 5f);
        }
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}