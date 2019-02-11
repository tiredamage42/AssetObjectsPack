/*
using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks.Animations {

    [CustomEditor(typeof(AnimationScene))]
    public class AnimationSceneEditor : Editor {

        GUIContent sync_roles_gui = new GUIContent("Sync Roles", "roles change cues at same time when ready as opposed to staggered (whenever last cue is done)");
        SerializedProperty loop_prop, interrupt_prop, sync_prop;

        void OnEnable () {
            loop_prop = serializedObject.FindProperty("isLooped");
            interrupt_prop = serializedObject.FindProperty("interruptsOthers");            
            sync_prop = serializedObject.FindProperty("syncRoles");
        }

        public override void OnInspectorGUI () {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(loop_prop);
            EditorGUILayout.PropertyField(interrupt_prop);            
            EditorGUILayout.PropertyField(sync_prop, sync_roles_gui);
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
 */
