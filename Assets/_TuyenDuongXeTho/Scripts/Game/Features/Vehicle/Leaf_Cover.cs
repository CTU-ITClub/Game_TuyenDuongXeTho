using UnityEngine;

public class Leaf_Cover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bike_Camouflage bikeCamouflage;

    [Header("Danh sách lá")]
    [SerializeField] private GameObject[] leaves;

    [Header("Leaf Settings")]
    [SerializeField] private float camouflageAddAmount = 10f;

    private int currentLeafCount;

    private void Awake()
    {
        if (bikeCamouflage == null)
        {
            bikeCamouflage = GetComponent<Bike_Camouflage>();
        }
    }

    private void Start()
    {
        if (bikeCamouflage == null) return;

        RefreshLeaves(bikeCamouflage.GetCamouflageIndex(), bikeCamouflage.GetCamouflageIndexMax());
    }

    public void AddLeaf()
    {
        if (bikeCamouflage == null) return;

        bikeCamouflage.UpdateCamouflageIndex(camouflageAddAmount);
    }

    public void RemoveLeaf()
    {
        if (bikeCamouflage == null) return;

        bikeCamouflage.UpdateCamouflageIndex(-camouflageAddAmount);
    }

    public void ChangeLeafStatus(bool status)
    {
        if (status)
        {
            AddLeaf();
        }
        else
        {
            RemoveLeaf();
        }
    }

    public void RefreshLeaves(float currentCamouflage, float maximumCamouflage)
    {
        if (leaves == null || leaves.Length == 0)
            return;

        if (maximumCamouflage <= 0f)
            return;

        float normalizedValue = Mathf.Clamp01(currentCamouflage / maximumCamouflage);

        currentLeafCount = Mathf.CeilToInt(normalizedValue * leaves.Length);

        for (int i = 0; i < leaves.Length; i++)
        {
            if (leaves[i] != null)
            {
                leaves[i].SetActive(i < currentLeafCount);
            }
        }
    }
}