using UnityEngine;

public class BombWarning : MonoBehaviour
{
    [Header("Settings")]
    public GameObject warningPrefab;
    public GameObject bombModel;
    public LayerMask groundLayer;
    public float maxDistance = 50f;

    private GameObject currentWarning;
    private bool hasExploded = false;

    [Header("VFX")]
    public ParticleSystem explosionEffect;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ShowWarning();

        Destroy(gameObject, 20f);
    }

    void Update()
    {
        if (!hasExploded)
        {
            UpdateWarningPosition();
        }
    }

    void ShowWarning()
    {
        if (warningPrefab != null && currentWarning == null)
        {
            currentWarning = Instantiate(warningPrefab);
        }
    }

    void UpdateWarningPosition()
    {
        if (currentWarning == null) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance, groundLayer))
        {
            currentWarning.SetActive(true);

            currentWarning.transform.position = hit.point + new Vector3(0, 0.05f, 0);

            currentWarning.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(90, 0, 0);
        }
        else
        {
            currentWarning.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.collider.CompareTag("Ground"))
        {
            Explode();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;
        if (other.CompareTag("Water"))
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        if (explosionEffect != null)
        {
            explosionEffect.transform.SetParent(null);

            if (currentWarning != null && currentWarning.activeSelf)
            {
                explosionEffect.transform.position = currentWarning.transform.position;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance, groundLayer))
                {
                    explosionEffect.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }
            }
            else
            {
                explosionEffect.transform.position = transform.position;
            }

            explosionEffect.Play();
        }

        if (currentWarning != null)
        {
            Destroy(currentWarning);
        }

        if (bombModel != null) bombModel.SetActive(false);
        rb.isKinematic = true;

        Destroy(gameObject, 2f);
    }

    private void OnDestroy()
    {
        if (currentWarning != null)
        {
            Destroy(currentWarning);
        }
    }
}