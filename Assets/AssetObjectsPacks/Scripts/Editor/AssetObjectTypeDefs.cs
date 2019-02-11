using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {

    [CustomEditor(typeof(AssetObjectPacks))]
    public class AssetObjectPacksEditor : Editor {
        const string def_params_field = "defParams";
        const string name_field = "name";
        const string parameter_field = "parameter";
        SerializedProperty packs;
        int current_pack_index;
        GUIContent[] tab_names;
        void OnEnable () {
            packs = serializedObject.FindProperty("packs");
            InitializeTabNames();
            OnPackViewChange();
        }

        SerializedProperty current_pack, current_default_params;
        
        string GetPackName(int index) {
            return packs.GetArrayElementAtIndex(index).FindPropertyRelative(name_field).stringValue;
        }
        void InitializeTabNames () {
            int l = packs.arraySize;
            tab_names = new GUIContent[l];
            for (int i = 0; i < l; i++) tab_names[i] = new GUIContent(GetPackName(i));
        }
        public static bool TypeValid(string typeName, int at_index) {
            if (typeName.IsEmpty()) return false;
            return typeName.IsValidTypeString();
        }
        bool DirectoryValid(string dir, int at_index) {
            if (dir.IsEmpty()) return false;
            return System.IO.Directory.Exists(dir);
        }
        bool FileExtensionValid(string file_extensions, int at_index) {
            if (file_extensions.IsEmpty()) return false;
            if (file_extensions.Contains(",")) {
                string[] split = file_extensions.Split(',');
                for (int i = 0; i < split.Length; i++) {
                    if (!FileExtensionValid(split[i], i)) return false;
                }
                return true;
            }
            return file_extensions.StartsWith(".");
        }

        bool PackNameValid(string name, int ignore_index) {
            if (name.IsEmpty()) return false;
            for (int i = 0; i < packs.arraySize; i++) {
                if (i == ignore_index) continue;
                if (packs.GetArrayElementAtIndex(i).FindPropertyRelative(name_field).stringValue == name) return false;
            }
            return true;
        }

        bool ParamNameValid(string name, int ignore_index) {
            if (name.IsEmpty()) return false;
            for (int i = 0; i < current_default_params.arraySize; i++) {
                if (i == ignore_index) continue;
                if (current_default_params.GetArrayElementAtIndex(i).FindPropertyRelative(parameter_field).FindPropertyRelative(name_field).stringValue == name) return false;
            }
            return true;
        }

        int[] GetExistingIDs () {
            return new int[0].GenerateArray( i => { return packs.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue; }, packs.arraySize );
        }
        void AddNewPack () {
            string orig_try_name = "New Pack";
            string new_name = orig_try_name;
            int trying = 0;
            while (!PackNameValid(new_name, -1)) {
                new_name = orig_try_name + " " + trying.ToString();
                trying ++;
            }

            int l = packs.arraySize;

            packs.InsertArrayElementAtIndex(l);
            SerializedProperty new_pack = packs.GetArrayElementAtIndex(l);

            new_pack.FindPropertyRelative("id").intValue = AssetObjectsEditor.GenerateNewIDList(1, GetExistingIDs())[0];  
            new_pack.FindPropertyRelative(name_field).stringValue = new_name;
            new_pack.FindPropertyRelative("objectsDirectory").stringValue = "";
            new_pack.FindPropertyRelative("assetType").stringValue = "";
            new_pack.FindPropertyRelative("fileExtensions").stringValue = "";
            new_pack.FindPropertyRelative(def_params_field).ClearArray();
            InitializeTabNames();
        }

        void OnPackViewChange() {
            assetTypeHelpText = fileExtensionsHelpText = nameHelpText = objectsDirectoryHelpText = "";
            current_pack = packs.GetArrayElementAtIndex(current_pack_index); 
            current_default_params = current_pack.FindPropertyRelative(def_params_field); 
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();



            EditorGUILayout.BeginHorizontal();
            if (packs.arraySize != 0) {
                int last_pack = current_pack_index;
                current_pack_index = GUIUtils.Tabs(tab_names, current_pack_index);
                if (current_pack_index != last_pack) {
                    OnPackViewChange();
                }
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
                    packs.DeleteArrayElementAtIndex(current_pack_index);
                    current_pack_index = 0;
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

        string nameHelpText, assetTypeHelpText, objectsDirectoryHelpText, fileExtensionsHelpText;

        delegate bool CheckInput(string input, int current_index);
        delegate string AdjustNewVal(string input);

        bool DrawStringField (int current_index, SerializedProperty pack, string prop_name, GUIContent label, ref string help_text, CheckInput check_input, AdjustNewVal adjust_new_val, string invalid_response) {
            bool was_changed = false;

            SerializedProperty prop = pack.FindPropertyRelative(prop_name);
            string old_val = prop.stringValue;
            string new_val = EditorGUILayout.DelayedTextField(label, old_val);
            
            new_val = adjust_new_val(new_val);
            bool old_val_empty = old_val.IsEmpty();
            bool new_val_empty = new_val.IsEmpty();
            bool changed = old_val != new_val && !new_val_empty;            
            if (changed || old_val_empty) {
                bool is_valid = check_input(new_val, current_index);
                    
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
         void DrawDefaultParam(SerializedProperty parameter, int index) {
            DrawStringField(index, parameter, name_field, new GUIContent("Name"), ref propertyNameHelpText, ParamNameValid, AdjustTrim, "Parameter name exists, or is invalid");
            EditorGUILayout.PropertyField(parameter.FindPropertyRelative(AssetObjectParam.param_type_field));
            EditorGUILayout.PropertyField(parameter.GetRelevantParamProperty());
        }


        string AdjustNone(string i) { return i; }
        string AdjustTrim(string i) { return i.Trim(); }
        string AdjustObjectDirectoryPath(string dir) {
            if (!dir.IsEmpty() && !dir.EndsWith("/")) return dir + "/";
            return dir;
        }

        void DrawCurrentPack() {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (DrawStringField (current_pack_index, current_pack,name_field, new GUIContent("Pack Name"), ref nameHelpText, PackNameValid, AdjustNone, "Pack name already exists!")) { InitializeTabNames(); }
            DrawStringField (current_pack_index, current_pack, "assetType", new GUIContent("Asset Type", "The asset type or component to the pack targets"), ref assetTypeHelpText, TypeValid, AdjustTrim, "Type does not exist in the current assembly!\nIf you're trying to target a Unity asset type or component, try adding 'UnityEngine.' before the asset type name.");
            DrawStringField (current_pack_index, current_pack,"objectsDirectory", new GUIContent("Objects Directory", "The directory where the assets are held"), ref objectsDirectoryHelpText, DirectoryValid, AdjustObjectDirectoryPath, "The directory specified does not exist in the project!");
            DrawStringField (current_pack_index, current_pack,"fileExtensions", new GUIContent("File Extensions", "The file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'"), ref fileExtensionsHelpText, FileExtensionValid, AdjustTrim, "Error!\nPlease specify file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'");
                

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset Object Parameters:");
            if (GUILayout.Button("+")) AddNewParameterToCurrent();
            EditorGUILayout.EndHorizontal();
            

            EditorGUI.indentLevel++;
            int l = current_default_params.arraySize;
            if (show_params == null || show_params.Length != l) show_params = new bool[l];
            
            List<int> delete_indicies = new List<int>();
            for (int i = 0; i < l; i++) {
                bool delete;
                DrawDefaultParamDef (current_default_params.GetArrayElementAtIndex(i), i, out delete);
                if(delete) delete_indicies.Add(i);
            }
            for (int i = delete_indicies.Count-1; i >= 0; i--) current_default_params.DeleteArrayElementAtIndex(delete_indicies[i]);
            EditorGUI.indentLevel--;
            


            EditorGUILayout.EndVertical();
            

        }

        void AddNewParameterToCurrent() {
            string orig_try_name = "New Parameter";
            string new_name = orig_try_name;
            int trying = 0;
            while (!ParamNameValid(new_name, -1)) {
                new_name = orig_try_name + " " + trying.ToString();
                trying ++;
            }
            
            SerializedProperty new_param = current_default_params.AddNewElement();
            SerializedProperty param_default = new_param.FindPropertyRelative(parameter_field);
            param_default.FindPropertyRelative(name_field).stringValue = new_name;
            new_param.FindPropertyRelative("hint").stringValue = "";

        }

        string propertyNameHelpText;


        bool[] show_params;
        void DrawDefaultParamDef(SerializedProperty defaultParam, int index, out bool delete) {
            
            EditorGUILayout.BeginHorizontal();
            show_params[index] = EditorGUILayout.Foldout(show_params[index], defaultParam.FindPropertyRelative(parameter_field).FindPropertyRelative(name_field).stringValue);
            delete = GUIUtils.LittleButton(EditorColors.red_color);
            EditorGUILayout.EndHorizontal();

            if (show_params[index]) {
                EditorGUI.indentLevel ++;

                EditorGUILayout.PropertyField(defaultParam.FindPropertyRelative("hint"));
                DrawDefaultParam(defaultParam.FindPropertyRelative(parameter_field), index);
                EditorGUI.indentLevel --;
            }
            
        }
       
    }


}