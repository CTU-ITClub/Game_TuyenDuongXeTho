using Game.Features.Player;
using UnityEngine;

namespace Game.Features.Animal.Wolf
{
    public class WolfAttackHitbox : MonoBehaviour
    {
        [SerializeField] private WolfController _wolf;
        public Collider collider => GetComponent<Collider>();

        private void Awake()
        {
            if (_wolf == null)
                _wolf = GetComponentInParent<WolfController>();

            Collider hitbox = GetComponent<Collider>();

            if (hitbox != null)
                hitbox.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_wolf.IsPlayerInAttackRange() == false)
                return;

            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                player.StartRagdollWithBomb(transform.position, 15f, 5f);
            }
        }
    }
}