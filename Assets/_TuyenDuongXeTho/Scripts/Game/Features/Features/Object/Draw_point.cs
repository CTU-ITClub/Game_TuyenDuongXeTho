using UnityEngine;

public class Draw_point : MonoBehaviour
{
    public Transform[] points;

    [Header("Gizmo Settings")]
    public float cubeSize = 1f;
    public Color cubeColor = Color.red;

    private void OnDrawGizmos()
    {
        if (points == null || points.Length == 0)
            return;

        Gizmos.color = cubeColor;

        foreach (Transform point in points)
        {
            if (!point)
                continue;

            Gizmos.DrawCube(
                point.position,
                Vector3.one * cubeSize
            );
        }
    }
}