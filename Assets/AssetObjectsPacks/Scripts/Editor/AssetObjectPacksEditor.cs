using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {

    [CustomEditor(typeof(AssetObjectPacks))]
    public class AssetObjectPacksEditor : Editor {
        const string objectsDirectoryField = "objectsDirectory";
        const string assetTypeField = "assetType";
        const string fileExtensionsField = "fileExtensions";
        const string defParamsField = "defParams";
        const string nameField = "name";
        const string parameterField = "parameter";
        const string packsField = "packs";
        const string idField = "id";
        const string hintField = "hint";

        int curPackI;
        GUIContent[] tabNames;
        SerializedProperty packs, curPack, curDefParams;
        string nameHelpText, assetTypeHelpText, objectsDirectoryHelpText, fileExtensionsHelpText, propertyNameHelpText;
        bool[] show_params;

        delegate bool CheckInput(string input, int current_index);
        delegate string AdjustNewVal(string input);

        void OnEnable () {
            packs = serializedObject.FindProperty(packsField);
            InitializeTabNames();
            OnPackViewChange();
        }
        
        string GetPackName(int index) {
            return packs.GetArrayElementAtIndex(index).FindPropertyRelative(nameField).stringValue;
        }
        string GetCurrentDefParamName(int index) {
            return curDefParams.GetArrayElementAtIndex(index).FindPropertyRelative(parameterField).FindPropertyRelative(nameField).stringValue;
        }
        int GetPackID (int index) {
            return packs.GetArrayElementAtIndex(index).FindPropertyRelative(idField).intValue;
        }
        void InitializeTabNames () {
            tabNames = new GUIContent[packs.arraySize].Generate( i => { return new GUIContent(GetPackName(i)); } );
        }
        public static bool TypeValid(string typeName, int at_index) {
            return typeName.IsValidTypeString();
        }
        bool DirectoryValid(string dir, int at_index) {
            return System.IO.Directory.Exists(dir);
        }
        bool FileExtensionValid(string file_extensions, int at_index) {
            if (file_extensions.Contains(",")) {
                string[] split = file_extensions.Split(',');
                for (int i = 0; i < split.Length; i++) {
                    if (!FileExtensionValid(split[i], i)) return false;
                }
                return true;
            }
            return file_extensions.StartsWith(".");
        }


        delegate string GetNameAtIndex(int index);

        bool NameValid (string name, int ignore_index, int count, GetNameAtIndex get_name) {
            for (int i = 0; i < count; i++) {
                if (i == ignore_index) continue;
                if (get_name(i) == name) return false;
            }
            return true;
        }
        bool PackNameValid(string name, int ignore_index=-1) {
            return NameValid(name, ignore_index, packs.arraySize, GetPackName);
        }
        bool ParamNameValid(string name, int ignore_index=-1) {
            return NameValid(name, ignore_index, curDefParams.arraySize, GetCurrentDefParamName);
        }
        

        void AddNewPack () {
            string new_name = AppendStringToUnique( "New Pack", PackNameValid );
            
            SerializedProperty new_pack = packs.AddNewElement();
            new_pack.FindPropertyRelative(idField).intValue = AssetObjectsEditor.GenerateNewIDList(1, new int[packs.arraySize].Generate( i => { return GetPackID(i); } ))[0];  
            new_pack.FindPropertyRelative(nameField).stringValue = new_name;
            new_pack.FindPropertyRelative(objectsDirectoryField).stringValue = string.Empty;
            new_pack.FindPropertyRelative(assetTypeField).stringValue = string.Empty;
            new_pack.FindPropertyRelative(fileExtensionsField).stringValue = string.Empty;
            new_pack.FindPropertyRelative(defParamsField).ClearArray();

            InitializeTabNames();
        }

        void OnPackViewChange() {
            assetTypeHelpText = fileExtensionsHelpText = nameHelpText = objectsDirectoryHelpText = string.Empty;
            curPack = packs.GetArrayElementAtIndex(curPackI); 
            curDefParams = curPack.FindPropertyRelative(defParamsField); 
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            if (packs.arraySize != 0) {
                bool changed_pack;
                curPackI = GUIUtils.Tabs(tabNames, curPackI, out changed_pack);
                if (changed_pack) OnPackViewChange();
            }

            if (GUILayout.Button("Add New Pack", EditorStyles.toolbarButton)) {
                AddNewPack();
            }

            EditorGUILayout.EndHorizontal();

            if (packs.arraySize != 0) {
                
                EditorGUILayout.Space();
                DrawCurrentPack();
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Color32 orig_color = GUI.backgroundColor;
                GUI.backgroundColor = EditorColors.red_color;
                if (GUILayout.Button("Delete Pack")) {
                    packs.DeleteArrayElementAtIndex(curPackI);
                    curPackI = 0;
                    InitializeTabNames();
                    OnPackViewChange();
                }
                GUI.backgroundColor = orig_color;
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        bool DrawStringField (int current_index, SerializedProperty pack, string prop_name, GUIContent label, ref string help_text, CheckInput check_input, AdjustNewVal adjust_new_val, string invalid_response) {
            bool was_changed = false;

            SerializedProperty prop = pack.FindPropertyRelative(prop_name);
            string old_val = prop.stringValue;
            
            string new_val = EditorGUILayout.DelayedTextField(label, old_val);
            
            new_val = adjust_new_val(new_val);

            bool changed = old_val != new_val && !new_val.IsEmpty();            
            
            if (changed || old_val.IsEmpty()) {
            
                bool is_valid = !new_val.IsEmpty() && check_input(new_val, current_index);
                if (is_valid) {
                    help_text = "";
                    prop.stringValue = new_val;
                    was_changed = true;
                }
                else {
                    help_text = "['" + new_val + "'] " + invalid_response;
                }
            }
            if (!help_text.IsEmpty()) EditorGUILayout.HelpBox(help_text, MessageType.Error);
            return was_changed;
        }
        
        string AdjustNone(string i) { return i; }
        string AdjustTrim(string i) { return i.Trim(); }
        string AdjustObjectDirectoryPath(string dir) {
            if (!dir.IsEmpty() && !dir.EndsWith("/")) return dir + "/";
            return dir;
        }

        void DrawCurrentPack() {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (DrawStringField (curPackI, curPack, nameField, new GUIContent("Pack Name"), ref nameHelpText, PackNameValid, AdjustNone, "Pack name already exists!")) { InitializeTabNames(); }
            DrawStringField (curPackI, curPack, assetTypeField, new GUIContent("Asset Type", "The asset type or component to the pack targets"), ref assetTypeHelpText, TypeValid, AdjustTrim, "Type does not exist in the current assembly!\nIf you're trying to target a Unity asset type or component, try adding 'UnityEngine.' before the asset type name.");
            DrawStringField (curPackI, curPack, objectsDirectoryField, new GUIContent("Objects Directory", "The directory where the assets are held"), ref objectsDirectoryHelpText, DirectoryValid, AdjustObjectDirectoryPath, "The directory specified does not exist in the project!");
            DrawStringField (curPackI, curPack, fileExtensionsField, new GUIContent("File Extensions", "The file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'"), ref fileExtensionsHelpText, FileExtensionValid, AdjustTrim, "Error!\nPlease specify file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset Object Parameters:");
            if (GUILayout.Button("+")) AddNewParameterToCurrent();
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel++;
            int l = curDefParams.arraySize;
            if (show_params == null || show_params.Length != l) show_params = new bool[l];
            
            List<int> delete_indicies = new List<int>();
            for (int i = 0; i < l; i++) {
                bool delete;
                DrawDefaultParamDef (curDefParams.GetArrayElementAtIndex(i), i, out delete);
                if(delete) delete_indicies.Add(i);
            }
            for (int i = delete_indicies.Count-1; i >= 0; i--) curDefParams.DeleteArrayElementAtIndex(delete_indicies[i]);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

        }


        delegate bool ValidCheck (string check_string, int ignore_index=-1);
        string AppendStringToUnique( string orig_name, ValidCheck valid_check, int max_checks = 900 ) {
            string new_name = orig_name;
            int trying = 0;
            while (!valid_check(new_name) && trying <= max_checks ) {
                new_name = orig_name + " " + trying.ToString();
                trying ++;
            }
            return new_name;
        }


        void AddNewParameterToCurrent() {
            string new_name = AppendStringToUnique( "New Parameter", ParamNameValid );
            SerializedProperty new_param = curDefParams.AddNewElement();
            SerializedProperty param_default = new_param.FindPropertyRelative(parameterField);
            param_default.FindPropertyRelative(nameField).stringValue = new_name;
            new_param.FindPropertyRelative(hintField).stringValue = string.Empty;
        }



        void DrawDefaultParamDef(SerializedProperty defaultParam, int index, out bool delete) {
            EditorGUILayout.BeginHorizontal();
            show_params[index] = EditorGUILayout.Foldout(show_params[index], defaultParam.FindPropertyRelative(parameterField).FindPropertyRelative(nameField).stringValue);
            delete = GUIUtils.SmallButton(new GUIContent("D", "Delete Parameter"), EditorColors.red_color, EditorColors.white_color);
            EditorGUILayout.EndHorizontal();
            if (show_params[index]) {
                EditorGUI.indentLevel ++;
                EditorGUILayout.PropertyField(defaultParam.FindPropertyRelative(hintField));
                DrawDefaultParam(defaultParam.FindPropertyRelative(parameterField), index);
                EditorGUI.indentLevel --;
            }   
        }  
        void DrawDefaultParam(SerializedProperty parameter, int index) {
            DrawStringField(index, parameter, nameField, new GUIContent("Name"), ref propertyNameHelpText, ParamNameValid, AdjustTrim, "Parameter name exists, or is invalid");
            EditorGUILayout.PropertyField(parameter.FindPropertyRelative(AssetObjectParam.param_type_field));
            EditorGUILayout.PropertyField(SerializedPropertyUtils.Parameters.GetParamProperty( parameter));
        }

    }
}