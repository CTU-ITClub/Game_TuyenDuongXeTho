using UnityEngine;
using Game.Features.Player;
using VGDSystem.Animation;

public class InteractWater : MonoBehaviour
{
    public GameObject bombPrefab;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pl = other.GetComponent<PlayerController>();
            if (pl != null)
            {
                pl.CanMove = false; 
                pl._pcInputHandler.canPlaySound = false;
            }

            AnimatorHandler animate = pl.GetComponent<AnimatorHandler>();
            if (animate != null)
            {
                animate.PlayAnimation(Animator.StringToHash("PlayerIdle"));
            }

            // Instantiate the bomb prefab at the player's position
            Vector3 spawnPosition = other.transform.position;
            spawnPosition.y += 10f;
            Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
