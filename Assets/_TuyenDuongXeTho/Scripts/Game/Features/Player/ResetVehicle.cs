using UnityEngine;
using Game.Features.Vehicle;
using Game.Features.Player;

public class ResetVehicle : MonoBehaviour
{
    public GameObject notice;
    public LayerMask layer;
    public Transform pointResetBike;
    public float distance;
    private VehicleController _vehicle;
    public PlayerController _player;
    public bool canReset = false;
    public bool nearVehicle = false;

    void Update()
    {
        ActiveFeature();
        PutNotice();

        if (pointResetBike == null || _vehicle == null) return;
        
        if (Input.GetKeyDown(KeyCode.P) && canReset && nearVehicle)
        {
            _vehicle.ResetVehicleRPC(pointResetBike);
            canReset = false;
            nearVehicle = false;
            notice.SetActive(false);
        }
    }

    private void PutNotice()
    {
        if (!canReset) return;

        // Bắn Raycast đến khi gặp Layer
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distance, layer))
        {
            notice.SetActive(true);
            notice.transform.position = hit.point + new Vector3(0, 0.05f, 0);
            // Xoay để nó nằm trên layer
            notice.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(90, 0, 0);

            pointResetBike.position = hit.point + new Vector3(0, 5f, 0);
        }
        else
        {
            notice.SetActive(false);
        }
    }

    private void CheckVehihcle()
    {
        Collider[] collis = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("XeTho"));
        foreach (Collider collider in collis)
        {
            VehicleController vehicle = collider.GetComponent<VehicleController>();
            if (vehicle != null)
            {
                _vehicle = vehicle;
                nearVehicle = true;
                return;
            }
        }
        nearVehicle = false;
    }

    private void NoticeToPlayer(string message)
    {
        if (_player != null)
        {
            _player.ChangeNotice1(message);
        }
    }

    private void ActiveFeature()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CheckVehihcle();
            if (!nearVehicle) return;
            else
            {
                canReset = true;
            }
        }
    }
}
