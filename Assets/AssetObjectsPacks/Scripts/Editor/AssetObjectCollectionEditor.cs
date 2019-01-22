
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public abstract class AssetObjectEventEditor<P> : Editor
        //where O : AssetObjectInstance, new()
        where P : Object 
    {
        protected abstract string PackName();
        protected abstract string PackFileExtension();
        protected abstract void MakeAssetObjectInstanceDefault(SerializedProperty obj_instance);
        protected abstract string[] InstanceFieldNames();
        protected abstract GUIContent[] InstanceFieldLabels();

        AssetObjectListGUI<P> oe;
        AssetObjectListGUI<P> objectExplorer {
            get {
                if (oe == null) {
                    oe = new AssetObjectListGUI<P>();
                    /*
                    (target as AssetObjectHolder).multi_edit_instance = new O();

                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(target);
                    
                    serializedObject.Update();


                    Debug.Log((target as AssetObjectHolder).multi_edit_instance);

                    SerializedProperty multi_edit_instance = serializedObject.FindProperty("multi_edit_instance");
                    Debug.Log(multi_edit_instance);
                     */


                    objectExplorer.OnEnable(PackName(), PackFileExtension(), MakeAssetObjectInstanceDefault, InstanceFieldNames(), InstanceFieldLabels(), serializedObject);
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
    }

}







