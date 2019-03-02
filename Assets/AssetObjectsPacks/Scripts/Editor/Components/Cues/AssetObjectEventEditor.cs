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

            GUIUtils.StartBox(1);

            EditorGUI.indentLevel++;


            GUIUtils.DrawMultiLineStringProp(
                so[Cue.sendMessageField], 
                new GUIContent("<b>Send Messages</b> (seperated by commas): ", "Methods should take a Transform for parameter"), 
                false, GUILayout.MinHeight(32)
            );
            
            GUIUtils.Space();

            GUIUtils.DrawProp(so[Cue.snap_player_style_field], new GUIContent("Snap Style", "If the event should wait for the player to snap to the event transform before being considered ready"));
                
            if (target.snapPlayerStyle == Cue.SnapPlayerStyle.Smooth) {
                EditorGUI.indentLevel--;
                GUIUtils.BeginIndent(2);
                GUIUtils.StartBox(Colors.darkGray);
                GUIUtils.DrawProp(so[Cue.smooth_pos_time_field], new GUIContent("Position Time (s)"));
                GUIUtils.DrawProp(so[Cue.smooth_rot_time_field], new GUIContent("Rotation Time (s)"));
                GUIUtils.EndBox();
                GUIUtils.EndIndent();
                EditorGUI.indentLevel++;
                GUIUtils.Space();
            }
                
            GUIUtils.DrawProp(so[Cue.playlist_field], new GUIContent("Playlist", "Playlist to trigger"));
            
            EditorGUI.indentLevel--;
            
            if (target.playlist == null) {
                GUIUtils.Space();
                GUIUtils.DrawObjArrayProp( so[Cue.event_packs_field] );
            }


            GUIUtils.EndBox(1);

            GUIUtils.EndCustomEditor(so);
                
        }
    }
}