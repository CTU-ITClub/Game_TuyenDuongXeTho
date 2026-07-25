using UnityEngine;

// Script này được gắn vào Shelter.
// Nó kiểm tra khi xe đi vào hoặc đi ra khỏi hầm.
public class ShelterZone : MonoBehaviour
{
    // Unity chạy hàm này khi một object đi vào Trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Tìm Bike_Camouflage trên object đi vào
        // hoặc trên object cha của nó.
        Bike_Camouflage bike =
            other.GetComponentInParent<Bike_Camouflage>();

        // Nếu tìm thấy, object đó chính là xe.
        if (bike != null)
        {
            // Báo cho xe biết nó đang ở trong Shelter.
            bike.SetInsideShelter(true);

            Debug.Log(
                "Xe đã vào Shelter. Is Inside Shelter = true."
            );
        }
    }

    // Unity chạy hàm này khi object rời khỏi Trigger.
    private void OnTriggerExit(Collider other)
    {
        Bike_Camouflage bike =
            other.GetComponentInParent<Bike_Camouflage>();

        if (bike != null)
        {
            // Báo cho xe biết nó đã ra khỏi Shelter.
            bike.SetInsideShelter(false);

            Debug.Log(
                "Xe đã ra khỏi Shelter. Is Inside Shelter = false."
            );
        }
    }
}