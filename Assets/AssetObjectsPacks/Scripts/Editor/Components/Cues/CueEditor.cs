using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(Cue))]
    public class CueEditor : Editor {
        const string repeatsField = "repeats";
        const string gizmoColorField = "gizmoColor", useRandomPlaylistField = "useRandomPlaylist";
        const string behaviorField = "behavior";
               
        EditorProp so;
        Transform soT;
        GUIContent useRandomPlaylistGUI = new GUIContent("Use Random Playlist Choice", "Sub playlists/cues play one random choice, not all sequentially");
        GUIContent repeatsGUI = new GUIContent("Repeats", "How Many times this cue repeats");
        GUIContent gizmoColorgUI = new GUIContent("Gizmo Color");
        GUIContent behaviorGUI = new GUIContent("Cue Behavior");
        Cue cue;

        void OnEnable () {
            so = new EditorProp( serializedObject );
            soT = (target as MonoBehaviour).transform;
            cue = target as Cue;
        }
        
        bool HasSubCues() {
            if (soT.childCount == 0) return false;
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
            GUIUtils.DrawProp(so[gizmoColorField], gizmoColorgUI);
            GUIUtils.Space();
            GUIUtils.DrawProp(so[behaviorField], behaviorGUI);
            GUIUtils.DrawProp(so[repeatsField], repeatsGUI);
            if (so[repeatsField].intValue < 1) so[repeatsField].SetValue(1);
            if (HasSubCues()) {
                GUIUtils.DrawToggleProp(so[useRandomPlaylistField], useRandomPlaylistGUI);
            }
            GUIUtils.EndBox();

            //runtime transform tracking
            if (Application.isPlaying) {
                GUIUtils.StartBox();
                cue.transformTracker.tracking = GUIUtils.ToggleButton(new GUIContent("Track Transform"), GUIStyles.button, cue.transformTracker.tracking, out _);
                GUIUtils.EndBox();
            }
            
            EditorGUI.indentLevel--;
            GUIUtils.EndCustomEditor(so);                
        }
    }
}