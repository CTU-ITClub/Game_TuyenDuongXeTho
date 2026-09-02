using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Leaf_Cover_Prototype : MonoBehaviour
{
    [Header("Đối tượng")]
    public GameObject leaf;
    public Bike_Camouflage bikeCamouflage;

    [Header("Đắp lá")]
    
    // Mỗi lần bấm E sẽ cộng bao nhiêu điểm ngụy trang
    public int camouflagePerPress = 10;


    private void Start()
    {
        UpdateLeafVisual();
    }


    private void Update()
    {
        bool pressedE = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            pressedE = Keyboard.current.eKey.wasPressedThisFrame;
        }
#else
        pressedE = Input.GetKeyDown(KeyCode.E);
#endif

        // Bấm E để đắp thêm lá
        if (pressedE)
        {
            AddLeaf();
        }

        // Cập nhật hình ảnh lá theo camouflage
        UpdateLeafVisual();
    }


    private void AddLeaf()
    {
        if (bikeCamouflage == null)
        {
            return;
        }

        // Sử dụng hàm AddCamouflage có sẵn
        // trong Bike_Camouflage
        bikeCamouflage.AddCamouflage(
            camouflagePerPress
        );

        Debug.Log(
            "Đắp thêm lá! Camouflage hiện tại: " +
            bikeCamouflage.camouflage
        );
    }


    private void UpdateLeafVisual()
    {
        if (leaf == null || bikeCamouflage == null)
        {
            return;
        }

        // Có camouflage thì hiện khối lá.
        // Camouflage về 0 thì ẩn.
        leaf.SetActive(
            bikeCamouflage.camouflage > 0
        );
    }
}   