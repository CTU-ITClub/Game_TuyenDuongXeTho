using UnityEngine;

public class Zone_reduce_vehi_dura : MonoBehaviour
{
    bool isPlayerInZone = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("XeTho") && !isPlayerInZone)
        {
            isPlayerInZone = true;

            Vehicle_durability vehicleDurability = other.GetComponent<Vehicle_durability>();
            if (vehicleDurability != null)
            {
                vehicleDurability.UpdateDurability(-90f); // Giảm 10 điểm độ bền khi vào vùng
            }
        }
    }
}
