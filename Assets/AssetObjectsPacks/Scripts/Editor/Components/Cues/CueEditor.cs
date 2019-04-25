using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(Cue))]
    public class CueEditor : Editor {
        EditorProp so;
        void OnEnable () {
            so = new EditorProp( serializedObject );
        }
        bool HasSubCues() {
            Transform soT = (target as MonoBehaviour).transform;
            for (int i = 0; i < soT.childCount; i++) {
                if (soT.GetChild(i).GetComponent<Cue>() != null) return true;
            }
            return false;
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();
            EditorGUI.indentLevel++;
            GUIUtils.StartBox();
            GUIUtils.DrawProp(so["gizmoColor"], new GUIContent("Gizmo Color"));
            GUIUtils.Space();
            GUIUtils.DrawProp(so["behavior"], new GUIContent("Cue Behavior"));

            EditorProp repeats = so["repeats"];
            GUIUtils.DrawProp(repeats, new GUIContent("Repeats", "How Many times this cue repeats"));
            if (repeats.intValue < 1) repeats.SetValue(1);
            if (HasSubCues()) {
                GUIUtils.DrawToggleProp(so["useRandomPlaylist"], new GUIContent("Use Random Sub-Cue", "Plays one sub-cue at random, not all sequentially"));
            }
            GUIUtils.EndBox();
            EditorGUI.indentLevel--;
            GUIUtils.EndCustomEditor(so);                
        }
    }
}