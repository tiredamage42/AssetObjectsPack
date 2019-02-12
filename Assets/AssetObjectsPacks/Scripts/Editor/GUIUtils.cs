




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace AssetObjectsPacks {
    public static class EditorColors {

        public static readonly Color32 red_color = new Color32(225,25,25,255);
        public static readonly Color32 blue_color = new Color32(100,125,225,255);
        public static readonly Color32 green_color = new Color32(38,178,56,255);
        public static readonly Color32 clear_color = new Color32(0,0,0,0);
        public static readonly Color32 white_color = new Color32(255,255,255,255);
        public static readonly Color32 selected_color = blue_color;
        public static readonly Color32 hidden_text_color = new Color32 (100,100,100,255);
        public static readonly Color32 dark_color = new Color32(37,37,37,255);
        public static readonly Color32 med_gray = new Color32(100,100,100,255);
        public static readonly Color32 light_gray = new Color32(175,175,175,255);


        public static readonly Color32 selected_text_color = white_color;
        public static readonly Color32 black_color = new Color32 (0,0,0,255);

    }
    public static class GUIUtils
    {


        public static void Space(int count = 1) {
            for (int i = 0; i < count; i++) EditorGUILayout.Space();
        }
        static void DoWithinColor(Color32 color, Action fn) {
            Color32 orig_bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            fn();
            GUI.backgroundColor = orig_bg;
        }
        public static void StartCustomEditor () {
            EditorGUI.BeginChangeCheck();
            DoWithinColor(EditorColors.dark_color, () => { EditorGUILayout.BeginVertical(GUI.skin.window); } );
        }
        public static void EndCustomEditor(Editor editor, bool force_change) {
            EditorGUILayout.EndVertical();
            
            if (EditorGUI.EndChangeCheck() || force_change) {                
                EditorUtility.SetDirty(editor.target);
                editor.serializedObject.ApplyModifiedProperties();
            }            
        }

        static GUIStyle _bs = null;
        static GUIStyle button_style {
            get {
                if (_bs == null) _bs = new GUIStyle( GUI.skin.button);
                return _bs;
            }
        }
        public static bool Button (GUIContent content, bool fit_content) {
            return Button(content, fit_content, EditorColors.light_gray, EditorColors.black_color );
        }
        public static bool Button (GUIContent content, bool fit_content, Color32 color, Color32 text_color) {
            GUILayoutOption[] options = new GUILayoutOption[0];
            if (fit_content) options = new GUILayoutOption[] { GUILayout.Width( button_style.CalcSize( content ).x ) };
            
            button_style.normal.textColor = text_color;
            bool button_clicked = false;
            DoWithinColor(color, () => { button_clicked = GUILayout.Button(content, options); } );
            return button_clicked;
        }

        public static int Tabs (GUIContent[] tab_contents, int current_index, out bool was_changed) {
            int c = tab_contents.Length;
            int orig_index = current_index;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < c; i++) {
                bool drawing_cur_index = i == current_index;
                Color32 col = drawing_cur_index ? EditorColors.selected_color : EditorColors.light_gray;
                toolbar_button_style.normal.textColor = drawing_cur_index ? EditorColors.selected_text_color : EditorColors.black_color;
                DoWithinColor(col, () => { if (GUILayout.Button(tab_contents[i], toolbar_button_style)) current_index = i; } );
            }
            EditorGUILayout.EndHorizontal();
            was_changed = orig_index != current_index;
            return current_index;
        }
            

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
                if (_tbs == null) _tbs = new GUIStyle(EditorStyles.toolbarButton);
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




       
       

        
        static readonly GUILayoutOption[] small_button_layouts = new GUILayoutOption[] { GUILayout.Width(16), GUILayout.Height(13) };
        //static readonly GUILayoutOption[] little_button_layouts = new GUILayoutOption[] { GUILayout.Width(17.5f), GUILayout.Height(13.0f) };
        
        static GUIStyle _sbs = null;
        static GUIStyle small_button_style {
            get {
                if (_sbs == null) {
                    _sbs = new GUIStyle(EditorStyles.miniButton);
                    _sbs.fontSize = 7;
                }
                return _sbs;
            }
        }

        
        public static bool SmallButton (Color32 color, Color32 text_color, GUIContent content = null) {
            if (content == null) content = blank_content;
            Color32 orig_bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            small_button_style.normal.textColor = text_color;
            bool button_pressed = GUILayout.Button(content, small_button_style, small_button_layouts);
            GUI.backgroundColor = orig_bg;       
            return button_pressed;     
        }
        
        public static bool ToggleButton (bool value, GUIContent content = null) {
            if (content == null) content = blank_content;
            
            //GUIStyle s = new GUIStyle(EditorStyles.miniButton);
            //s.fontSize = 8;

            //GUIContent c = new GUIContent( (is_open ? "V" : ">") + " " + label);
            //GUIContent c = new GUIContent( label);
            
            //GUILayoutOption w = GUILayout.Width(s.CalcSize(c).x);

            //Debug.Log(s.CalcSize(c));


           
            //bool clicked = GUILayout.Button(c, s, w);
            //GUI.backgroundColor = orig_color;


            Color32 orig_bg = GUI.backgroundColor;
            Color32 orig_text_color = small_button_style.normal.textColor;
            GUI.backgroundColor = value ? EditorColors.selected_color : EditorColors.white_color;
            small_button_style.normal.textColor = value ? EditorColors.selected_text_color : EditorColors.black_color;
            //GUI.backgroundColor = color;
            
            bool button_pressed = GUILayout.Button(content, small_button_style, small_button_layouts);
            
            GUI.backgroundColor = orig_bg;    
            //small_button_style.normal.textColor = orig_text_color;   
            

            return button_pressed ? !value : value;

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




