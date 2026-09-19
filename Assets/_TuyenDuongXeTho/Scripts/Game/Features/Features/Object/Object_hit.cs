using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Object_hit : MonoBehaviourPun
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Item Drop")]
    public GameObject itemDropPrefab;

    [Header("VFX")]
    public ParticleSystem hitEffect;
    public ParticleSystem dieEffect;

    [Header("Hit Animation")]
    public float shrinkScale = 0.85f;
    public float popScale = 1.1f;

    public float shrinkDuration = 0.08f;
    public float popDuration = 0.12f;
    public float returnDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine hitAnimation;

    private bool canTakeDamage = true;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        // Gửi yêu cầu gây damage cho MasterClient
        photonView.RPC(
            nameof(RPC_RequestDamage),
            RpcTarget.MasterClient,
            damage
        );
    }

    [PunRPC]
    private void RPC_RequestDamage(float damage)
    {
        // Chỉ Master được xử lý máu
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!canTakeDamage || isDead)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        Debug.Log(
            $"{gameObject.name} HP: {currentHealth}/{maxHealth}"
        );

        // Sync trạng thái hit cho tất cả client
        photonView.RPC(
            nameof(RPC_Hit),
            RpcTarget.All,
            currentHealth
        );

        if (currentHealth <= 0f)
        {
            canTakeDamage = false;
            isDead = true;

            photonView.RPC(
                nameof(RPC_Die),
                RpcTarget.All
            );

            DropItem();
        }
    }

    [PunRPC]
    private void RPC_Hit(float syncedHealth)
    {
        currentHealth = syncedHealth;

        // Restart animation mỗi lần bị hit
        if (hitAnimation != null)
        {
            StopCoroutine(hitAnimation);
            hitAnimation = null;

            StopEffect(hitEffect);
        }

        PlayEffect(hitEffect);

        hitAnimation = StartCoroutine(HitAnimation());
    }

    [PunRPC]
    private void RPC_Die()
    {
        isDead = true;
        canTakeDamage = false;
        currentHealth = 0f;

        if (hitAnimation != null)
        {
            StopCoroutine(hitAnimation);
            hitAnimation = null;
        }

        // Spawn death effect LOCAL trên từng client
        if (dieEffect != null)
        {
            Vector3 effectPosition =
                transform.position +
                new Vector3(0f, 0.5f, 0f);

            GameObject dieEff = Instantiate(
                dieEffect.gameObject,
                effectPosition,
                Quaternion.identity
            );

            dieEff.SetActive(true);

            ParticleSystem ps =
                dieEff.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                ps.Play();

                Destroy(
                    dieEff,
                    ps.main.duration + ps.main.startLifetime.constantMax
                );
            }
            else
            {
                Destroy(dieEff, 3f);
            }
        }

        StartCoroutine(DieAnimation());
    }

    private void DropItem()
    {
        // Hàm này chỉ được gọi từ Master
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (itemDropPrefab == null)
            return;

        Vector3 spawnPosition =
            transform.position + Vector3.up * 2f;

        // itemDropPrefab phải nằm trong Resources và có PhotonView
        PhotonNetwork.InstantiateRoomObject(
            itemDropPrefab.name,
            spawnPosition,
            Quaternion.identity
        );
    }

    void PlayEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Play();
        }
    }

    void StopEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    IEnumerator HitAnimation()
    {
        // 1. Co lại
        yield return ScaleTo(
            originalScale * shrinkScale,
            shrinkDuration
        );

        // 2. Nở lên
        yield return ScaleTo(
            originalScale * popScale,
            popDuration
        );

        // 3. Trở về bình thường
        yield return ScaleTo(
            originalScale,
            returnDuration
        );

        hitAnimation = null;
    }

    IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration
    )
    {
        Vector3 startScale = transform.localScale;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                t
            );

            yield return null;
        }

        transform.localScale = targetScale;
    }

    IEnumerator DieAnimation()
    {
        yield return ScaleTo(
            originalScale * 0.8f,
            0.08f
        );

        yield return ScaleTo(
            originalScale * 1.15f,
            0.1f
        );

        yield return ScaleTo(
            Vector3.zero,
            0.15f
        );

        Destroy(gameObject);
    }
}