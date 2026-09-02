using UnityEngine;

public class Bomb_Damage : MonoBehaviour
{
    public int damage = 25;
    public float lifeTime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Bike_Camouflage bike =
            collision.gameObject.GetComponentInParent<Bike_Camouflage>();

        if (bike != null)
        {
            bike.TakeDamage(damage);

            Debug.Log(
                "Bom trúng xe! Damage = " + damage
            );
        }

        Destroy(gameObject);
    }
}