using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {

    [CustomEditor(typeof(AssetObjectEventPlaylist))]
    public class AssetObjectEventPlaylistEditor : Editor {
        GUIContent sync_channels_gui = new GUIContent("Sync Channels", "channels change events at same time when ready as opposed to staggered (whenever last event is done)");
        SerializedProperty loop_prop, sync_prop; //, interrupt_prop;
        void OnEnable () {
            loop_prop = serializedObject.FindProperty("isLooped");
            //interrupt_prop = serializedObject.FindProperty("interruptsOthers");            
            sync_prop = serializedObject.FindProperty("syncChannels");
        }
        public override void OnInspectorGUI () {
            EditorGUI.BeginChangeCheck();
            //EditorGUILayout.PropertyField(interrupt_prop);            
            EditorGUILayout.PropertyField(loop_prop);
            EditorGUILayout.PropertyField(sync_prop, sync_channels_gui);
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}