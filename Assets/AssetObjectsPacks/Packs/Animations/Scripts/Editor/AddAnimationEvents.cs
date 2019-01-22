

using UnityEngine;
using UnityEditor;
/*
    Use to add multiple animation events to multiple animations
*/
namespace AnimCorpus {
    public class AddAnimationEvents : ScriptableWizard {
        [System.Serializable] public struct AnimEvent {
            public float time;
            public string functionName;
            public float floatValue;
            public int intValue;
            public string stringValue;
        }
        public AnimEvent[] new_events = new AnimEvent[0];

        [MenuItem("AnimationCorpus/2. Add Animation Events")]
        static void CreateWizard() {
            ScriptableWizard.DisplayWizard<AddAnimationEvents>("Add Animation Events", "Add");
        }
        void OnWizardCreate() {
            GameObject[] animations = Selection.gameObjects;
            for (int i = 0; i < animations.Length; i++) {
                string path = AssetDatabase.GetAssetPath(animations[i]);
                AddEvents(AssetDatabase.GetAssetPath(animations[i]));
            }
        }
        void AddEvents (string file_path) {
            ModelImporter importer = AssetImporter.GetAtPath(file_path) as ModelImporter;
            if (importer == null) {
                
                return;
            }
            if (importer.animationType != ModelImporterAnimationType.Human) {
                Debug.LogError("Avatar must be human!");
                return;
            }
            if(importer.clipAnimations.Length == 0)
                importer.clipAnimations = importer.defaultClipAnimations;        
            SerializedObject serializedObject = new SerializedObject(importer);
            new AnimationClipInfoProperties(serializedObject.FindProperty("m_ClipAnimations").GetArrayElementAtIndex(0)).SetEvents(new_events);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.ImportAsset((importer.assetPath));
        }
        class AnimationClipInfoProperties {
	        public AnimationClipInfoProperties(SerializedProperty prop) { 
                m_Property = prop; 
            }
            SerializedProperty m_Property;
            SerializedProperty Get(string property) { return m_Property.FindPropertyRelative(property); }
            public void SetEvents(AnimEvent[] newEvents) {
                SerializedProperty events = Get("events");
                if (events != null && events.isArray) {
                    events.ClearArray();
                    foreach (AnimEvent evt in newEvents) {
                        events.InsertArrayElementAtIndex(events.arraySize);
                        int index = events.arraySize - 1;
                        events.GetArrayElementAtIndex(index).FindPropertyRelative("floatParameter").floatValue = evt.floatValue;
                        events.GetArrayElementAtIndex(index).FindPropertyRelative("functionName").stringValue = evt.functionName;
                        events.GetArrayElementAtIndex(index).FindPropertyRelative("intParameter").intValue = evt.intValue;
                        events.GetArrayElementAtIndex(index).FindPropertyRelative("objectReferenceParameter").objectReferenceValue = null;//evt.objectReferenceParameter;
                        events.GetArrayElementAtIndex(index).FindPropertyRelative("data").stringValue = evt.stringValue;
                        events.GetArrayElementAtIndex(index).FindPropertyRelative("time").floatValue = evt.time;
                    
                    }
                }
            }
            /*
            public AnimationEvent[] GetEvents (SerializedProperty sp){
                SerializedProperty serializedProperty = sp.FindPropertyRelative("events");
                AnimationEvent[] array = null;
                if (serializedProperty != null && serializedProperty.isArray){
                    int count = serializedProperty.arraySize;
                    array = new AnimationEvent[count];
                    for (int i = 0; i < count; i++){
                        AnimationEvent animationEvent = new AnimationEvent();
                        SerializedProperty eventProperty = serializedProperty.GetArrayElementAtIndex (i);
                        animationEvent.floatParameter = eventProperty.FindPropertyRelative ("floatParameter").floatValue;
                        animationEvent.functionName = eventProperty.FindPropertyRelative ("functionName").stringValue;
                        animationEvent.intParameter = eventProperty.FindPropertyRelative ("intParameter").intValue;
                        animationEvent.objectReferenceParameter = eventProperty.FindPropertyRelative ("objectReferenceParameter").objectReferenceValue;
                        animationEvent.stringParameter = eventProperty.FindPropertyRelative ("data").stringValue;
                        animationEvent.time     = eventProperty.FindPropertyRelative ("time").floatValue;
                        array [i] = animationEvent;
                    }
                }
                return array;
            }
            */
        }
    }
}

