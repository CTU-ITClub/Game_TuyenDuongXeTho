using UnityEngine;
using System.Collections;

public class Pickaxe : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 10f;

    [Header("Attack")]
    public float cooldown = 1f;
    public float hitDuration = 0.2f;

    public bool canAttack = true;
    public bool canDealDamage = false;
    public KeyCode useKey = KeyCode.U;

    void Update()
    {
        if (Input.GetKeyDown(useKey) && canAttack)
        {
            Attack();
        }
    }

    void Attack()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;
        canDealDamage = true;

        yield return new WaitForSeconds(hitDuration);

        canDealDamage = false;

        yield return new WaitForSeconds(cooldown);

        canAttack = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage)
            return;

        if (other.CompareTag("Rock"))
        {
            Object_hit rockHit =
                other.GetComponentInParent<Object_hit>();

            if (rockHit != null)
            {
                rockHit.TakeDamage(damage);

                canDealDamage = false;
            }
        }
    }
}