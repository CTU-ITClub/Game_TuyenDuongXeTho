using UnityEngine;

public class Plane_fan : MonoBehaviour
{
    public float speedRotate = 100f;
    public float direction = 1f;
    public Transform fan;

    void Update()
    {
        //Xoay trục Y 
        fan.Rotate(Vector3.up * speedRotate * direction * Time.deltaTime);
    }
}
