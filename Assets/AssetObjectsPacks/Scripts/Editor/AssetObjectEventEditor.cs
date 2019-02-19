using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(AssetObjectEvent))]
    public class AssetObjectEventEditor : Editor
    {
        GUIContent snap_style_gui = new GUIContent("Snap Style", "If the event should wait for the player to snap to the event transform before being considered ready");
        GUIContent pos_smooth_gui = new GUIContent("Position Time (s)");
        GUIContent rot_smooth_gui = new GUIContent("Rotation Time (s)");
        new AssetObjectEvent target;
        bool array_fold;
        void OnEnable () {
            this.target = base.target as AssetObjectEvent;
        }
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(AssetObjectEvent.snap_player_style_field), snap_style_gui);
            if (target.snapPlayerStyle == AssetObjectEvent.SnapPlayerStyle.Smooth) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(AssetObjectEvent.smooth_pos_time_field), pos_smooth_gui);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(AssetObjectEvent.smooth_rot_time_field), rot_smooth_gui);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty(AssetObjectEvent.playlist_field));
            if (target.playlist == null) {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                GUIUtils.DrawArrayProp( new EditorProp ( serializedObject.FindProperty(AssetObjectEvent.event_packs_field) ), ref array_fold);
            }
                
            if (EditorGUI.EndChangeCheck()) {                
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}