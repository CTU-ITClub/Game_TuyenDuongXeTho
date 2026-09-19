using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MountainRoadGenerator : MonoBehaviour
{
    [Header("References")]
    public SplineContainer splineContainer;
    public Terrain terrain;

    [Header("Road Settings")]
    [Min(0.5f)]
    public float roadWidth = 5f;

    [Min(0f)]
    public float blendWidth = 4f;

    [Min(0.2f)]
    public float sampleDistance = 1f;

    [Header("Height Offset")]
    public float roadHeightOffset = 0.1f;

    [Header("Debug")]
    public bool drawPreview = true;

    private float[,] originalHeights;
    private bool hasBackup = false;

    [ContextMenu("Backup Terrain")]
    public void BackupTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain chưa được gán.");
            return;
        }

        TerrainData data = terrain.terrainData;

        int resolution = data.heightmapResolution;

        originalHeights = data.GetHeights(
            0,
            0,
            resolution,
            resolution
        );

        hasBackup = true;

        Debug.Log("Đã backup Terrain.");
    }

    [ContextMenu("Generate Road")]
    public void GenerateRoad()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain chưa được gán.");
            return;
        }

        if (splineContainer == null)
        {
            Debug.LogError("Spline Container chưa được gán.");
            return;
        }

        if (!hasBackup)
        {
            BackupTerrain();
        }

        TerrainData data = terrain.terrainData;

        int resolution = data.heightmapResolution;

        float[,] heights = data.GetHeights(
            0,
            0,
            resolution,
            resolution
        );

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = data.size;

        Spline spline = splineContainer.Spline;

        float splineLength = spline.GetLength();

        int samples = Mathf.CeilToInt(
            splineLength / sampleDistance
        );

        for (int i = 0; i <= samples; i++)
        {
            float distance = Mathf.Min(
                i * sampleDistance,
                splineLength
            );

            float t = splineLength > 0f
                ? distance / splineLength
                : 0f;

            float3 localPos =
                spline.EvaluatePosition(t);

            Vector3 worldPos =
                splineContainer.transform.TransformPoint(
                    (Vector3)localPos
                );

            ApplyRoadAtPoint(
                worldPos,
                heights,
                resolution,
                terrainPos,
                terrainSize
            );
        }

        data.SetHeights(
            0,
            0,
            heights
        );

        terrain.Flush();

        Debug.Log("Generate Road hoàn tất.");
    }

    private void ApplyRoadAtPoint(
        Vector3 roadWorldPos,
        float[,] heights,
        int resolution,
        Vector3 terrainPos,
        Vector3 terrainSize
    )
    {
        float fullWidth =
            roadWidth * 0.5f + blendWidth;

        float normalizedX =
            (roadWorldPos.x - terrainPos.x)
            / terrainSize.x;

        float normalizedZ =
            (roadWorldPos.z - terrainPos.z)
            / terrainSize.z;

        int centerX =
            Mathf.RoundToInt(
                normalizedX * (resolution - 1)
            );

        int centerZ =
            Mathf.RoundToInt(
                normalizedZ * (resolution - 1)
            );

        float metersPerPixelX =
            terrainSize.x / (resolution - 1);

        float metersPerPixelZ =
            terrainSize.z / (resolution - 1);

        int radiusX =
            Mathf.CeilToInt(
                fullWidth / metersPerPixelX
            );

        int radiusZ =
            Mathf.CeilToInt(
                fullWidth / metersPerPixelZ
            );

        for (
            int z = centerZ - radiusZ;
            z <= centerZ + radiusZ;
            z++
        )
        {
            if (z < 0 || z >= resolution)
                continue;

            for (
                int x = centerX - radiusX;
                x <= centerX + radiusX;
                x++
            )
            {
                if (x < 0 || x >= resolution)
                    continue;

                float worldX =
                    terrainPos.x
                    + (
                        x / (float)(resolution - 1)
                      )
                    * terrainSize.x;

                float worldZ =
                    terrainPos.z
                    + (
                        z / (float)(resolution - 1)
                      )
                    * terrainSize.z;

                float distanceXZ =
                    Vector2.Distance(
                        new Vector2(
                            worldX,
                            worldZ
                        ),
                        new Vector2(
                            roadWorldPos.x,
                            roadWorldPos.z
                        )
                    );

                if (distanceXZ > fullWidth)
                    continue;

                float targetWorldHeight =
                    roadWorldPos.y
                    + roadHeightOffset;

                float targetNormalizedHeight =
                    (
                        targetWorldHeight
                        - terrainPos.y
                    )
                    / terrainSize.y;

                targetNormalizedHeight =
                    Mathf.Clamp01(
                        targetNormalizedHeight
                    );

                float halfRoad =
                    roadWidth * 0.5f;

                if (distanceXZ <= halfRoad)
                {
                    heights[z, x] =
                        targetNormalizedHeight;
                }
                else
                {
                    float blendT =
                        Mathf.InverseLerp(
                            halfRoad,
                            fullWidth,
                            distanceXZ
                        );

                    blendT =
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            blendT
                        );

                    heights[z, x] =
                        Mathf.Lerp(
                            targetNormalizedHeight,
                            heights[z, x],
                            blendT
                        );
                }
            }
        }
    }

    [ContextMenu("Restore Terrain")]
    public void RestoreTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain chưa được gán.");
            return;
        }

        if (!hasBackup || originalHeights == null)
        {
            Debug.LogWarning(
                "Chưa có Terrain backup."
            );

            return;
        }

        terrain.terrainData.SetHeights(
            0,
            0,
            originalHeights
        );

        terrain.Flush();

        Debug.Log("Đã Restore Terrain.");
    }

    private void OnDrawGizmos()
    {
        if (!drawPreview)
            return;

        if (splineContainer == null)
            return;

        Spline spline =
            splineContainer.Spline;

        float splineLength =
            spline.GetLength();

        if (splineLength <= 0f)
            return;

        int samples =
            Mathf.Max(
                2,
                Mathf.CeilToInt(
                    splineLength
                    / Mathf.Max(
                        sampleDistance,
                        0.5f
                    )
                )
            );

        Gizmos.color = Color.yellow;

        Vector3 previousLeft =
            Vector3.zero;

        Vector3 previousRight =
            Vector3.zero;

        for (int i = 0; i <= samples; i++)
        {
            float t =
                i / (float)samples;

            float3 localPos =
                spline.EvaluatePosition(t);

            float3 localTangent =
                spline.EvaluateTangent(t);

            Vector3 worldPos =
                splineContainer.transform
                    .TransformPoint(
                        (Vector3)localPos
                    );

            Vector3 tangent =
                splineContainer.transform
                    .TransformDirection(
                        (Vector3)localTangent
                    )
                    .normalized;

            Vector3 side =
                Vector3.Cross(
                    Vector3.up,
                    tangent
                )
                .normalized;

            Vector3 left =
                worldPos
                - side * roadWidth * 0.5f;

            Vector3 right =
                worldPos
                + side * roadWidth * 0.5f;

            Gizmos.DrawLine(
                left,
                right
            );

            if (i > 0)
            {
                Gizmos.DrawLine(
                    previousLeft,
                    left
                );

                Gizmos.DrawLine(
                    previousRight,
                    right
                );
            }

            previousLeft = left;
            previousRight = right;
        }
    }
}