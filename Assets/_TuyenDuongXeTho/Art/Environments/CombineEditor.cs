using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CombineEditor
{
    [MenuItem("Tools/Combine Selected Meshes")]
    static void CombineSelectedMeshes()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected!");
            return;
        }

        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (var go in selection)
        {
            meshFilters.AddRange(go.GetComponentsInChildren<MeshFilter>());
        }

        if (meshFilters.Count == 0)
        {
            Debug.LogWarning("No MeshFilters found!");
            return;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        for (int i = 0; i < meshFilters.Count; i++)
        {
            if (meshFilters[i].sharedMesh == null)
                continue;

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh
        {
            name = "CombinedMesh"
        };
        combinedMesh.CombineMeshes(combine, true, true);
        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();
        combinedMesh.Optimize();

        // Create GameObject
        GameObject combinedGO = new GameObject("Combined Mesh");
        MeshFilter mf = combinedGO.AddComponent<MeshFilter>();
        MeshRenderer mr = combinedGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = combinedMesh;
        mr.sharedMaterial = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        // Reset transform
        combinedGO.transform.position = Vector3.zero;
        combinedGO.transform.rotation = Quaternion.identity;
        combinedGO.transform.localScale = Vector3.one;

        // Save asset safely
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/CombinedMesh.asset");
        AssetDatabase.CreateAsset(combinedMesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Mesh Combined Successfully!");
    }
}
