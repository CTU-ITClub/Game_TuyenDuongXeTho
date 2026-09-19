using UnityEngine;
using UnityEditor;

public class TerrainBatchOptimizer : EditorWindow
{
    // ===== HEIGHTMAP / TEXTURE =====
    private int heightmapResolution = 257;
    private int controlTextureResolution = 512;
    private int baseMapResolution = 512;

    // ===== DETAILS / GRASS =====
    private int detailResolution = 512;
    private int detailResolutionPerPatch = 32;
    private float detailDistance = 50f;
    private float detailDensity = 0.7f;

    // ===== TREES =====
    private float treeDistance = 350f;
    private float billboardStart = 50f;
    private float fadeLength = 5f;
    private int maxMeshTrees = 40;


    [MenuItem("Tools/Terrain Batch Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<TerrainBatchOptimizer>("Terrain Optimizer");
    }


    private void OnGUI()
    {
        GUILayout.Label("Terrain Batch Optimizer", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // =====================================================
        // HEIGHTMAP
        // =====================================================

        GUILayout.Label("Heightmap / Texture", EditorStyles.boldLabel);

        heightmapResolution =
            EditorGUILayout.IntField(
                "Heightmap Resolution",
                heightmapResolution
            );

        controlTextureResolution =
            EditorGUILayout.IntField(
                "Control Texture Resolution",
                controlTextureResolution
            );

        baseMapResolution =
            EditorGUILayout.IntField(
                "Base Map Resolution",
                baseMapResolution
            );


        EditorGUILayout.Space();


        // =====================================================
        // DETAIL
        // =====================================================

        GUILayout.Label("Grass / Details", EditorStyles.boldLabel);

        detailResolution =
            EditorGUILayout.IntField(
                "Detail Resolution",
                detailResolution
            );

        detailResolutionPerPatch =
            EditorGUILayout.IntField(
                "Detail Resolution Per Patch",
                detailResolutionPerPatch
            );

        detailDistance =
            EditorGUILayout.FloatField(
                "Detail Distance",
                detailDistance
            );

        detailDensity =
            EditorGUILayout.Slider(
                "Detail Density",
                detailDensity,
                0f,
                1f
            );


        EditorGUILayout.Space();


        // =====================================================
        // TREE
        // =====================================================

        GUILayout.Label("Trees", EditorStyles.boldLabel);

        treeDistance =
            EditorGUILayout.FloatField(
                "Tree Distance",
                treeDistance
            );

        billboardStart =
            EditorGUILayout.FloatField(
                "Billboard Start",
                billboardStart
            );

        fadeLength =
            EditorGUILayout.FloatField(
                "Fade Length",
                fadeLength
            );

        maxMeshTrees =
            EditorGUILayout.IntField(
                "Max Mesh Trees",
                maxMeshTrees
            );


        EditorGUILayout.Space(20);


        // =====================================================
        // BUTTON
        // =====================================================

        if (GUILayout.Button(
            "APPLY TO ALL TERRAINS",
            GUILayout.Height(40)))
        {
            ApplySettings();
        }
    }


    private void ApplySettings()
    {
        Terrain[] terrains =
            FindObjectsByType<Terrain>(
                FindObjectsSortMode.None
            );

        if (terrains.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy Terrain!");
            return;
        }


        foreach (Terrain terrain in terrains)
        {
            TerrainData data = terrain.terrainData;

            Undo.RecordObject(
                terrain,
                "Terrain Batch Optimize"
            );

            Undo.RecordObject(
                data,
                "Terrain Batch Optimize"
            );


            // ============================
            // HEIGHTMAP
            // ============================

            if (data.heightmapResolution != heightmapResolution)
                data.heightmapResolution =
                    heightmapResolution;


            // ============================
            // TEXTURES
            // ============================

            if (data.alphamapResolution != controlTextureResolution)
                data.alphamapResolution =
                    controlTextureResolution;

            data.baseMapResolution =
                baseMapResolution;


            // ============================
            // DETAILS
            // ============================

            data.SetDetailResolution(
                detailResolution,
                detailResolutionPerPatch
            );

            terrain.detailObjectDistance =
                detailDistance;

            terrain.detailObjectDensity =
                detailDensity;


            // ============================
            // TREES
            // ============================

            terrain.treeDistance =
                treeDistance;

            terrain.treeBillboardDistance =
                billboardStart;

            terrain.treeCrossFadeLength =
                fadeLength;

            terrain.treeMaximumFullLODCount =
                maxMeshTrees;


            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(terrain);
        }


        AssetDatabase.SaveAssets();


        Debug.Log(
            $"✅ Optimized {terrains.Length} Terrains!"
        );
    }
}