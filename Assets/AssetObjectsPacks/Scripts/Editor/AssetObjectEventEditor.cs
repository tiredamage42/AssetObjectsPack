using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace AssetObjectsPacks {


    [CustomEditor(typeof(AssetObjectEvent))]
    public class AssetObjectEventEditor : Editor
    {
        GUIContent snap_style_gui = new GUIContent("Snap Style", "If the event should wait for the player to snap to the event transform before being considered ready");
        GUIContent pos_smooth_gui = new GUIContent("Position Time (s)");
        GUIContent rot_smooth_gui = new GUIContent("Rotation Time (s)");
        Dictionary<string, SerializedProperty> script_properties = new Dictionary<string, SerializedProperty>(prop_names.Length);
        new AssetObjectEvent target;
        
        static readonly string[] prop_names = new string[] {
            AssetObjectEvent.snap_player_style_field,
            AssetObjectEvent.smooth_pos_time_field,
            AssetObjectEvent.smooth_rot_time_field,
            AssetObjectEvent.playlist_field,
            AssetObjectEvent.event_packs_field,
        };

        
        void OnEnable () {
            this.target = base.target as AssetObjectEvent;
            InitializeSerializedProperties();
        }

        void InitializeSerializedProperties () {
            int l = prop_names.Length;
            for (int i = 0; i < l; i++) {
                string n = prop_names[i];
                script_properties.Add(n, serializedObject.FindProperty(n));
            }
        }

        bool array_fold;
        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(script_properties[AssetObjectEvent.snap_player_style_field], snap_style_gui);
            if (target.snapPlayerStyle == AssetObjectEvent.SnapPlayerStyle.Smooth) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(script_properties[AssetObjectEvent.smooth_pos_time_field], pos_smooth_gui);
                EditorGUILayout.PropertyField(script_properties[AssetObjectEvent.smooth_rot_time_field], rot_smooth_gui);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(script_properties[AssetObjectEvent.playlist_field]);
            if (target.playlist == null) {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                GUIUtils.ArrayGUI(script_properties[AssetObjectEvent.event_packs_field], ref array_fold);
            }
                
            if (EditorGUI.EndChangeCheck()) {                
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}