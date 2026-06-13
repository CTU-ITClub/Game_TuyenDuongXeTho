using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public GameObject UI;

    public void Success()
    {
        UI.SetActive(true);
    }
}
