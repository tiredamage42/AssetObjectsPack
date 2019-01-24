using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace AssetObjectsPacks {
    public abstract class AssetObjectEventEditor : Editor
    {
        protected abstract string PackName();
        protected abstract string PackFileExtension();
        protected abstract string AssetObjectUnityAssetType();
        //protected abstract void MakeAssetObjectInstanceDefault(SerializedProperty obj_instance);
        //protected abstract string[] InstanceFieldNames();
        //protected abstract GUIContent[] InstanceFieldLabels();

        protected abstract AssetObjectParamDef[] DefaultParameters ();



        
        AssetObjectListGUI oe;
        AssetObjectListGUI objectExplorer {
            get {
                if (oe == null) {
                    oe = new AssetObjectListGUI();
                    //objectExplorer.OnEnable(PackName(), AssetObjectUnityAssetType(), PackFileExtension(), MakeAssetObjectInstanceDefault, InstanceFieldNames(), InstanceFieldLabels(), serializedObject, new SerializedObject(this));
                    objectExplorer.OnEnable(PackName(), AssetObjectUnityAssetType(), PackFileExtension(), DefaultParameters(), serializedObject, new SerializedObject(this));
                    serializedObject.ApplyModifiedProperties();
                }
                return oe;
            }
        }
        public override bool HasPreviewGUI() { return objectExplorer.HasPreviewGUI(); }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { objectExplorer.OnInteractivePreviewGUI(r, background); }
        public override void OnPreviewSettings() { objectExplorer.OnPreviewSettings(); }
        
        protected void DrawObjectExplorer(int window_height = 256) {
            objectExplorer.Draw(window_height);
        }    


        const string sLooped = "looped";
        const string sDuration = "duration";
        GUIContent duration_gui = new GUIContent("Duration", "Nagative values for animation duration");
        

        protected void DrawAssetObjectEvent (int window_height = 256) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(sLooped));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(sDuration), duration_gui);
            DrawObjectExplorer();
        }


    }

}







