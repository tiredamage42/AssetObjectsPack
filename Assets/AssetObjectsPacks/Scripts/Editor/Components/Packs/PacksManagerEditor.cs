using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace AssetObjectsPacks {
    [CustomEditor(typeof(PacksManager))]
    public class PacksManagerEditor : Editor {
        public const string packsField = "packs";
        public const string idField = "id";
        const string nameField = "name";
        const string defaultParametersField = "defaultParameters";
        const string dirField = "dir";
        const string assetTypeField = "assetType";
        const string extensionsField = "extensions";
        const string isCustomField = "isCustom";

        string errors;
        int displayPackIndex;

        EditorProp so;
        PacksManager packsManager;
        EditorProp packs { get { return so[packsField]; } }

        void OnEnable () {
            so = new EditorProp( serializedObject );
            packsManager = target as PacksManager;
            errors = GetPacksErrors(packsManager, packs);
        }

        bool DrawTopPackSelectionTabs (int packsSize) {
            GUIUtils.StartBox(1);
        
            EditorGUILayout.BeginHorizontal();
            if (packsSize != 0) {    
                GUIContent[] tabGUIs = packsSize.Generate( i => new GUIContent(packs[i][nameField].stringValue) ).ToArray();    
                GUIUtils.Tabs(tabGUIs, ref displayPackIndex);
            }
            bool addedPack = false;
        
            if (GUIUtils.Button(new GUIContent("Add New Pack"), GUIStyles.toolbarButton, Colors.green, Colors.black)) {
                AddNewPackToPacksList( packs );
                addedPack = true;
            }
        
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox(1);
            return addedPack;
        }

        public override void OnInspectorGUI () {
            //base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();
            DrawPacksErrors(errors);
            int l = packs.arraySize;
            if (displayPackIndex >= l) displayPackIndex = l - 1;
            bool addedPack = DrawTopPackSelectionTabs(l);
            bool changedVars = DrawCurrentPack(l);
            GUIUtils.EndCustomEditor(so);
            if (addedPack || changedVars) errors = GetPacksErrors(packsManager, packs);
        }

        string InvalidAssetTypeError () {
            return "\nValue is empty, does not inherit from UnityEngine.Object, or does not exist in the current assembly!\n";
        }
        string InvalidDirectoryError () {
            return "\nValue is empty or does not exist in the project!\n";
        }
        string InvalidFileExtensionsError () {
            return "\nValue is empty or invalid.\n\nPlease specify file extensions to look for in the directory, seperated by commas.\n<b>E.G.:</b><i>'.fbx, .wav, .mp3'</i>\n";
        }

        void DrawHelpBox (EditorProp variable, Func<string, bool> validCheck, Func<string> errorMsg) {
            if (validCheck(variable.stringValue)) return;
            GUIUtils.HelpBox(errorMsg(), MessageType.Error);
            GUIUtils.Space();
        }
        bool CheckedVariable (EditorProp variable, GUIContent content, GUILayoutOption option) {
            return GUIUtils.DrawTextProp(variable, content, GUIUtils.TextFieldType.Normal, false, " var ", option);
        }   
        bool DrawPackFields (EditorProp pack) {
            GUILayoutOption w = GUILayout.Width(100);
            
            bool changedVar = false;

            GUIUtils.StartBox(1);
            //name
            changedVar = GUIUtils.DrawTextProp(pack[nameField], new GUIContent("Pack Name"), GUIUtils.TextFieldType.Normal, false, "name", w) || changedVar;
            
            GUIUtils.DrawToggleProp(pack[isCustomField], new GUIContent("Custom Objects", "Does the pack consist of custom objects that arent represented by assets in the project?"), w);

            GUIUtils.Space();

            bool isCustom = pack[isCustomField].boolValue;
            if (!isCustom) {
                //asset type
                changedVar = CheckedVariable (pack[assetTypeField], new GUIContent("Asset Type", "The asset type or component the pack targets.\nMust inherit from UnityEngine.Object"), w) || changedVar;
                DrawHelpBox(pack[assetTypeField], IsValidTypeString, InvalidAssetTypeError);
                //directory            
                changedVar = GUIUtils.DrawDirectoryField(pack[dirField], new GUIContent("Objects Directory", "The directory where the assets are held"), true, w) || changedVar;
                DrawHelpBox(pack[dirField], IsValidDirectory, InvalidDirectoryError);
                //file extensions
                changedVar = CheckedVariable (pack[extensionsField], new GUIContent("File Extensions", "The file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'"), w) || changedVar;
                DrawHelpBox(pack[extensionsField], FileExtensionValid, InvalidFileExtensionsError);    
            }
            else {
                changedVar = CheckedVariable (pack[assetTypeField], new GUIContent("Icon Asset Type", "The asset type or component to use for the icon.\nMust inherit from UnityEngine.Object"), w) || changedVar;
                DrawHelpBox(pack[assetTypeField], IsValidTypeString, InvalidAssetTypeError);
            }
            GUIUtils.EndBox(1);
            return changedVar;
        }

        bool DrawDeletePackButton () {
            bool delete = false;
            GUIUtils.StartBox(0);
            if (GUIUtils.Button(new GUIContent("Delete Pack"), GUIStyles.button, Colors.red, Colors.white )) {
                if (EditorUtility.DisplayDialog("Delete Pack", "Are you sure you want to delete this pack?", "Delete Pack", "Cancel")) {
                    packs.DeleteAt(displayPackIndex, "Deleted pack");
                    delete = true;
                }
            }
            GUIUtils.EndBox(0);
            return delete;
        }
        void DrawUpdateIDsButton () {
            AssetObjectPack displayPack = packsManager.packs[displayPackIndex];

            if (!displayPack.isCustom) {
                GUIUtils.StartBox();
                DrawGenIDsForPack(displayPack);
                GUIUtils.EndBox();
            }
        }
        void DrawGenIDsForPack (AssetObjectPack pack) {
            if (GUIUtils.Button(new GUIContent("Update IDs in Directory"), GUIStyles.button, Colors.selected, Colors.black)) {
                string msg = "Generating IDs will rename assets without IDs, are you sure?";
                if (EditorUtility.DisplayDialog("Generate IDs", msg, "Generate IDs", "Cancel")) {
                    UpdateIDsForPack(pack);
                }
            }
        }
        void UpdateIDsForPack (AssetObjectPack pack) {
            string[] pathsWithoutIDs = EditorUtils.GetFilePathsInDirectory(pack.dir, true, pack.extensions, AssetObjectsEditor.sIDKey, false);
            if (pathsWithoutIDs.Length == 0) {
                Debug.Log("All assets in " + pack.name + " objects directory have IDs");
                return;
            }
            GenerateNewIDs(AssetObjectsEditor.GetAllAssetObjectPaths(pack.dir, pack.extensions, false), pathsWithoutIDs);
        }
        
        void GenerateNewIDs (string[] validPaths, string[] noIDs) {
            int l = noIDs.Length;
            int[] newIDs = GenerateNewIDList(l, validPaths.Length.Generate(i => AssetObjectsEditor.GetObjectIDFromPath(validPaths[i])).ToHashSet() );
            for (int i = 0; i < l; i++) {
                string path = noIDs[i];
                if (path.Contains(AssetObjectsEditor.sIDKey)) {
                    Debug.LogWarning("asset was already assigned an id: " + path);
                    continue;
                }
                AssetDatabase.RenameAsset(path, AssetObjectsEditor.sIDKey + newIDs[i] + "-" + EditorUtils.RemoveDirectory(path));
            }
            Debug.Log("Assets are now ready with unique IDs");
        }
        int[] GenerateNewIDList (int count, HashSet<int> used_ids) {
            int[] result = new int[count];
            int generated = 0;
            int trying_id = 0;
            while (generated < count) {
                if (!used_ids.Contains(trying_id)) {
                    result[generated] = trying_id;
                    generated++;
                }
                trying_id++;
            }
            return result;
        }


        bool DrawCurrentPack(int packsSize){
            if (packsSize == 0 || displayPackIndex < 0) return false;
            EditorProp pack = packs[displayPackIndex]; 
            bool changedFields = DrawPackFields(pack);
            bool parametersChanged = DrawPackParameters(pack[defaultParametersField]);
            DrawUpdateIDsButton();
            GUIUtils.Space();
            bool delete = DrawDeletePackButton();
            return changedFields || delete || parametersChanged;
        }
            
        bool DrawPackParameters (EditorProp parameters) {
            bool anyChange = false;
            GUIUtils.StartBox();
            GUIUtils.Label(new GUIContent ("<b>Asset Object Default Parameters:</b>"));    
            GUIUtils.StartBox(Colors.darkGray);
            string dupName;
            if (parameters.ContainsDuplicateNames(out dupName)) {
                GUIUtils.HelpBox(string.Format("\nMultiple parameters named:\n\t<i>'{0}'</i>\n", dupName), MessageType.Error);
            }
            GUIUtils.StartBox (1);
            int deleteParamIndex = -1;
            for (int i = 0; i < parameters.arraySize; i++) {
                UnityEngine.GUI.enabled = i != 0;
                EditorGUILayout.BeginHorizontal();
                if (GUIUtils.SmallDeleteButton()) deleteParamIndex = i;
                if (DrawPackParameter(parameters[i])) anyChange = true;
                EditorGUILayout.EndHorizontal();
                UnityEngine.GUI.enabled = true;
            }
            if (deleteParamIndex >= 0) {
                parameters.DeleteAt(deleteParamIndex, "manual parameter delete");
                anyChange = true;
            }
            GUIUtils.BeginIndent();                
            if (GUIUtils.Button(new GUIContent("Add Parameter"), GUIStyles.miniButton, Colors.green, Colors.black, true)) {
                parameters.AddNew("New Parameter");
                anyChange = true;
            }
            GUIUtils.EndIndent();   
            GUIUtils.EndBox(1);
            GUIUtils.EndBox();
            GUIUtils.EndBox();  
            return anyChange;    
        }

        bool DrawPackParameter(EditorProp parameter) {        
            //name
            bool nameChanged = GUIUtils.DrawTextProp(parameter[nameField], GUIUtils.TextFieldType.Normal, false, "param name", GUILayout.MinWidth(32));
            //type
            GUILayoutOption pFieldWidth = GUILayout.Width(100);
            GUIUtils.DrawEnumProp(
                parameter[CustomParameterEditor.typeField], 
                (int i) => (CustomParameter.ParamType)i, 
                (Enum s) => (int)((CustomParameter.ParamType)s), 
                pFieldWidth
            );
            //value
            GUIUtils.DrawProp(CustomParameterEditor.GetParamValueProperty( parameter ), GUILayout.Width(128));
            return nameChanged;
        }    

        public static void DrawPacksErrors (string errors) {
            if (errors.IsEmpty()) return;
            GUIUtils.StartBox();
            GUIUtils.HelpBox(errors, MessageType.Error);
            GUIUtils.EndBox();
        }  

        
        public static bool PackHasErrors (AssetObjectPack pack, EditorProp packProp) {
            //duplicate param names
            if (packProp[defaultParametersField].ContainsDuplicateNames(out _))
                return true;
            

            if (!pack.isCustom) {
                //check variables
                Func<string, bool>[] validChecks = new Func<string, bool>[] { IsValidTypeString, IsValidDirectory, FileExtensionValid };
                string[] fieldChecks = new string[] { pack.assetType, pack.dir, pack.extensions };                
                for (int x = 0; x < 3; x++)
                    if (!validChecks[x](fieldChecks[x])) 
                        return true;    
            }
            
            return false;
        }

        public static string GetPacksErrors (PacksManager packsManager=null, EditorProp packsProp=null) {
            if (packsManager == null) 
                packsManager = PacksManager.instance;
                
            if (packsManager == null) 
                return "\nPacks Manager Object could not be found!" + 
                    "\n\nIf it was deleted, create a new one." + 
                    "\n\nIn the Unity project window:" + 
                    "\n\tRight Click -> Create -> Asset Objects Packs -> Packs Manager\n";
            
            int l = packsManager.packs.Length;
            if (l == 0) 
                return "\nPlease create an Asset Object Pack\n";

            if (packsProp == null) {
                packsProp = new EditorProp (new SerializedObject ( packsManager ) )[ packsField ];
            }
            
            //check for duplicate pack name
            string dupName;
            if (packsProp.ContainsDuplicateNames(out dupName)) 
                return "\nThere are multiple packs named:\n\t<i>'" + dupName + "'</i>\n";
            
            string packsString = "";
            for (int i = 0; i < l; i++)
                if (PackHasErrors(packsManager.packs[i], packsProp[i])) 
                    packsString += packsManager.packs[i].name + ", ";
            
            if (!packsString.IsEmpty()) 
                return string.Format("\nPack(s):\n<b>{0}</b>\nHave errors, fix them in the Packs Manager asset.\n", packsString);
            
            return null;
        }

        void AddNewPackToPacksList (EditorProp packs) {
            //generate new id for the new pack
            int newID = GenerateNewIDList(1, packs.arraySize.Generate( i => packs[i][idField].intValue ).ToHashSet())[0];
            
            var newPack = packs.AddNew("New Pack");
            newPack[idField].SetValue( newID );            
            newPack[dirField].SetValue( string.Empty );
            newPack[assetTypeField].SetValue( string.Empty );
            newPack[extensionsField].SetValue( string.Empty );
            newPack[isCustomField].SetValue( false );
            
            
            //add default params
            var defParams = newPack[defaultParametersField];
            defParams.Clear();
            DefaultDurationParameter(defParams.AddNew());
        }

        static void DefaultDurationParameter (EditorProp parameter) {
            parameter[CustomParameterEditor.nameField].SetValue( "Duration" );
            parameter[CustomParameterEditor.typeField].SetValue( (int)CustomParameter.ParamType.FloatValue );
            parameter[CustomParameter.ParamType.FloatValue.ToString()].SetValue( -1.0f );
        }

        static bool IsValidTypeString(string s) {
            if (s.IsEmpty()) return false;
            Type t = s.ToType();
            if (t == null) return false;
            return t.IsSubclassOf(typeof (UnityEngine.Object));
        }
        static bool IsValidDirectory(string s) {
            if (s.IsEmpty()) return false;
            if (!s.EndsWith("/")) return false;
            return Directory.Exists(s);
        }
        static bool FileExtensionValid(string s) {
            if (s.IsEmpty()) return false;
            if (s.Contains(",")) {
                string[] split = s.Split(',');
                for (int i = 0; i < split.Length; i++) {
                    if (!FileExtensionValid(split[i])) {
                        return false;
                    }
                }
                return true;
            }
            return s.StartsWith(".");
        }

        public static void UpdateAssetObjectParametersIfDifferentFromDefaults (EditorProp ao, EditorProp pack, bool reset, bool debug) {
            if (reset) CustomParameterEditor.CopyParameterList(ao[AOStateMachineEditor.paramsField], pack[defaultParametersField]);
            else UpdateParametersIfDifferentFromDefaults(ao[AOStateMachineEditor.paramsField], pack[defaultParametersField], debug);
        }

        static void UpdateParametersIfDifferentFromDefaults (EditorProp parameters, EditorProp defaultParams, bool debug) {
            int aoParametersCount = parameters.arraySize;
            int defParametersCount = defaultParams.arraySize;

            Func<EditorProp, string> GetParamName = (EditorProp parameter) => parameter[CustomParameterEditor.nameField].stringValue;

            //check for parameters to delete
            for (int i = aoParametersCount - 1; i >= 0; i--) {                    
                string name = GetParamName(parameters[i]);
                bool inDefParams = false;
                for (int d = 0; d < defParametersCount; d++) {                
                    if (GetParamName(defaultParams[d]) == name) {
                        inDefParams = true;
                        break;
                    }
                }
                if (!inDefParams) {
                    if (debug) {

                        Debug.Log("Deleting param: " + name);
                    }
                    parameters.DeleteAt(i, "deleting different params");
                }
            }
            // Debug.Log("finished deletes");

            aoParametersCount = parameters.arraySize;
            


            //check for parameters that need adding
            for (int i = 0; i < defParametersCount; i++) {
                var defParam = defaultParams[i];
                string defParamName = GetParamName(defParam);


                bool inParams = false;
                for (int p = 0; p < aoParametersCount; p++) {
                    if (GetParamName(parameters[p])== defParamName) {
                        inParams = true;
                        break;
                    }
                }
                if (!inParams) {
                    if (debug) {

                    Debug.Log("adding param: " + defParamName);
                    }
                    CustomParameterEditor.CopyParameter(parameters.AddNew(), defParam);
                }
            }

            aoParametersCount = parameters.arraySize;
            

            // Debug.Log("finished Adds");
            
            //reorder to same order

            
            EditorProp temp = null;

            for (int d = 0; d < defParametersCount; d++) {
                string defParamName = GetParamName(defaultParams[d]);
                var parameter = parameters[d];
                if (GetParamName(parameter) == defParamName) {
                    continue;
                }

                if (debug) {
                    Debug.Log("moving param: " + GetParamName(parameter));
                }

                EditorProp trueParam = null;
                //for (int p = d + 1; p < c_p; p++) {
                for (int p = 0; p < aoParametersCount; p++) {
                    trueParam = parameters[p];
                    if (GetParamName(trueParam) == defParamName) break;
                }

                if (trueParam == null) {
                    if (debug) {
                        Debug.LogError("couldnt find: " + defParamName);
                    }
                }
                //put the current one in temp

                if (temp == null) {
                    //make extra temp parameeter
                    temp = parameters.AddNew();
                }

                // Debug.Log("put in temp");
                CustomParameterEditor.CopyParameter(temp, parameter);
                //place the real param in the current
                // Debug.Log("put in current");
                CustomParameterEditor.CopyParameter(parameter, trueParam);


                //place temp in old param that was moved
                // Debug.Log("put in temp 2");
                
                CustomParameterEditor.CopyParameter(trueParam, temp);
            }

            if (temp != null) {
                //delete temp parameter
                parameters.DeleteAt(parameters.arraySize-1, "deleting temp param");
            }
            
            //check type changes
            Func<EditorProp, int> GetParamType = (EditorProp parameter) => parameter[CustomParameterEditor.typeField].intValue;
            for (int i = 0; i < defParametersCount; i++) {
                if (GetParamType(parameters[i]) != GetParamType(defaultParams[i])) {
                    if (debug) {

                    Debug.Log("changing: " + GetParamName(parameters[i]) + " from " + GetParamType(parameters[i]) + " to " + GetParamType(defaultParams[i]));
                    }
                    CustomParameterEditor.CopyParameter(parameters[i], defaultParams[i]);
                }
            }
        }
    }
}