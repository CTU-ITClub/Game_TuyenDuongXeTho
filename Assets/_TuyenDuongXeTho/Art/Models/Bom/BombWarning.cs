using UnityEngine;

public class BombWarning : MonoBehaviour
{
    [Header("Settings")]
    public GameObject warningPrefab; 
    public LayerMask groundLayer;    
    public float maxDistance = 50f; 

    private GameObject currentWarning;

    void Start()
    {
        ShowWarning();
    }

    void Update()
    {
        UpdateWarningPosition();
    }

    void ShowWarning()
    {
        if (warningPrefab != null)
        {
            currentWarning = Instantiate(warningPrefab);
        }
    }

    void UpdateWarningPosition()
    {
        if (currentWarning == null) return;

        // B?n m?t tia Raycast t? v? trí qu? bom th?ng xu?ng d??i
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance, groundLayer))
        {
            currentWarning.SetActive(true);

            currentWarning.transform.position = hit.point + new Vector3(0, 0.05f, 0);

            currentWarning.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(90, 0, 0);
        }
        else
        {
            currentWarning.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (currentWarning != null)
        {
            Destroy(currentWarning);
        }
    }
}