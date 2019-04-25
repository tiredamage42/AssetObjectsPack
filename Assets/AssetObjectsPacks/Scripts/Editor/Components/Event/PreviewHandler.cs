using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace AssetObjectsPacks {

    public class PreviewHandler {

        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public void OnPreviewSettings() { 
            if (preview != null) preview.OnPreviewSettings();
        }

        public bool previewOpen;
        Editor preview;

        BindingFlags getPreviewFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type inspectorWindowType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
        Dictionary<int, EventStatePackInfo> packInfos;
        ElementSelectionSystem selectionSystem;

        public void OnEnable(Dictionary<int, EventStatePackInfo> packInfos, ElementSelectionSystem selectionSystem) {
            this.packInfos = packInfos;
            this.selectionSystem = selectionSystem;
        }
        
        //no repeat ids (force same pack)
        public void RebuildPreviewEditor () {
        
            if (preview != null) Editor.DestroyImmediate(preview);
            if (!previewOpen) return;

            IEnumerable<Vector2Int> elementsInSelection = selectionSystem.GetElementsInSelection();

            if (elementsInSelection == null || elementsInSelection.Count() == 0) return;
        
            //check if all same pack
            int packID = elementsInSelection.First().y;
            EventStatePackInfo packInfo = packInfos[packID];
            
            if (packInfo.pack.isCustom) {
                return;
            }
            
            foreach (var i in elementsInSelection) {
                if (packID != i.y) return;
            }


            preview = Editor.CreateEditor( elementsInSelection.Generate(e => packInfo.GetObjectRefForID(e.x) ).ToArray());

            preview.HasPreviewGUI();
            preview.OnInspectorGUI();
            preview.OnPreviewSettings();

            //auto play single selection for animations
            if (packInfo.pack.assetType == "AnimationClip") {     
                if (elementsInSelection.Count() == 1) {
                    // preview_editor.m_AvatarPreview.timeControl.playing = true
                    var avatarPreview = preview.GetType().GetField("m_AvatarPreview", getPreviewFlags).GetValue(preview);
                    var timeControl = avatarPreview.GetType().GetField("timeControl", getPreviewFlags).GetValue(avatarPreview);
                    var setter = timeControl.GetType().GetProperty("playing", getPreviewFlags).GetSetMethod(true);
                    setter.Invoke(timeControl, new object[] { true });
                }
            }
        }
        public void PreviewToggle () {
            TogglePreview(!previewOpen);
            if (previewOpen) {
                RebuildPreviewEditor();
            }
        }

        
        object[] nullParams = new object[] {};
        void TogglePreview(bool enabled)
        {
            previewOpen = enabled;
            //get first inspector window with a preview window
            var inspector = Resources.FindObjectsOfTypeAll(inspectorWindowType).Where( o => o.GetType().GetField("m_PreviewWindow", getPreviewFlags) != null).ToArray()[0];

            //get the preview resizer on that window
            var previewResizer = inspector.GetType().GetField("m_PreviewResizer", getPreviewFlags).GetValue(inspector);
            
            //toggle expand if not already expanded
            var t = previewResizer.GetType();
            bool expanded = (bool)t.GetMethod("GetExpanded", getPreviewFlags).Invoke(previewResizer, nullParams);
            if (expanded != enabled) t.GetMethod("ToggleExpanded", getPreviewFlags).Invoke(previewResizer, nullParams);    
        }
    }
}