




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace AssetObjectsPacks {
    public static class EditorColors {

        public static readonly Color32 red_color = new Color32(225,25,25,255);
        public static readonly Color32 blue_color = new Color32(100,155,235,255);
        public static readonly Color32 green_color = new Color32(38,178,56,255);
        public static readonly Color32 clear_color = new Color32(0,0,0,0);
        public static readonly Color32 white_color = new Color32(255,255,255,255);
        public static readonly Color32 selected_color = blue_color;
        //public static readonly Color32 hidden_text_color = new Color32 (100,100,100,255);
        public static readonly Color32 black_color = new Color32 (0,0,0,255);
        public static readonly Color32 dark_color = new Color32(37,37,37,255);
        public static readonly Color32 med_gray = new Color32(80,80,80,255);
        public static readonly Color32 light_gray = new Color32(175,175,175,255);


        public static readonly Color32 selected_text_color = black_color;//new Color32 (100,100,100,255);


        public static Color32 ToggleBackgroundColor (bool selected) {
            return selected ? selected_color : light_gray;
        }
        public static Color32 ToggleTextColor (bool selected) {
            return selected ? selected_text_color : black_color;
        }


    }
    public static class GUIUtils
    {
        public static void Label(GUIContent c, bool fit_content) {
            Label(c, fit_content, EditorColors.light_gray);
        }
        public static void Label (GUIContent c, bool fit_content, Color32 text_color) {

            GUIStyle s = EditorStyles.label;
            DoWithinColor (s, text_color, 
                () => {
                    EditorGUILayout.LabelField(c, fit_content ? GUILayout.Width(s.CalcSize(c).x) : GUILayout.ExpandWidth(true) );
                } );
        }


        public static void Space(int count = 1) {
            for (int i = 0; i < count; i++) EditorGUILayout.Space();
        }

        static void DoWithinColor(GUIStyle style, Color32 text_color, Action fn) {
            Color32 orig_txt = style.normal.textColor;
            style.normal.textColor = text_color;
            fn();
            style.normal.textColor = orig_txt;
        }
        

        static void DoWithinColor(Color32 color, GUIStyle style, Color32 text_color, Action fn) {
            Color32 orig_bg = GUI.backgroundColor;
            Color32 orig_txt = style.normal.textColor;
            style.normal.textColor = text_color;
            GUI.backgroundColor = color;
            fn();
            GUI.backgroundColor = orig_bg;
            style.normal.textColor = orig_txt;
        }
        
        
        
        static void DoWithinColor(Color32 color, Action fn) {
            Color32 orig_bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            fn();
            GUI.backgroundColor = orig_bg;
        }
        public static void StartBox (Color32 color) {
            DoWithinColor(color, () => { EditorGUILayout.BeginVertical(GUI.skin.box); } );
        }
        public static void StartBox () {
            StartBox(EditorColors.med_gray);
        }
        public static void BeginIndent (int indent_space = 1) {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < indent_space; i++) SmallButtonClear();
            EditorGUILayout.BeginVertical();
        }
        public static void EndIndent () {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        
        public static void EndBox () {
            EditorGUILayout.EndVertical();
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
/*
        static GUIStyle _bs = null;
        static GUIStyle button_style {
            get {
                if (_bs == null) _bs = new GUIStyle( GUI.skin.button);
                return _bs;
            }
        }
 */
        public static bool ToggleButton (GUIContent content, bool fit_content, bool value, GUIStyle style) {
            return Button(content, fit_content, style, EditorColors.ToggleBackgroundColor(value), EditorColors.ToggleTextColor(value)) ? !value : value;
        }
        public static bool Button (GUIContent content, bool fit_content, GUIStyle style) {
            return Button(content, fit_content, style, EditorColors.light_gray, EditorColors.black_color );
        }




        public static bool Button (GUIContent content, bool fit_content, GUIStyle style, Color32 color, Color32 text_color) {
            GUILayoutOption[] options = new GUILayoutOption[0];
            if (fit_content) options = new GUILayoutOption[] { GUILayout.Width( style.CalcSize( content ).x ) };
            
            //Color orig_tex_color = style.normal.textColor;
            //style.normal.textColor = text_color;
            
            bool button_clicked = false;

            DoWithinColor(color, style, text_color, () => { button_clicked = GUILayout.Button(content, style, options); } );

            //style.normal.textColor = orig_tex_color;



            return button_clicked;
        }

        static readonly GUILayoutOption[] smallButtonOpts = new GUILayoutOption[] { GUILayout.Width(16), GUILayout.Height(13) };
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
        public static void SmallButtonClear () {
            SmallButton(blank_content, EditorColors.clear_color, EditorColors.clear_color);
        }

        public static bool SmallButton (GUIContent content, Color32 color, Color32 text_color) {
            if (content == null) content = blank_content;
            bool button_clicked = false;
            //small_button_style.normal.textColor = text_color; 
            DoWithinColor(color, small_button_style, text_color, () => { button_clicked = GUILayout.Button(content, small_button_style, smallButtonOpts); });
            return button_clicked;
        }
        public static bool SmallToggleButton (GUIContent content, bool value) {
            return SmallButton(content, EditorColors.ToggleBackgroundColor(value), EditorColors.ToggleTextColor(value)) ? !value : value;
        }



        public static int Tabs (GUIContent[] tab_contents, int current, out bool was_changed, bool fit_content=false) {
            int c = tab_contents.Length;
            int orig_index = current;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < c; i++) {
                bool selected = i == current;
                //toolbar_button_style.normal.textColor = EditorColors.ToggleTextColor(selected); 
                DoWithinColor(EditorColors.ToggleBackgroundColor(selected), EditorStyles.toolbarButton, EditorColors.ToggleTextColor(selected), () => { if (GUILayout.Button(tab_contents[i], EditorStyles.toolbarButton, fit_content ? tab_contents[i].CalcWidth() : GUILayout.ExpandWidth(true) )) current = i; } );
            }
            EditorGUILayout.EndHorizontal();
            was_changed = orig_index != current;
            return current;
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
        //static GUIStyle toolbar_button_style {
        //    get {
        //        if (_tbs == null) _tbs = new GUIStyle(EditorStyles.toolbarButton);
        //        return _tbs;
        //    }
        //}


        public static bool ScrollWindowElement (GUIContent gui_content, bool selected, bool hidden, bool directory, GUILayoutOption width) {
            scroll_window_element_style.fontStyle = hidden ? FontStyle.Italic : FontStyle.Normal;
            scroll_window_element_style.normal.background = (!selected && !directory) ? null : orig_scroll_window_background;
            //scroll_window_element_style.normal.textColor = selected ? EditorColors.selected_text_color : ( directory ? EditorColors.black_color : EditorColors.light_gray);
            
            bool clicked = false;
            DoWithinColor (
                EditorColors.ToggleBackgroundColor(selected),
                scroll_window_element_style,
                selected ? EditorColors.selected_text_color : ( directory ? EditorColors.black_color : EditorColors.light_gray),
            
                () => {
                    string t = gui_content.text;
                    if (hidden) gui_content.text = t + hiddenSuffix;
                    clicked = GUILayout.Button(gui_content, scroll_window_element_style, width);
                    if (hidden) gui_content.text = t;
                }
            );
            return clicked;

        }
        const string hiddenSuffix = " [HIDDEN]";

        static GUIStyle _swes = null;
        static Texture2D orig_scroll_window_background;
        static GUIStyle scroll_window_element_style {
            get {
                if (_swes == null) {
                    _swes = new GUIStyle(EditorStyles.toolbarButton);
                    _swes.alignment = TextAnchor.MiddleLeft;
                    orig_scroll_window_background = _swes.normal.background;
                }
                return _swes;
            }
        }
        public static GUILayoutOption CalcWidth(this GUIContent c) {
            return GUILayout.Width(EditorStyles.label.CalcSize(c).x);
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

        public static void ArrayGUI(SerializedProperty property, ref bool fold) {
            fold = EditorGUILayout.Foldout(fold, property.displayName, true);
            if (!fold) return;
            EditorGUI.indentLevel++;
            SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
            EditorGUILayout.PropertyField(arraySizeProp);
            int c = arraySizeProp.intValue;
            for (int i = 0; i < c; i++) EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
            EditorGUI.indentLevel--;
        }




    }
}




