using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public static class PagesGUI {
        static readonly GUIContent prevPageGUI = new GUIContent(" << "), nextPageGUI = new GUIContent(" >> ");
        
        public static void DrawPages (GUIStyle buttonStyle, out bool prevPage, out bool nextPage, GUIContent curPageGUI) {
            GUIUtils.StartBox(0);
            EditorGUILayout.BeginHorizontal();
            prevPage = GUIUtils.Button(prevPageGUI, true, buttonStyle);

            GUIStyle s = EditorStyles.label;
            TextAnchor ol = s.alignment;
            s.alignment = TextAnchor.LowerCenter;
            EditorGUILayout.LabelField(curPageGUI, s);
            s.alignment = ol;

            nextPage = GUIUtils.Button(nextPageGUI, true, buttonStyle);
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox(0);
        }
    }
}
