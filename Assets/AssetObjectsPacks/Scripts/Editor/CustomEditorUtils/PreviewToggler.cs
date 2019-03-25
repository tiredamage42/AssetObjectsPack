using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEditor;
using System.Linq;

namespace AssetObjectsPacks {
    public class PreviewToggler 
    {

        public bool previewOpen;

        BindingFlags getPreviewFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type inspectorWindowType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
        
        public void TogglePreview(bool enabled)
        {
            previewOpen = enabled;

            //get first inspector window with a preview window
            var inspector = Resources.FindObjectsOfTypeAll(inspectorWindowType).Where( o => o.GetType().GetField("m_PreviewWindow", getPreviewFlags) != null).ToArray()[0];
            
            //get the preview resizer on that window
            var previewResizer = inspector.GetType().GetField("m_PreviewResizer", getPreviewFlags).GetValue(inspector);
            
            object[] nullParams = new object[] {};
            var t = previewResizer.GetType();
            bool expanded = (bool)t.GetMethod("GetExpanded", getPreviewFlags).Invoke(previewResizer, nullParams);
            if (expanded != enabled) t.GetMethod("ToggleExpanded", getPreviewFlags).Invoke(previewResizer, nullParams);    
        }
        



    }

}
