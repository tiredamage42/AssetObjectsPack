using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SavePlayModeTransform))]
public class SavePlayModeTransformEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        Transform transform = (target as MonoBehaviour).transform;
        int instanceID = transform.GetInstanceID();

        if (Application.isPlaying) {
            if (GUILayout.Button("Save Tranform")) {
                SavePlayModeTransform.savedTransforms[instanceID] = new AssetObjectsPacks.MiniTransform(transform.localPosition, transform.localRotation);
            }
        }
        else {
            EditorGUILayout.BeginHorizontal();
            if (SavePlayModeTransform.savedTransforms.ContainsKey(instanceID)) {
                if (GUILayout.Button("Load Tranform")) {
                    transform.localPosition = SavePlayModeTransform.savedTransforms[instanceID].pos;
                    transform.localRotation = SavePlayModeTransform.savedTransforms[instanceID].rot;
                    SavePlayModeTransform.savedTransforms.Remove(instanceID);
                }
            }
            if (SavePlayModeTransform.savedTransforms.Count > 0) {
                if (GUILayout.Button("Clear All Saved Transforms")) {
                    SavePlayModeTransform.savedTransforms.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
