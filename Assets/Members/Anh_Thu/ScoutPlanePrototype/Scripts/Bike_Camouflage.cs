using UnityEngine;

// Script quản lý mức ngụy trang
// và độ bền của xe thồ.
public class Bike_Camouflage : MonoBehaviour
{
    [Header("Ngụy trang")]

    [Range(0, 100)]
    public int camouflage = 100;

    [Range(0, 100)]
    public int detectionThreshold = 50;

    public int decayAmount = 1;
    public float decayInterval = 1f;


    [Header("Độ bền xe")]

    public int maxDurability = 100;
    public int currentDurability = 100;


    private float decayTimer = 0f;


    // Xe có thể bị máy bay phát hiện
    // khi mức ngụy trang thấp hơn ngưỡng phát hiện.
    public bool CanBeDetected
    {
        get
        {
            return camouflage < detectionThreshold;
        }
    }


    private void Update()
    {
        // Cộng thời gian sau mỗi frame.
        decayTimer += Time.deltaTime;

        // Khi đủ thời gian thì giảm ngụy trang.
        if (decayTimer >= decayInterval)
        {
            ReduceCamouflage();
            decayTimer = 0f;
        }
    }


    // Giảm mức ngụy trang theo thời gian.
    private void ReduceCamouflage()
    {
        if (camouflage <= 0)
        {
            return;
        }

        camouflage -= decayAmount;

        camouflage = Mathf.Clamp(
            camouflage,
            0,
            100
        );
    }


    // Dùng khi người chơi đắp thêm lá/ngụy trang.
    public void AddCamouflage(int amount)
    {
        camouflage += amount;

        camouflage = Mathf.Clamp(
            camouflage,
            0,
            100
        );
    }


    // Bom gọi hàm này khi gây sát thương cho xe.
    public void TakeDamage(int damage)
    {
        currentDurability -= damage;

        currentDurability = Mathf.Clamp(
            currentDurability,
            0,
            maxDurability
        );

        Debug.Log(
            "Xe bị trúng bom. Độ bền còn: " +
            currentDurability
        );
    }
}