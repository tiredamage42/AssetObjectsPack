using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {

    [CustomEditor(typeof(PacksManager))]

    public class PacksManagerEditor : Editor {
        
        public static EditorProp GetPacksList () {
            if (AssetObjectsEditor.packManager == null) return null;
            return new EditorProp ( new SerializedObject ( AssetObjectsEditor.packManager ).FindProperty( packsField ) );
        }

        const string packsField = "packs";
        EditorProp packs;
        string[] warnings, errors;
        int curPackI, noIDsCount;

        EditorProp so;
        void OnEnable () {
            so = new EditorProp( serializedObject );

            packs = so[ packsField ];
            Reinitialize();
        }
        void Reinitialize () {
            PackEditor.GetErrorsAndWarnings(packs, (packs.arraySize > 0) ? packs[curPackI] : null, out errors, out warnings, out noIDsCount);
        }
        public override void OnInspectorGUI () {
            //base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();
            bool genIDs = PackEditor.GUI.DrawErrorsAndWarnings(errors, warnings, noIDsCount, curPackI);
            PackEditor.GUI.DrawPacks(packs, ref curPackI);
            if (GUIUtils.EndCustomEditor(so) || genIDs) Reinitialize();
        }
    }
}