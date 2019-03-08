using UnityEngine;
using UnityEditor;
//using System.Linq;
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
            
            GUIUtils.DrawProp(so[Cue.repeatsField], new GUIContent("Repeats", "How Many times this cue repeats"));
            if (so[Cue.repeatsField].intValue < 1) {
                so[Cue.repeatsField].SetValue(1);
            }
            

            const float lineHeight = 16;

            GUIUtils.DrawMultiLineStringProp(
                so[Cue.messagesBlockField], 
                new GUIContent("<b>Send Messages</b>:", ""), 
                false, GUILayout.MinHeight(so[Cue.messagesBlockField].stringValue.Split('\n').Length * lineHeight)
            );


            GUIUtils.DrawMultiLineStringProp(
                so[Cue.sendMessageField], 
                new GUIContent("<b>Send Messages</b> (seperated by slashes '/' ): ", ""), 
                false, GUILayout.MinHeight(32)
            );

            GUIUtils.DrawMultiLineStringProp(
                so[Cue.postMessageField], 
                new GUIContent("<b>Post Play Messages</b> (seperated by slashes '/' ): ", ""), 
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

                GUIUtils.DrawProp(so[Cue.playImmediateField], new GUIContent("Play Immediate", "Play before the position snap"));

                GUIUtils.EndBox();
                GUIUtils.EndIndent();
                EditorGUI.indentLevel++;
                GUIUtils.Space();
            }
                
            GUIUtils.DrawProp(so[Cue.playlist_field], new GUIContent("Playlist", "Playlist to trigger"));
            
            EditorGUI.indentLevel--;
            
            if (target.playlist == null) {

                GUIUtils.DrawProp(so[Cue.overrideDurationField], new GUIContent("Override Duration", "negative values give control to the events"));
            
                GUIUtils.Space();
                GUIUtils.DrawObjArrayProp( so[Cue.event_packs_field] );
            }


            GUIUtils.EndBox(1);

            GUIUtils.EndCustomEditor(so);
                
        }
    }
}