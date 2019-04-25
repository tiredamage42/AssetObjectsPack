using UnityEngine;
using UnityEditor;
using System;

namespace AssetObjectsPacks {
    public static class Colors {

        public static readonly Color32 red = new Color32(225,25,25,255);
        public static readonly Color32 yellow = new Color32 (255, 155, 0, 255);
        public static readonly Color32 blue = new Color32(90,140,225,255);
        public static readonly Color32 green = new Color32(38,178,56,255);
        public static readonly Color32 clear = new Color32(0,0,0,0);
        public static readonly Color32 white = new Color32(255,255,255,255);
        public static readonly Color32 selected = blue;
        public static readonly Color32 black = new Color32 (0,0,0,255);
        public static readonly Color32 darkGray = new Color32(37,37,37,255);
        public static readonly Color32 medGray = new Color32(70,70,70,255);
        
        public static readonly Color32 liteGray = new Color32(190,190,190,255);
        public static readonly Color32 medliteGray = new Color32(110,110,110,255);

        public static Color32 Toggle (bool isSelected) {
            return isSelected ? selected : medliteGray;
        }
    }

    public static class GUIStyles {

        public static GUISkin s_DarkSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
/*
            public static GUIStyle label = GetStyle("ControlLabel");
            public static GUIStyle popup = GetStyle("MiniPopup");
            public static GUIStyle textField = GetStyle("textField");

            public static Color cursorColor = s_DarkSkin.settings.cursorColor;

            private static GUIStyle GetStyle(string name)
            {
                return new GUIStyle(s_DarkSkin.GetStyle(name));
            }
 */

        const int stylesCount = 11;
        static GUIStyle[] baseStyles = new GUIStyle[stylesCount];
        static GUIStyle ReturnOrBuild (int i, GUIStyle s) {
            if (baseStyles[i] == null) {
                baseStyles[i] = new GUIStyle(s);                
                baseStyles[i].richText = true;
            }
            return baseStyles[i];
        }
        public static GUIStyle window { get { return ReturnOrBuild(9, GUI.skin.window); } }
        public static GUIStyle box { get { return ReturnOrBuild(8, GUI.skin.box); } }
        public static GUIStyle toolbarButton { get { return ReturnOrBuild(0, EditorStyles.toolbarButton); } }
        public static GUIStyle label { get { return ReturnOrBuild(1, EditorStyles.label); } }
        public static GUIStyle helpBox { get { return ReturnOrBuild(2, EditorStyles.helpBox); } }
        public static GUIStyle button { get { return ReturnOrBuild(3, GUI.skin.button); } }
        public static GUIStyle miniButton { get { return ReturnOrBuild(4, EditorStyles.miniButton); } }
        public static GUIStyle miniButtonLeft { get { return ReturnOrBuild(5, EditorStyles.miniButtonLeft); } }
        public static GUIStyle miniButtonRight { get { return ReturnOrBuild(6, EditorStyles.miniButtonRight); } }
        public static GUIStyle miniButtonMid { get { return ReturnOrBuild(7, EditorStyles.miniButtonMid); } }
        public static GUIStyle popup {
            get {
                int i = 10;
                if (baseStyles[i] == null) {
                    baseStyles[i] = new GUIStyle(s_DarkSkin.GetStyle("MiniPopup"));
                    baseStyles[i].richText = true;
                }
                return baseStyles[i];
        
            }
        }
    }
    public static class GUIUtils {      


        public static void DrawEnumProp(EditorProp prop, Func<int, Enum> intToEnum, Func<Enum, int> enumToInt, params GUILayoutOption[] options) {
            GUI.backgroundColor = Color.white;
            if (prop.property.propertyType == SerializedPropertyType.Enum) {
                prop.property.enumValueIndex = enumToInt(EditorGUILayout.EnumPopup(intToEnum(prop.property.enumValueIndex), GUIStyles.popup, options));
            }
            else {
                prop.property.intValue = enumToInt(EditorGUILayout.EnumPopup(intToEnum(prop.property.intValue), GUIStyles.popup, options));
            }
        }
        public static void DrawEnumProp (EditorProp prop, GUIContent gui, Func<int, Enum> intToEnum, Func<Enum, int> enumToInt, params GUILayoutOption[] options) {
            EditorGUILayout.BeginHorizontal();
            Label(gui, options);
            DrawEnumProp(prop, intToEnum, enumToInt);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawToggleProp(EditorProp prop) {
            prop.property.boolValue = DrawToggle(prop.property.boolValue);
        }
        public static bool DrawToggle(bool value) {
            return SmallToggleButton(GUIContent.none, value, out _);
        }
        
        public static void DrawToggleProp (EditorProp prop, GUIContent gui, params GUILayoutOption[] options) {
            prop.property.boolValue = DrawToggle(prop.property.boolValue, gui, options);
        }

        public static bool DrawToggle (bool value, GUIContent gui, params GUILayoutOption[] options) {
            EditorGUILayout.BeginHorizontal();
            Label(gui, options);//GUILayout.MaxWidth(EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth));// GUILayout.Width(EditorGUIUtility.labelWidth * 2));// options);
            
            //for (int i = 0; i < EditorGUI.indentLevel + 1; i++) {
            //    SmallButtonClear();
            //}
            bool val = DrawToggle(value);
            EditorGUILayout.EndHorizontal();
            return val;    
        }
        
        

        
        public static void DrawProp (EditorProp prop, params GUILayoutOption[] options) {    
            GUI.backgroundColor = Color.white;
            NameNextControl("prop");
            EditorGUILayout.PropertyField(prop.property, GUIContent.none, options);

            if (IsFocused("prop")) {
                UnityEngine.Event e = UnityEngine.Event.current;
                bool clicked = (e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == 0;         
                if (clicked && !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)) {
                    FocusOnTextArea("");
                }   
            }
            
            
            
        }

        public static void DrawProp (EditorProp prop, GUIContent gui, params GUILayoutOption[] options) {
            EditorGUILayout.BeginHorizontal();
            Label(gui, options);
            DrawProp(prop);
            EditorGUILayout.EndHorizontal();
        }
        
        static bool showArray = true;
        public static void DrawObjArrayProp<T> (EditorProp array) where T : UnityEngine.Object {
            
            if (showArray) {
                StartBox(Colors.darkGray);

                //label and show
                EditorGUILayout.BeginHorizontal();
                showArray = SmallToggleButton(GUIContent.none, showArray, out _);
                Label(new GUIContent(string.Format("<b>{0}:</b>", array.displayName)));

                if (GUIUtils.SmallButton(new GUIContent("", "Add Element"), Colors.green, Colors.black)) {
                    array.AddNew().SetValue((UnityEngine.Object)null);       
                }
                EditorGUILayout.EndHorizontal();

                int newVal = array.arraySize;
                if (newVal != 0) {
                    Space();
                }
                
                int deleteIndex = -1;
                //array elements
                
                for (int i = 0; i < newVal; i++) {
                    EditorGUILayout.BeginHorizontal();

                    if (SmallDeleteButton()) {
                        deleteIndex = i;
                    }
                    DrawProp (array[i]);
                    EditorGUILayout.EndHorizontal();
                
                }
                if (deleteIndex != -1){
                    array.DeleteAt(deleteIndex);
                }
                
                EndBox(newVal == 0 ? 0 : 1);

            }
            else {
                EditorGUILayout.BeginHorizontal();
                showArray = SmallToggleButton(GUIContent.none, showArray, out _);
                Label(new GUIContent(string.Format("<b>{0}</b>", array.displayName)));
                EditorGUILayout.EndHorizontal();


            }
    
        }
        public static bool DrawTextProp (EditorProp prop, GUIContent content, TextFieldType type, bool overrideHotKeys, string controlName, params GUILayoutOption[] options) {
            EditorGUILayout.BeginHorizontal();
            Label(content, options);
            bool changed = DrawTextProp(prop, type, overrideHotKeys, controlName);
            EditorGUILayout.EndHorizontal();
            return changed;
        }
        public static bool DrawTextProp (EditorProp prop, TextFieldType type, bool overrideHotKeys, string controlName, params GUILayoutOption[] options) {
            bool changed;
            string new_val = DrawTextField(prop.stringValue, type, overrideHotKeys, controlName, out changed, options);            
            if (changed) prop.SetValue( new_val );
            return changed;
        }
        public static void DrawMultiLineStringProp (EditorProp prop, GUIContent gui, bool overrideHotKeys, string controlName, params GUILayoutOption[] options) {
            Label(gui);
            DrawMultiLineStringProp(prop, overrideHotKeys, controlName, options);
        }
        public static void DrawMultiLineStringProp (EditorProp prop, bool overrideHotKeys, string controlName, params GUILayoutOption[] options) {
            bool changed;
            string new_val = DrawTextField(prop.stringValue, TextFieldType.Area, overrideHotKeys, controlName, out changed, options);            
            if (changed) prop.SetValue( new_val );
        }

        const float lineHeight = 16;
        public static void DrawMultiLineExpandableString(EditorProp prop, bool overrideHotKeys, string controlName, float minHeight = lineHeight) {
            DrawMultiLineStringProp(prop, overrideHotKeys, controlName, GUILayout.MinHeight(Mathf.Max(prop.stringValue.Split('\n').Length * lineHeight, minHeight)));
        }

        
        public enum TextFieldType {
            Normal, Delayed, Area
        }
        public static string DrawTextField (string value, TextFieldType type, bool overrideHotKeys, string controlName, out bool changed, params GUILayoutOption[] options) {
            GUI.backgroundColor = Color.white;
            
            string nameToUse = overrideHotKeys ? overrideKeyboardControlName : controlName;
            NameNextControl(nameToUse);
            
            string newVal = value;
            switch(type) {
                case TextFieldType.Normal: newVal = EditorGUILayout.TextField(value, options); break;
                case TextFieldType.Delayed: newVal = EditorGUILayout.DelayedTextField(value, options); break;
                case TextFieldType.Area: newVal = EditorGUILayout.TextArea(value, options); break;
            }
            if (IsFocused(nameToUse)) {
                UnityEngine.Event e = UnityEngine.Event.current;
                bool clicked = (e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == 0;         
                if (clicked && !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)) {
                    FocusOnTextArea("");
                }   
            }
            changed = newVal != value;
            return newVal;
        }
        public static string DrawTextField (Rect r, string value, bool overrideHotKeys, string controlName, out bool changed) {
            GUI.backgroundColor = Color.white;
            
            string nameToUse = overrideHotKeys ? overrideKeyboardControlName : controlName;
            NameNextControl(nameToUse);
            string newVal = GUI.TextField(r, value);
            if (IsFocused(nameToUse)) {
                UnityEngine.Event e = UnityEngine.Event.current;
                bool clicked = (e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == 0;         
                if (clicked && !r.Contains(e.mousePosition)) {
                    FocusOnTextArea("");
                }   
            }
            changed = newVal != value;
            return newVal;
        }

        public static void FocusOnTextArea (string name) {
            EditorGUI.FocusTextInControl(name);
            GUI.FocusControl(name);
        }
        public static void NameNextControl(string name) {
            GUI.SetNextControlName(name);
        }
  
        public const string overrideKeyboardControlName = "overrideKeyboard";
        public static bool KeyboardOverriden () {
            return IsFocused(overrideKeyboardControlName);
        }


        public static bool IsFocused(string controlName) {
            return GUI.GetNameOfFocusedControl() == controlName;
        }

        public static string DrawDirectoryField (string value, GUIContent content, bool forceProject=true, params GUILayoutOption[] options) {
            
            EditorGUILayout.BeginHorizontal();
            Label(content, options);

            GUI.enabled = false;
            EditorGUILayout.TextField(GUIContent.none, value);
            GUI.enabled = true;
            
            //change to folder icon
            if (SmallButton(new GUIContent ("D", "Select Directory"))) {
                string dPath = Application.dataPath;
                string path = EditorUtility.OpenFolderPanel("Choose Directory", dPath, "");
                if (path != "") {
                    if (forceProject) {
                        if (path.StartsWith(dPath)) value = path.Substring(dPath.Length - 6) + "/";
                        else Debug.LogError( string.Format("Invalid Selection: '{0}'\nDirectory must be in the project!", path ) );
                    }
                    else value = path + "/";
                }
            }
            //delete
            if (SmallDeleteButton()) value = "";
            EditorGUILayout.EndHorizontal();
            
            return value;
        }
        
        public static bool DrawDirectoryField (EditorProp prop, GUIContent content, bool forceProject=true, params GUILayoutOption[] options) {
            
            string old = prop.stringValue;
            string newVal = DrawDirectoryField(old, content, forceProject, options);
            if (old != newVal) {
                prop.SetValue(newVal);
                return true;
            }
            return false;
        }
        
        public static void Label (GUIContent c, bool fit_content) {
            Label(c, Colors.liteGray, fit_content);
        }
        public static void Label (GUIContent c, Color32 text_color, bool fit_content) {
            Label(c, text_color, fit_content ? GUILayout.Width(GUIStyles.label.CalcSize(c).x) : null);
        }
        public static void Label (GUIContent c, params GUILayoutOption[] options) {
            Label(c, Colors.liteGray, options);
        }
        public static void Label (GUIContent c, Color32 text_color, params GUILayoutOption[] options) {
            DoWithinColor (GUIStyles.label, text_color);
             //() => 
             EditorGUILayout.LabelField(c, GUIStyles.label, options);
             //);
        }
        public static void Label (Rect r, GUIContent c, Color32 text_color) {
            DoWithinColor (GUIStyles.label, text_color);//, () => 
            GUI.Label(r, c, GUIStyles.label);
            // );
        }



        
        public static void Space(int count = 1) {
            for (int i = 0; i < count; i++) EditorGUILayout.Space();
        }
        static void DoWithinColor(GUIStyle style, Color32 text_color){//, Action fn) {
            //Color32 orig_txt0 = style.normal.textColor;
            //Color32 orig_txt1 = style.active.textColor;
            style.normal.textColor = text_color;
            style.active.textColor = text_color;
            //fn();
            //style.normal.textColor = orig_txt0;
            //style.active.textColor = orig_txt1;
        }

        static void DoWithinColor(Color32 color, GUIStyle style, Color32 text_color){//}, Action fn) {
            //Color32 orig_bg = GUI.backgroundColor;
            //Color32 orig_txt0 = style.normal.textColor;
            //Color32 orig_txt1 = style.active.textColor;
            style.normal.textColor = text_color;
            style.active.textColor = text_color;
            GUI.backgroundColor = color;
            //fn();
            //GUI.backgroundColor = orig_bg;
            //style.normal.textColor = orig_txt0;
            //style.active.textColor = orig_txt1;
        }
        static void DoWithinColor(Color32 color){//, Action fn) {
            //Color32 orig_bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            //fn();
            //GUI.backgroundColor = orig_bg;
        }
        public static void StartBox (Color32 color, int space=0, params GUILayoutOption[] options) {
            DoWithinColor(color);//, () => 
            EditorGUILayout.BeginVertical(GUIStyles.box, options);
            //);
            Space(space);
        }

        public static void StartBox (int space=0, params GUILayoutOption[] options) {
            StartBox(Colors.medGray, space, options);
        }
        public static void EndBox (int space=0) {
            Space(space);
            EditorGUILayout.EndVertical();
        }
        
        public static void BeginIndent (int indentSpace = 1) {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < indentSpace; i++) SmallButtonClear();
            EditorGUILayout.BeginVertical();
        }
        public static void EndIndent () {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void StartCustomEditor () {
            EditorGUI.BeginChangeCheck();
            DoWithinColor(Colors.darkGray);//, () => 
            EditorGUILayout.BeginVertical(GUIStyles.window, GUILayout.MinHeight(1));
            //);   
        }
        public static void EndCustomEditor (EditorProp baseObject) {
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck()) {     
                baseObject.SaveObject();
            }
            GUI.backgroundColor = Color.white;
            
        }

        public static bool EndCustomEditorWindow () {
            EditorGUILayout.EndVertical();
            return EditorGUI.EndChangeCheck();
        }
        public static bool Button (GUIContent c, GUIStyle s, Color32 color, Color32 text_color, params GUILayoutOption[] options) {
            bool r = false;
            DoWithinColor(color, s, text_color);//, () => 
            r = GUILayout.Button(c, s, options);
            // );
            return r;
        }

        
        public static bool Button (GUIContent c, GUIStyle s, Color32 color, Color32 text_color, Rect rt){
            bool r = false;
            DoWithinColor(color, s, text_color);//, () => 
            r = GUI.Button(rt, c, s);
            //);
            return r;
        }

        public static bool Button(GUIContent c, GUIStyle s, params GUILayoutOption[] options) {
            return Button(c, s, Colors.medliteGray, Colors.liteGray, options);
        }
        public static bool Button (GUIContent c, GUIStyle s, bool fit_content) {
            return Button(c, s, Colors.medliteGray, Colors.liteGray, fit_content );
        }
        public static bool Button (GUIContent c, GUIStyle s, Color32 color, Color32 text_color, bool fit_content) {
            return Button(c, s, color, text_color, fit_content ? c.CalcWidth(s) : null );
        }

        public static bool ToggleButton (GUIContent c, GUIStyle s, bool value, Color32 onColor, Color32 offColor, out bool changed, params GUILayoutOption[] options) {
            changed = Button(c, s, value ? onColor : offColor, Colors.black, options);
            return changed ? !value : value;
        }

        public static bool ToggleButton (GUIContent c, GUIStyle s, bool value, out bool changed, params GUILayoutOption[] options) {
            changed = Button(c, s, value ? Colors.selected : Colors.medliteGray, 
            value ? Colors.black : Colors.liteGray);
            return changed ? !value : value;
        }

        static readonly GUIContent deleteButtonGUI = new GUIContent("");

        public static bool SmallDeleteButton(string hint = "Delete") {
            deleteButtonGUI.tooltip = hint;
            return SmallButton(deleteButtonGUI, Colors.red, Colors.white);
        }
        
        public static bool SmallButton (GUIContent c, Color32 color, Color32 text_color) {
            return Button(c, smallButtonStyle, color, text_color, smallButtonOpts );
        }
        public static bool SmallButton (GUIContent c) {
            return Button(c, smallButtonStyle, Colors.medliteGray, Colors.black, smallButtonOpts );
        }

        static GUIStyle smallButtonStyle = GUIStyles.miniButton;
        public static bool SmallToggleButton (GUIContent c, bool value, Color32 onColor, Color32 offColor, out bool changed) {
            changed = Button(c, smallButtonStyle, value ? onColor : offColor, Colors.black, smallButtonOpts);
            return changed ? !value : value;
        }

        public static bool SmallToggleButton (GUIContent c, bool value, out bool changed) {
            changed = SmallButton(c, value ? Colors.selected : Colors.medliteGray, Colors.black);
            return changed ? !value : value;
        }

        static readonly GUILayoutOption[] smallButtonOpts = new GUILayoutOption[] { 
            GUILayout.Width(12), GUILayout.Height(12) 
        };
        
        public static void SmallButtonClear () {
            SmallButton(GUIContent.none, Colors.clear, Colors.clear);
        }


        public enum FitContent { Largest, False, True, };
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
                if (fitContent == FitContent.Largest) EditorGUILayout.BeginVertical(largestWidth);
                else EditorGUILayout.BeginVertical();
                Space(1);
            }
            else EditorGUILayout.BeginHorizontal();
            
            for (int i = 0; i < c; i++) {
                bool selected = i == current;
                bool pressed = false;

                GUILayoutOption o = largestWidth;
                switch (fitContent) {
                    case FitContent.False:
                        o = GUILayout.ExpandWidth(true);
                        break;
                    case FitContent.True:
                        o = guis[i].CalcWidth(s);
                        break;
                }

                DoWithinColor(Colors.Toggle(selected), s, selected ? Colors.black : Colors.liteGray);//, () => 
                pressed = GUILayout.Button(guis[i], s, o);
                // );
                
                if (pressed) {
                    current = i;
                    changed = true;
                }
            }
            s.alignment = a;
            if (vertical) EditorGUILayout.EndVertical();
            else EditorGUILayout.EndHorizontal();
            return changed;
        }
            
        public static GUILayoutOption CalcWidth(this GUIContent c, GUIStyle s) {
            return GUILayout.Width(s.CalcSize(c).x);
        }
        public static void ShowPopUpAtMouse(PopupList.InputData inputData) {
            PopupWindow.Show(new Rect(UnityEngine.Event.current.mousePosition, Vector2.zero), new PopupList(inputData));
        }

        public static void HelpBox(string message, MessageType messageType) {
            GUI.backgroundColor = Color.white;
            GUIStyle s = EditorStyles.helpBox;
            bool origRichText = s.richText;
            s.richText = true;
            EditorGUILayout.HelpBox(message, messageType);
            s.richText = origRichText;
        }
    }
}