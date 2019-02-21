




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace AssetObjectsPacks {
    public static class EditorColors {

        public static readonly Color32 red = new Color32(225,25,25,255);
        public static readonly Color32 yellow = new Color32 (255, 155, 0, 255);
        public static readonly Color32 blue = new Color32(100,155,235,255);
        public static readonly Color32 green = new Color32(38,178,56,255);
        public static readonly Color32 clear = new Color32(0,0,0,0);
        public static readonly Color32 white = new Color32(255,255,255,255);
        public static readonly Color32 selected = blue;
        public static readonly Color32 black = new Color32 (0,0,0,255);
        public static readonly Color32 darkGray = new Color32(37,37,37,255);
        public static readonly Color32 medGray = new Color32(80,80,80,255);
        public static readonly Color32 liteGray = new Color32(175,175,175,255);
        public static Color32 ToggleBackgroundColor (bool selected) {
            return selected ? EditorColors.selected : liteGray;
        }
    }

    public static class GUIStyles {

        const int stylesCount = 8;
        static GUIStyle[] baseStyles = new GUIStyle[stylesCount];
        static GUIStyle ReturnOrBuild (int i, GUIStyle s) {
            if (baseStyles[i] == null) {
                baseStyles[i] = new GUIStyle(s);
                baseStyles[i].richText = true;
            }
            return baseStyles[i];
        }
        public static GUIStyle toolbarButton { get { return ReturnOrBuild(0, EditorStyles.toolbarButton); } }
        public static GUIStyle label { get { return ReturnOrBuild(1, EditorStyles.label); } }
        public static GUIStyle helpBox { get { return ReturnOrBuild(2, EditorStyles.helpBox); } }
        public static GUIStyle button { get { return ReturnOrBuild(3, GUI.skin.button); } }
        public static GUIStyle miniButton { get { return ReturnOrBuild(4, EditorStyles.miniButton); } }
        public static GUIStyle miniButtonLeft { get { return ReturnOrBuild(5, EditorStyles.miniButtonLeft); } }
        public static GUIStyle miniButtonRight { get { return ReturnOrBuild(6, EditorStyles.miniButtonRight); } }
        public static GUIStyle miniButtonMid { get { return ReturnOrBuild(7, EditorStyles.miniButtonMid); } }

    }
    public static class GUIUtils {        
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
        public static void Label (GUIContent c, bool fit_content) {
            Label(c, fit_content, EditorColors.liteGray);
        }
        public static void Label (GUIContent c, GUILayoutOption option) {
            Label(c, EditorColors.liteGray, option);
        }
        public static void Label (GUIContent c, bool fit_content, Color32 text_color) {
            Label(c, text_color, fit_content ? GUILayout.Width(GUIStyles.label.CalcSize(c).x) : GUILayout.ExpandWidth(true));
        }
        public static void Label (GUIContent c, Color32 text_color, GUILayoutOption option) {
            DoWithinColor (GUIStyles.label, text_color, () => { EditorGUILayout.LabelField(c, GUIStyles.label, option); } );
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
        public static void StartBox (int space, Color32 color, GUILayoutOption option) {
            DoWithinColor(color, () => { EditorGUILayout.BeginVertical(GUI.skin.box, option); } );
            Space(space);
        }
        public static void StartBox (int space, Color32 color) {
            DoWithinColor(color, () => { EditorGUILayout.BeginVertical(GUI.skin.box); } );
            Space(space);
        }
        public static void StartBox (int space) {
            StartBox(space, EditorColors.medGray);
        }
        public static void StartBox (int space, GUILayoutOption option) {
            StartBox(space, EditorColors.medGray, option);
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
        public static void EndBox (int space) {
            Space(space);
            EditorGUILayout.EndVertical();
        }
        public static void StartCustomEditor () {
            EditorGUI.BeginChangeCheck();
            DoWithinColor(EditorColors.darkGray, () => { EditorGUILayout.BeginVertical(GUI.skin.window); } );   
        }
        public static void EndCustomEditor (Editor editor, bool forceChange) {
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck() || forceChange) {                
                EditorUtility.SetDirty(editor.target);
                editor.serializedObject.ApplyModifiedProperties();
            }
        }

        public static void StartCustomEditorWindow () {
            EditorGUI.BeginChangeCheck();
            DoWithinColor(EditorColors.darkGray, () => { EditorGUILayout.BeginVertical(GUI.skin.window); } );   
        }
        public static bool EndCustomEditorWindow () {
            EditorGUILayout.EndVertical();
            return EditorGUI.EndChangeCheck();
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
            changed = Button(c, fit_content, s, EditorColors.ToggleBackgroundColor(value), EditorColors.black);
            return changed ? !value : value;
        }
        public static bool Button (GUIContent c, bool fit_content, GUIStyle s) {
            return Button(c, fit_content, s, EditorColors.liteGray, EditorColors.black );
        }
        public static bool Button (GUIContent c, bool fit_content, GUIStyle s, Color32 color, Color32 text_color) {
            return Button(c, s, color, text_color, new GUILayoutOption[]  { fit_content ? c.CalcWidth(s) : GUILayout.ExpandWidth(true) } );
        }
        public static bool SmallButton (GUIContent c, Color32 color, Color32 text_color) {
            return Button(c, small_button_style, color, text_color, smallButtonOpts );
        }
        public static bool SmallButton (GUIContent c) {
            return Button(c, small_button_style, EditorColors.liteGray, EditorColors.black, smallButtonOpts );
        }
        public static bool SmallToggleButton (GUIContent c, bool value, out bool changed) {
            changed = SmallButton(c, EditorColors.ToggleBackgroundColor(value), EditorColors.black);
            return changed ? !value : value;
        }

        static readonly GUILayoutOption[] smallButtonOpts = new GUILayoutOption[] { GUILayout.Width(16), GUILayout.Height(13) };
        static GUIStyle _sbs = null;
        static GUIStyle small_button_style {
            get {
                if (_sbs == null) {
                    _sbs = new GUIStyle(GUIStyles.miniButton);
                    _sbs.fontSize = 7;
                }
                return _sbs;
            }
        }
        public static void SmallButtonClear () {
            SmallButton(blank_content, EditorColors.clear, EditorColors.clear);
        }


        public enum FitContent {
            Largest,
            False,
            True,
        };
        public static bool Tabs (GUIContent[] guis, ref int current, FitContent fitContent=FitContent.False, bool vertical=false, TextAnchor alignment = TextAnchor.MiddleCenter) {
            int c = guis.Length;
            bool changed = false;

            GUIStyle s = GUIStyles.toolbarButton;
            TextAnchor a = s.alignment;
            s.alignment = alignment;


            GUILayoutOption largestWidth = null;
            if (fitContent == FitContent.Largest) {
                float max = -1;
                for (int i = 0; i < c; i++) {
                    float w = s.CalcSize(guis[i]).x;
                    if (w > max) max = w;
                }
                largestWidth = GUILayout.Width(max);
            }


            if (vertical) {
                if (fitContent == FitContent.Largest) {
                    EditorGUILayout.BeginVertical(largestWidth);
                }
                else {
                    EditorGUILayout.BeginVertical();
                }
                Space(1);

            }
            else {
                EditorGUILayout.BeginHorizontal();
            }


            for (int i = 0; i < c; i++) {
                bool selected = i == current;
                bool pressed = false;

                GUILayoutOption o = null;
                switch (fitContent) {
                    case FitContent.False:
                        o = GUILayout.ExpandWidth(true);
                        break;
                    case FitContent.True:
                        o = guis[i].CalcWidth(s);
                        break;
                    case FitContent.Largest:
                        o = largestWidth;
                        break;
                }

                DoWithinColor(EditorColors.ToggleBackgroundColor(selected), () => { pressed = GUILayout.Button(guis[i], s, o); } );
                if (pressed) {
                    current = i;
                    changed = true;
                }
            }
            s.alignment = a;
            if (vertical) {
                EditorGUILayout.EndVertical();
            }
            else {
                EditorGUILayout.EndHorizontal();
            }
            return changed;
        }
            
        public static readonly GUIContent blank_content = GUIContent.none;

        public static GUILayoutOption CalcWidth(this GUIContent c, GUIStyle s) {
            return GUILayout.Width(s.CalcSize(c).x);
        }
        public static void ShowPopUpAtMouse(PopupList.InputData inputData) {
            PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), new PopupList(inputData));
        }
    }
}




