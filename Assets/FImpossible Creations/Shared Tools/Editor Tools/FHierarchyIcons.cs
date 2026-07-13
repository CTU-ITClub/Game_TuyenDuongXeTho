#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/* Code made by LaneFox from Unity Community Forum */

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class FHierarchyIcons
{
    static FHierarchyIcons()
    {
#if UNITY_EDITOR
        // Sửa lỗi ở đây: Đổi hierarchyWindowItemOnGUI thành hierarchyWindowItemByEntityIdOnGUI
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += EvaluateIcons;
#endif
    }

#if UNITY_EDITOR
    // Đổi tham số từ int sang EntityId cho khớp với Delegate mới và bọc toàn bộ bằng #if UNITY_EDITOR
    private static void EvaluateIcons(EntityId entityId, Rect selectionRect)
    {
        // Sửa lỗi ở đây: Đổi InstanceIDToObject thành EntityIdToObject
        GameObject go = EditorUtility.EntityIdToObject(entityId) as GameObject;
        if (go == null) return;

        IFHierarchyIcon slotCon = go.GetComponent<IFHierarchyIcon>();
        if (slotCon != null) DrawIcon(slotCon.EditorIconPath, selectionRect);
    }
#endif

    private static void DrawIcon(string texName, Rect rect)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(texName)) return;
        Rect r = new Rect(rect.x + rect.width - 16f, rect.y, 16f, 16f);
        GUI.DrawTexture(r, GetTex(texName));
#endif
    }

    private static Texture2D GetTex(string name)
    {
#if UNITY_EDITOR
        return (Texture2D)Resources.Load(name);
#else
        return null;
#endif
    }
}

public interface IFHierarchyIcon
{
    string EditorIconPath { get; }
}

/*

{...}
    public class ItemUiSlot : MonoBehaviour, IDropHandler, FIHierarchyIcon
    {
        public string EditorIconPath { get { return "LogoGrey";  } }
{...}

*/