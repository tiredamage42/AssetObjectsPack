




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    public static class EditorColors {
        public static readonly Color32 red_color = new Color32(225,25,25,255);
        public static readonly Color32 blue_color = new Color32(100,125,225,255);
        public static readonly Color32 green_color = new Color32(38,178,56,255);
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


        public static bool ScrollWindowElement (GUIContent gui_content, bool selected, bool hidden, bool directory, GUILayoutOption width=null) {
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
            bool clicked = false;

            if (width != null) {
                clicked = GUILayout.Button(gui_content, scroll_window_element_style, width);

            }
            else {
                clicked = GUILayout.Button(gui_content, scroll_window_element_style);
    
            }

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
        public static GUILayoutOption CalcWidth(this GUIContent c) {
            return GUILayout.Width(EditorStyles.label.CalcSize(c).x);
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

        static void AdjustRectToMousePosition(ref Rect rect) {
            Vector2 mousePos = Event.current.mousePosition;
            rect.x = mousePos.x;
            rect.y = mousePos.y;
        }

        public static void ShowPopUpAtMouse(PopupList.InputData popup_input, bool draw_search, bool draw_remove) {
            Rect rect = new Rect();
            AdjustRectToMousePosition(ref rect);
            PopupWindow.Show(rect, new PopupList(popup_input, draw_search, draw_remove));
        }



        public static void ArrayGUI(SerializedProperty property, ref bool fold)
        {
            fold = EditorGUILayout.Foldout(fold, property.displayName);
            if (fold)
            {
    
                EditorGUI.indentLevel++;
    
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp);
                for (int i = 0; i < arraySizeProp.intValue; i++)
                {                
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
                }
                EditorGUI.indentLevel--;
            }
        }




        public class Pagination {
            public int cur_page;
            public bool NextPage(int all_count, int per_page) {
                int max_pages = all_count / per_page;
                if (all_count % per_page != 0) max_pages++;
                return NextPage_(max_pages);
            }
            bool NextPage_ (int max_pages) {
                if (cur_page+1 < max_pages) {
                    cur_page++;
                    return true;
                }
                return false;
            }
            public bool PreviousPage () {
                Debug.Log("gui previous: " + cur_page);
                
                if (cur_page-1 >= 0) {
                    Debug.Log("gui previous ok");
                
                    cur_page--;
                    return true;
                }
                return false;
            }
            public void GetIndexRange(out int min, out int max, int per_page, int all_count) {
                min = cur_page * per_page;
                max = Mathf.Min(min + per_page, all_count - 1);
            }

            bool gui_init;
            GUIStyle button_s, label_s;
            GUIContent back_gui = new GUIContent("<<"), fwd_gui = new GUIContent(">>");
            GUIContent show_page_gui;

            void InitGUI () {
                if (gui_init) return;
                gui_init = true;
                button_s = EditorStyles.toolbarButton;
                label_s = new GUIStyle(EditorStyles.label);
                label_s.alignment = TextAnchor.MiddleCenter;   
            }

            public bool ChangePageGUI (int all_count, int per_page) {
                InitGUI();
                bool changed_page = false;
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(back_gui, button_s)) changed_page = PreviousPage();

                int max_pages = all_count / per_page;
                if (all_count % per_page != 0) max_pages++;
                EditorGUILayout.LabelField("Page: " + (cur_page + 1) + " / " + max_pages, label_s);
                
                if (GUILayout.Button(fwd_gui, button_s)) changed_page = NextPage_(max_pages);
                
                EditorGUILayout.EndHorizontal();
                return changed_page;
            }
        }

    }
}




