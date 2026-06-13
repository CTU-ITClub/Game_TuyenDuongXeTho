using UnityEngine;
using Game.Features.Player;

public class BombWarning : MonoBehaviour
{
    [Header("Settings")]
    public GameObject warningPrefab;    // prefab tâm điểm cảnh báo
    public GameObject bombModel;
    public LayerMask groundLayer;
    public float maxDistance = 50f;

    private GameObject currentWarning;
    private bool hasExploded = false;

    [Header("VFX")]
    public ParticleSystem explosionEffect;

    private Rigidbody rb;
    
    [Header("CheckPlayer")]
    public float checkRadius = 5f;

    [Header("Sound Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _exploSound, _smokeSound;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ShowWarning();
        // Phá hủy sau 20s nếu chưa nổ
        Destroy(gameObject, 20f);

        if (_audioSource != null && _smokeSound != null)
        {
            _audioSource.clip = _smokeSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }
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
        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance, groundLayer)) // Nếu tia raycast chạm vào Layer đươc chỉ định
        {
            currentWarning.SetActive(true);
            //Đặt cảnh báo tại vị trí chạm + 0.05f trục y
            currentWarning.transform.position = hit.point + new Vector3(0, 0.05f, 0);
            // Xoay để nó nằm trên layer
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

    void OnTriggerEnter(Collider other) // Do trên mặt nước không có collider nên dùng trigger để phát hiện
    {
        if (hasExploded) return;
        if (other.CompareTag("Water") || other.CompareTag("Player")) 
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        CheckPlayer();

        // Phát âm thanh khi nổ
        if (_audioSource != null && _exploSound != null)
        {
            _audioSource.clip = _exploSound;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        if (explosionEffect != null)
        {
            // Tách explosionEffect ra khỏi Bomb để nó không bị phá hủy cùng với Bomb
            explosionEffect.transform.SetParent(null);

            if (currentWarning != null && currentWarning.activeSelf)
            {
                explosionEffect.transform.position = currentWarning.transform.position;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance, groundLayer))
                {
                    // Xoay effect cùng hướng với mặt đất
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
        // Ẩn mô hình bom để chỉ còn lại hiệu ứng nổ
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

    private void CheckPlayer()
    {
        //Check Tag player
        Collider[] collider = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("Player"));

        foreach (Collider cl in collider)
        {
            PlayerController player = cl.GetComponent<PlayerController>();
            if (player != null) 
                player.StartRagdollWithBomb(transform.position, 15f, 5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}