




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace AssetObjectsPacks {
    public static class EditorColors {

        public static readonly Color32 red_color = new Color32(225,25,25,255);
        public static readonly Color32 yellow_color = new Color32 (255, 155, 0, 255);
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
        public static void DrawProp (EditorProp prop) {
            EditorGUILayout.PropertyField(prop.prop);
        }
        public static void DrawProp (EditorProp prop, GUIContent content) {
            EditorGUILayout.PropertyField(prop.prop, content);
        }        
        public static void DrawProp (EditorProp prop, GUIContent content, GUILayoutOption[] options) {
            EditorGUILayout.PropertyField(prop.prop, content, options);
        }
        public static void DrawProp (EditorProp prop, GUIContent content, GUILayoutOption options) {
            EditorGUILayout.PropertyField(prop.prop, content, options);
        }
        const string sArraySize = "Array.size";
        public static void DrawArrayProp (EditorProp array, ref bool show) {
            show = EditorGUILayout.Foldout(show, array.prop.displayName, true);
            if (!show) return;
            EditorGUI.indentLevel++;
            DrawProp(array[sArraySize]);
            int c = array[sArraySize].intValue;
            for (int i = 0; i < c; i++) DrawProp (array[i]);
            EditorGUI.indentLevel--;
        }

        public static bool DrawDelayedTextProp (EditorProp prop, GUIContent content, GUILayoutOption option) {
            EditorGUILayout.BeginHorizontal();
            Label(content, option);
            bool changed = DrawDelayedTextProp(prop);
            EditorGUILayout.EndHorizontal();
            return changed;
        }
        public static bool DrawDelayedTextProp (EditorProp prop) {
            string old_val = prop.stringValue;
            string new_val = EditorGUILayout.DelayedTextField(blank_content, old_val);
            bool changed = new_val != old_val;
            if (changed) prop.SetValue( new_val );
            return changed;
        }
            


        
        const string overrideKeyboardControlName = "overrideKeyboard";
        public static bool KeyboardOverriden () {
            return GUI.GetNameOfFocusedControl() == overrideKeyboardControlName;
        }
        public static void NextControlOverridesKeyboard () {
            // Set the internal name of the textfield
            GUI.SetNextControlName(overrideKeyboardControlName);
        }
        public static void CheckLoseFocusLastRect () {
            //if clicked outside, lose focus
            bool lastRectClicked;
            if (ClickHappened(out lastRectClicked)) {
                if (!lastRectClicked) EditorGUI.FocusTextInControl("");
            }
        }
        static bool ClickHappened (out bool lastElementClicked) {
            Event e = Event.current;
            bool click_happened = e.type == EventType.MouseDown && e.button == 0;            
            lastElementClicked = false;
            if (click_happened) lastElementClicked = GUILayoutUtility.GetLastRect().Contains(e.mousePosition);
            return click_happened;
        }


        public static void Label(GUIContent c, bool fit_content) {
            Label(c, fit_content, EditorColors.light_gray);
        }
        public static void Label(GUIContent c, GUILayoutOption option) {
            Label(c, EditorColors.light_gray, option);
        }
        public static void Label (GUIContent c, bool fit_content, Color32 text_color) {
            Label(c, text_color, fit_content ? GUILayout.Width(EditorStyles.label.CalcSize(c).x) : GUILayout.ExpandWidth(true));
        }
        public static void Label (GUIContent c, Color32 text_color, GUILayoutOption option) {
            DoWithinColor (EditorStyles.label, text_color, () => { EditorGUILayout.LabelField(c, option); } );
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
        public static void StartBox (int space, Color32 color) {
            DoWithinColor(color, () => { EditorGUILayout.BeginVertical(GUI.skin.box); } );
            Space(space);
        }
        public static void StartBox (int space) {
            StartBox(space, EditorColors.med_gray);
        }
/*
*/
        public static void BeginIndent (int indent_space = 1) {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < indent_space; i++) SmallButtonClear();
            EditorGUILayout.BeginVertical();
        }
        public static void EndIndent () {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void EndBox (int space) {
            Space(space);
            EditorGUILayout.EndVertical();
        }
/*
*/




        public static void CustomEditor (Editor editor, Action fn) {
            EditorGUI.BeginChangeCheck();
            DoWithinColor(EditorColors.dark_color, () => { EditorGUILayout.BeginVertical(GUI.skin.window); } );
            fn();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck()) {                
                EditorUtility.SetDirty(editor.target);
                editor.serializedObject.ApplyModifiedProperties();
            }
        }
        
        
        public static bool Button (GUIContent c, GUIStyle s, Color32 color, Color32 text_color, GUILayoutOption[] options) {
            bool r = false;
            bool origRT = s.richText;
            s.richText = true;
            DoWithinColor(color, s, text_color, () => { r = GUILayout.Button(c, s, options); } );
            s.richText = origRT;
            return r;
        }
        public static bool Button (GUIContent c, GUIStyle s, Color32 color, Color32 text_color, GUILayoutOption option) {
            return Button(c, s, color, text_color, new GUILayoutOption[] { option } );
        }
        
        public static bool ToggleButton (GUIContent c, bool fit_content, bool value, GUIStyle s, out bool changed) {
            changed = Button(c, fit_content, s, EditorColors.ToggleBackgroundColor(value), EditorColors.ToggleTextColor(value));
            return changed ? !value : value;
        }
        public static bool Button (GUIContent c, bool fit_content, GUIStyle s) {
            return Button(c, fit_content, s, EditorColors.light_gray, EditorColors.black_color );
        }
        public static bool Button (GUIContent c, bool fit_content, GUIStyle s, Color32 color, Color32 text_color) {
            return Button(c, s, color, text_color, new GUILayoutOption[]  { fit_content ? c.CalcWidth(s) : GUILayout.ExpandWidth(true) } );
        }
        public static bool SmallButton (GUIContent c, Color32 color, Color32 text_color) {
            return Button(c, small_button_style, color, text_color, smallButtonOpts );
        }
        public static bool SmallButton (GUIContent c) {
            return Button(c, small_button_style, EditorColors.light_gray, EditorColors.black_color, smallButtonOpts );
        }
        
        public static bool SmallToggleButton (GUIContent c, bool value, out bool changed) {
            changed = SmallButton(c, EditorColors.ToggleBackgroundColor(value), EditorColors.ToggleTextColor(value));
            return changed ? !value : value;
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

        



        public static bool Tabs (GUIContent[] tab_contents, ref int current, bool fit_content=false) {
            int c = tab_contents.Length;
            int orig_index = current;
            EditorGUILayout.BeginHorizontal();

            GUIStyle s = EditorStyles.toolbarButton;
            for (int i = 0; i < c; i++) {
                bool selected = i == current;
                bool pressed = false;
                DoWithinColor(
                    EditorColors.ToggleBackgroundColor(selected), 
                    s, EditorColors.ToggleTextColor(selected), 
                    () => { 
                        pressed = GUILayout.Button(tab_contents[i], s, fit_content ? tab_contents[i].CalcWidth(s) : GUILayout.ExpandWidth(true) );
                    } 
                );
                if (pressed) current = i;
            }
            EditorGUILayout.EndHorizontal();
            return orig_index != current;
        }
            

        public static readonly GUIContent blank_content = GUIContent.none;

        public static bool ScrollWindowElement (GUIContent gui_content, bool selected, bool hidden, bool directory, GUILayoutOption width) {
            scroll_window_element_style.fontStyle = hidden ? FontStyle.Italic : FontStyle.Normal;
            scroll_window_element_style.normal.background = (!selected && !directory) ? null : orig_scroll_window_background;
            bool clicked = false;
            string t = gui_content.text;
            if (hidden) gui_content.text = t + hiddenSuffix;

            clicked = Button(
                gui_content, scroll_window_element_style, 
                EditorColors.ToggleBackgroundColor(selected),
                selected ? EditorColors.selected_text_color : ( directory ? EditorColors.black_color : EditorColors.light_gray),
                width
            );
                
            if (hidden) gui_content.text = t;
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
        public static GUILayoutOption CalcWidth(this GUIContent c, GUIStyle s) {
            return GUILayout.Width(s.CalcSize(c).x);
        }
        public static void ShowPopUpAtMouse(PopupList.InputData popup_input) {
            PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), new PopupList(popup_input));
        }
    }
}




