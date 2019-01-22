




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    public static class EditorColors {
        public static readonly Color32 red_color = new Color32(255,0,0,255);
        public static readonly Color32 blue_color = new Color32(100,100,225,255);
        public static readonly Color32 green_color = new Color32(0,255,0,255);
        public static readonly Color32 clear_color = new Color32(0,0,0,0);
        public static readonly Color32 white_color = new Color32(255,255,255,255);
        public static readonly Color32 selected_color = blue_color;
        public static readonly Color32 hidden_text_color = new Color32 (100,100,100,255);
        public static readonly Color32 selected_text_color = white_color;
    }
    public static class GUIUtils
    {
        public static readonly GUIContent blank_content = GUIContent.none;

         public static GUIStyle GetStyle(string styleName) {
            GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (guiStyle == null) {
                Debug.LogError((object) ("Missing built-in guistyle " + styleName));
            }
            return guiStyle;
        }

        static GUIStyle _tbs = null;
        static GUIStyle toolbar_button_style {
            get {
                if (_tbs == null) {
                    _tbs = new GUIStyle(EditorStyles.toolbarButton);
                }
                return _tbs;
            }
        }


        public static bool ScrollWindowElement (GUIContent gui_content, bool selected, bool hidden, bool directory) {
            Color32 orig_bg = GUI.backgroundColor;
            Color32 orig_text_color = scroll_window_element_style.normal.textColor;
            Texture2D orig_bg_tex = scroll_window_element_style.normal.background;
            FontStyle orig_font_style = scroll_window_element_style.fontStyle;
            
            if (selected) {
                GUI.backgroundColor = EditorColors.selected_color;
                scroll_window_element_style.normal.textColor = EditorColors.selected_text_color;
            }
            else {
                if (!directory) {
                    scroll_window_element_style.normal.background = null;
                }
                if (hidden) {
                    scroll_window_element_style.normal.textColor = EditorColors.hidden_text_color;
                }
            }

            if (hidden) {
                scroll_window_element_style.fontStyle = FontStyle.Italic;
            }

            bool clicked = GUILayout.Button(gui_content, scroll_window_element_style);
            if (selected) {
                GUI.backgroundColor = orig_bg;
                scroll_window_element_style.normal.textColor = orig_text_color;
            }
            else {
                if (!directory) {
                    scroll_window_element_style.normal.background = orig_bg_tex;
                }
                if (hidden) {
                scroll_window_element_style.normal.textColor = orig_text_color;
                }
            }
            if (hidden) {
                scroll_window_element_style.fontStyle = FontStyle.Normal;

            }
            return clicked;

        }
        static GUIStyle _swes = null;
        
        static GUIStyle scroll_window_element_style {
            get {
                if (_swes == null) {
                    _swes = new GUIStyle(EditorStyles.toolbarButton);
                    _swes.alignment = TextAnchor.MiddleLeft;
                }
                return _swes;
            }
        }




       
       

        public static int Tabs (GUIContent[] tab_contents, int current_index) {
            Color orig_bg = GUI.backgroundColor;
            Color32 orig_text_color = toolbar_button_style.normal.textColor;
            
            GUILayout.BeginHorizontal();
            int c = tab_contents.Length;
            for (int i = 0; i < c; i++) {
                bool drawing_cur_index = i == current_index;
                if (drawing_cur_index) {
                    GUI.backgroundColor = EditorColors.selected_color;
                    toolbar_button_style.normal.textColor = EditorColors.selected_text_color;
                }
                if (GUILayout.Button(tab_contents[i], toolbar_button_style)) current_index = i;
                if (drawing_cur_index) {
                    GUI.backgroundColor = orig_bg;
                    toolbar_button_style.normal.textColor = orig_text_color;
                }
            }
            GUILayout.EndHorizontal();
            return current_index;
        }
        static readonly GUILayoutOption[] little_button_layouts = new GUILayoutOption[] { GUILayout.Width(12), GUILayout.Height(12) };
        public static bool LittleButton (Color32 color, GUIContent content = null, GUIStyle st = null) {
            if (st == null) {
                st = GUI.skin.button;
            }
            if (content == null) {
                content = blank_content;
            }
            Color32 orig_bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool button_pressed = false;
            if (GUILayout.Button(content, st, little_button_layouts)){
                button_pressed = true;
            } 
            GUI.backgroundColor = orig_bg;       
            return button_pressed;     
        }

    }
}




