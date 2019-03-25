using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AssetObjectsPacks {

    public class PreviewHandler {

        public void OnInteractivePreviewGUI(Rect r, GUIStyle background) { 
            if (preview != null) preview.OnInteractivePreviewGUI(r, background); 
        }
        public void OnPreviewSettings() { 
            if (preview != null) preview.OnPreviewSettings();
        }

        public PreviewToggler previewToggler = new PreviewToggler();
        Editor preview;
        
        Dictionary<int, EventStatePackInfo> packInfos;
        ElementSelectionSystem selectionSystem;

        public void OnEnable(Dictionary<int, EventStatePackInfo> packInfos, ElementSelectionSystem selectionSystem) {
            this.packInfos = packInfos;
            this.selectionSystem = selectionSystem;
        }
        
        //no repeat ids (force same pack)
        public void RebuildPreviewEditor () {
        
            if (preview != null) Editor.DestroyImmediate(preview);
            if (!previewToggler.previewOpen) return;

            IEnumerable<Vector2Int> elementsInSelection = selectionSystem.GetElementsInSelection();

            if (elementsInSelection == null || elementsInSelection.Count() == 0) return;
        
            //check if all same pack
            int packID = elementsInSelection.First().y;
            foreach (var i in elementsInSelection) {
                if (packID != i.y) return;
            }

            EventStatePackInfo packInfo = packInfos[packID];
            preview = Editor.CreateEditor( elementsInSelection.Generate(e => packInfo.GetObjectRefForID(e.x) ).ToArray());

            preview.HasPreviewGUI();
            preview.OnInspectorGUI();
            preview.OnPreviewSettings();

            //auto play single selection for animations
            if (packInfo.pack.assetType == "AnimationClip") {     
                if (elementsInSelection.Count() == 1) {
                    // preview_editor.m_AvatarPreview.timeControl.playing = true
                    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                    var avatarPreview = preview.GetType().GetField("m_AvatarPreview", flags).GetValue(preview);
                    var timeControl = avatarPreview.GetType().GetField("timeControl", flags).GetValue(avatarPreview);
                    var setter = timeControl.GetType().GetProperty("playing", flags).GetSetMethod(true);
                    setter.Invoke(timeControl, new object[] { true });
                }
            }
        }
        public void PreviewToggle () {
            previewToggler.TogglePreview(!previewToggler.previewOpen);
            if (previewToggler.previewOpen) {
                RebuildPreviewEditor();
            }
        }
    }
}