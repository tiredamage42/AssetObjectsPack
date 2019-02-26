using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(Cue))]
    public class CueEditor : Editor
    {
        new Cue target;
        EditorProp so;
        void OnEnable () {
            this.target = base.target as Cue;
            so = new EditorProp( serializedObject );
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            GUIUtils.StartCustomEditor();

            //GUIUtils.Space(1);

            GUIUtils.StartBox(0);

            EditorGUI.indentLevel++;
            

            GUIUtils.DrawProp(so[Cue.sendMessageField], new GUIContent("Send Message: ", "Method should take a Transform for parameter"));

            GUIUtils.DrawProp(so[Cue.snap_player_style_field], new GUIContent("Snap Style", "If the event should wait for the player to snap to the event transform before being considered ready"));
            if (target.snapPlayerStyle == Cue.SnapPlayerStyle.Smooth) {
                EditorGUI.indentLevel++;
                GUIUtils.DrawProp(so[Cue.smooth_pos_time_field], new GUIContent("Position Time (s)"));
                GUIUtils.DrawProp(so[Cue.smooth_rot_time_field], new GUIContent("Rotation Time (s)"));
                EditorGUI.indentLevel--;
            }
            GUIUtils.DrawProp(so[Cue.playlist_field]);
            if (target.playlist == null) {
                GUIUtils.DrawArrayProp( so[Cue.event_packs_field] );
            }
            EditorGUI.indentLevel--;
            
            GUIUtils.EndBox(0);

            GUIUtils.EndCustomEditor(so);
                
        }
    }
}