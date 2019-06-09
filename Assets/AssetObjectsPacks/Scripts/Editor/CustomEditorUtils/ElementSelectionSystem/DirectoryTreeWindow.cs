using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    public class DirectoryTreeWindow : EditorWindow {
        GUIContent closeGUI = new GUIContent("Close");
        Vector2 scrollPos;
        ElementSelectionSystem baseSystem;

        DirectoryTreeWindow InitializeInternal (ElementSelectionSystem baseSystem) {
            this.baseSystem = baseSystem;
            position = new Rect (256, 256, 400, 400);
            return this;            
        }
        public static DirectoryTreeWindow NewDirectoryTreeWindow(ElementSelectionSystem baseSystem, string title="Directory Tree:")
        {
            return ((DirectoryTreeWindow)GetWindow(typeof(DirectoryTreeWindow), true, title, false)).InitializeInternal(baseSystem);
        }
        void OnDestroy () {
            if (baseSystem != null) {
                baseSystem.showDirectoryTree = false;
            }
        }
        bool CloseIfError () {
            if (this.baseSystem == null) {
                return true;
            }
            return false;
        }
        void OnGUI()
        {
            if (CloseIfError()) {
                Close();
                return;
            }

            UnityEngine.Event inputEvent = UnityEngine.Event.current;
            
            bool mouseUp = inputEvent.rawType == EventType.MouseUp;

            GUIUtils.StartBox(1);

            GUI.backgroundColor = Colors.white;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            Vector2 mousePos = inputEvent.mousePosition;
            
            baseSystem.GetDirectoryTreeElement(0).DrawDirectoryTreeElement(mouseUp, mousePos);

            baseSystem.dragDropHandler.UpdateLoopReceiverOnly(mousePos);            
            
            EditorGUILayout.EndScrollView();
                        
            GUILayout.FlexibleSpace();

            if (GUIUtils.Button(closeGUI, GUIStyles.button, Colors.blue, Colors.black )) {
                Close();
            }

            GUIUtils.EndBox(1);

            baseSystem.serializedObject.SaveObject();
        }
        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
