using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace AssetObjectsPacks {



    //public static class HelpGUI {
        public class HelpWindow : EditorWindow {
            


            [MenuItem("Asset Objects Packs/Help")]
            public static void Init()
            {
                // Get existing open window or if none, make a new one:
                HelpWindow window = (HelpWindow)GetWindow(typeof(HelpWindow), true, "Help", true);
                window.Show(true);
                window.maxSize = sz;
                window.minSize = sz;
                window.position = new Rect(new Vector2(Screen.width*.5f - sz.x * .5f, Screen.height*.5f - sz.y), sz);
                //GetWindow<HelpWindow>("Help");
            }

            static readonly Vector2 sz = new Vector2(750,500);


            
            GUILayoutOption windowHeight = GUILayout.Height(sz.y - 60);



            string[][] helpTexts = new string[][] {
                EventHelp.helpTexts,
            };
            GUIContent[][] helpTabs = new GUIContent[][] {
                EventHelp.helpTabsGUI,
            };
            GUIContent[] outerTabs = new GUIContent[] {
                new GUIContent("Events"),
            };

            int innerTab, outerTab;
            
            void OnGUI()
            {
                GUIUtils.StartCustomEditor();
                GUIUtils.StartBox(0);            
                
                GUIUtils.Tabs(outerTabs, ref outerTab);
                GUIUtils.EndBox(0);

                GUIUtils.StartBox(0);
                EditorGUILayout.BeginHorizontal();
                GUIUtils.Tabs(helpTabs[outerTab], ref innerTab, GUIUtils.FitContent.Largest, true, TextAnchor.MiddleLeft);
                EditorGUILayout.TextArea(helpTexts[outerTab][innerTab], GUIStyles.helpBox, windowHeight);
                EditorGUILayout.EndHorizontal();
                GUIUtils.EndBox(0);

                bool changed = GUIUtils.EndCustomEditorWindow();
            }
            void OnLostFocus()
            {
                this.Close();
            }

            void OnDestroy()
            {
                Debug.Log("Destroyed...");
            }
        }
        
            
    //}
}