using UnityEngine;

public class PlayerSetUp : MonoBehaviour
{
    public PlayerMove move;
    public GameObject cam;

    public void IsLocalPlayer()
    {
        move.enabled = true;
        cam.SetActive(true);
    }
}
