using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(PacksManager))]
    public class PacksManagerEditor : Editor {

        public static EditorProp GetPacksList () {
            if (AssetObjectsEditor.packManager == null) return null;
            return new EditorProp ( new SerializedObject ( AssetObjectsEditor.packManager ).FindProperty( packsField ) );
        }

        const string defaultParametersField = "defaultParameters";
        const string nameField = "name";
        const string objectsDirectoryField = "objectsDirectory";
        const string assetTypeField = "assetType";
        const string fileExtensionsField = "fileExtensions";
        const string idField = "id";

        const string fieldInvalidPrefix = "<b>{0} ::</b>\n\n\t<i>'{1}'</i>\n\n\tis empty or ";
        const string dupPackNamesHelp = "\n\nThere are multiple packs named:\n\n\t<i>'{0}'</i>\n\n";
        const string dupParametersHelp = "\n\n<b>'{0}'</b> has multiple parameters named:\n\n\t<i>'{1}'</i>\n\n";
        const string nullManagerHelp = "\n\nPacks Manager Object could not be found!\n\nIf it was deleted, create a new one.\n\n( Right Click in the Unity project window ->\nCreate -> Asset Objects Packs -> Packs Manager )\n\n";
        const string packNullError = "\n\nPlease choose or create an Asset Object Pack\n\n";
        const string packNameHeader = "\n\n<b>[ {0} ]</b>\n\n";

        const int packFieldsCount = 3;
        static readonly string[] fieldDisplayNames = new string[] { "Asset Type", "Objects Directory", "File Extension(s)" };
        static readonly string[] invalidFieldMsgs = new string[] {
            fieldInvalidPrefix + "does not exist in the current assembly!\n\tIf you're trying to target a Unity asset type or component,\n\ttry adding <i>'UnityEngine.'</i> before the asset type name.\n\n",
            fieldInvalidPrefix + "does not exist in the project!\n\n",
            fieldInvalidPrefix + "invalid.\n\n\tPlease specify file extensions to look for in the directory, seperated by commas.\n\t<b>E.G.:</b>\n\t\t<i>'.fbx, .wav, .mp3'</i>\n\n",
        };
        
        const string genIDsHelpString = "\n\n{0} [ '{1}' ] file(s) without proper IDs in the pack directory.\n\n\t<i>'{2}'</i>\n\n";
        

        public static void GetErrorsAndWarnings (int packIndex, out string[] errors, out string[] warnings, out int noIDsCount) {
            EditorProp packs = GetPacksList();            
            GetErrorsAndWarnings (packs, packs[packIndex], out errors, out warnings, out noIDsCount);
        }
        public static void GetErrorsAndWarnings (EditorProp packs, EditorProp pack, out string[] errors, out string[] warnings, out int noIDsCount) {
            
            List<string> err = new List<string>(), wrns = new List<string>();
            noIDsCount = 0;
            if (packs == null) err.Add(nullManagerHelp);
            if (pack == null) err.Add(packNullError);            
            else {
                string name, objectsDirectory, fileExtensions, assetType;
                GetValues(pack, out name, out objectsDirectory, out fileExtensions, out assetType);

                string[] values = new string[] { assetType, objectsDirectory, fileExtensions, };

                bool[] validChecks = new bool[] { assetType.IsValidTypeString(), objectsDirectory.IsValidDirectory(), FileExtensionValid(fileExtensions), };

                string hlpMsg = string.Format(packNameHeader, name);
                bool hasError = false;
                for (int i = 0; i < packFieldsCount; i++) {
                    if (!validChecks[i]) {
                        hlpMsg += string.Format(invalidFieldMsgs[i], fieldDisplayNames[i], values[i]); 
                        hasError = true;
                    }
                }
                
                if (hasError)
                    err.Add(hlpMsg);
                else
                    noIDsCount = AssetObjectsEditor.GetAllAssetObjectPathsWithoutIDs(objectsDirectory, fileExtensions).Length;
                
                string dupName;
                if (packs.ContainsDuplicateNames(out dupName, nameField)) wrns.Add( string.Format(dupPackNamesHelp, dupName));
                if (CustomParameterEditor.ParamsListContainsDuplicateName(pack[defaultParametersField], out dupName)) wrns.Add( string.Format(dupParametersHelp, name, dupName));
                if (noIDsCount != 0) wrns.Add(string.Format(genIDsHelpString, noIDsCount, fileExtensions, objectsDirectory));
            }
            errors = err.ToArray();
            warnings = wrns.ToArray();
        }

        public static void AdjustParametersToPack (EditorProp parameters, int packIndex, bool clear) {
            AdjustParametersToPack (parameters, GetPacksList()[packIndex], clear);    
        }
        public static void AdjustParametersToPack (EditorProp parameters, EditorProp pack, bool clear) {
            if (clear) CustomParameterEditor.ClearAndRebuildParameters(parameters, pack[defaultParametersField]);
            else CustomParameterEditor.UpdateParametersToReflectDefaults(parameters, pack[defaultParametersField]);
        }
        public static void GenerateIDsForPack (int packIndex) {
            GenerateIDsForPack(GetPacksList()[packIndex]);
        }
        public static void GenerateIDsForPack (EditorProp pack) {
            string name, objectsDirectory, fileExtensions, assetType;
            GetValues(pack, out name, out objectsDirectory, out fileExtensions, out assetType);
            AssetObjectsEditor.GenerateNewIDs(AssetObjectsEditor.GetAllAssetObjectPaths( objectsDirectory, fileExtensions, false), AssetObjectsEditor.GetAllAssetObjectPathsWithoutIDs(objectsDirectory, fileExtensions));
        }
        public static void GetValues (int packIndex, out string name, out string objectsDirectory, out string fileExtensions, out string assetType) {
            GetValues(GetPacksList()[packIndex], out name, out objectsDirectory, out fileExtensions, out assetType);
        }
        public static void GetValues (EditorProp pack, out string name, out string objectsDirectory, out string fileExtensions, out string assetType) {
            name = pack[nameField].stringValue;
            objectsDirectory = pack[objectsDirectoryField].stringValue;
            fileExtensions = pack[fileExtensionsField].stringValue;
            assetType = pack[assetTypeField].stringValue;       
        }
        static bool FileExtensionValid(string file_extensions) {
            if (file_extensions.Contains(",")) {
                string[] split = file_extensions.Split(',');
                int l = split.Length;
                for (int i = 0; i < l; i++) {
                    if (!FileExtensionValid(split[i])) return false;
                }
                return true;
            }
            return file_extensions.StartsWith(".");
        }
        void AddParameterToPack (EditorProp pack) {
            CustomParameterEditor.MakeParamDefault( pack[defaultParametersField].AddNew() ) ;      
        }
        void DeleteParameterFromPack (EditorProp pack, int index) {           
            pack[defaultParametersField].DeleteAt(index);
        }
        
        bool PackNameValid(string name) {
            for (int i = 0; i < packs.arraySize; i++) {
                if (packs[i][nameField].stringValue == name) return false;
            }
            return true;
        }
        void AddNewPackToPacksList () {
            int[] usedIDs = new int[packs.arraySize].Generate( i => { return packs[i][idField].intValue; } );
            int newID = AssetObjectsEditor.GenerateNewIDList(1, usedIDs)[0];

            string origName = "New Pack";
            string new_name = origName;
            int trying = 0;
            while (!PackNameValid(new_name) && trying <= 999 ) {
                new_name = origName + " " + trying.ToString();
                trying ++;
            }
            
            EditorProp newPack = packs.AddNew();
            //add default params
            EditorProp defParams = newPack[defaultParametersField];
            defParams.Clear();
            CustomParameterEditor.DefaultDurationParameter(defParams.AddNew());
        
            newPack[idField].SetValue( newID );            
            newPack[nameField].SetValue( new_name );
            newPack[objectsDirectoryField].SetValue( string.Empty );
            newPack[assetTypeField].SetValue( string.Empty );
            newPack[fileExtensionsField].SetValue( string.Empty );
        }

        public static class GUI {
            const string invalidDirMsg = "Invalid Selection: '{0}'\nObjects Directory must be in the project!";
            const string sDeletePack = "Delete Pack", sCancel = "Cancel", deletePackSureMsg = "Are you sure you want to delete this pack?";
            static readonly GUIContent deletePackGUI = new GUIContent(sDeletePack);
            static readonly GUIContent defParamsLbl = new GUIContent ("Asset Object Default Parameters:");
            static readonly GUIContent packNameGUI = new GUIContent("Pack Name");
            static readonly GUIContent packAssetTypeGUI = new GUIContent("Asset Type", "The asset type or component to the pack targets");
            static readonly GUIContent packDirGUI = new GUIContent("Objects Directory", "The directory where the assets are held");
            static readonly GUIContent packExtensionGUI = new GUIContent("File Extensions", "The file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'");
            static readonly GUIContent addParamGUI = new GUIContent("Add Parameter");
            static readonly GUIContent openFolderSelectGUI = new GUIContent ("F", "Select Directory");
            static readonly GUILayoutOption packsFieldsWidth = GUILayout.Width(100);

            public static GUIContent[] GetNameGUIs (EditorProp packs) {
                return new GUIContent[packs.arraySize].Generate( i => { return new GUIContent(packs[i][nameField].stringValue); } );
            }
            public static GUIContent[] GetDefaultParamGUIs(int packIndex) {

                return GetDefaultParamGUIs(GetPacksList()[packIndex]);
            }
            public static GUIContent[] GetDefaultParamGUIs(EditorProp pack) {
                EditorProp defParams = pack[defaultParametersField];
                int c = defParams.arraySize;
                return new GUIContent[c].Generate( i => { return CustomParameterEditor.GUI.GetParamGUI(defParams[i]); } );
            }
            public static void DrawPack(EditorProp pack, out bool deletePack, out bool changedName, out bool changedAssetType, out bool changedDir, out bool changedExtension, out bool changedAnyParamNames, out int deleteParamIndex, out bool addNewParameter) {
                
                GUIUtils.StartBox(1);

                //name
                changedName = GUIUtils.DrawDelayedTextProp (pack[nameField], packNameGUI, packsFieldsWidth);
                //asset type
                changedAssetType = GUIUtils.DrawDelayedTextProp (pack[assetTypeField], packAssetTypeGUI, packsFieldsWidth);
                
                //directory
                EditorGUILayout.BeginHorizontal();
                EditorProp dirProp = pack[objectsDirectoryField];
                UnityEngine.GUI.enabled = false;
                GUIUtils.DrawDelayedTextProp (dirProp, packDirGUI, packsFieldsWidth);
                UnityEngine.GUI.enabled = true;
                changedDir = false; 
                if (GUIUtils.SmallButton(openFolderSelectGUI)) {
                    string dPath = Application.dataPath;
                    string path = EditorUtility.OpenFolderPanel("Choose Objects Directory", dPath, "");
                    if (path != "") {
                        if (path.StartsWith(dPath)) {
                            dirProp.SetValue( path.Substring(dPath.Length - 6) + "/" );
                            changedDir = true;
                        }
                        else Debug.LogError( string.Format(invalidDirMsg, path ) );
                    }
                }
                EditorGUILayout.EndHorizontal();

                //file extensions
                changedExtension = GUIUtils.DrawDelayedTextProp (pack[fileExtensionsField], packExtensionGUI, packsFieldsWidth);
                GUIUtils.EndBox(1);
                
                //default params
                GUIUtils.StartBox(0);
                GUIUtils.Label(defParamsLbl, false);    
                GUIUtils.StartBox(0, EditorColors.darkGray);
                CustomParameterEditor.GUI.DrawParamsList (pack[defaultParametersField], true, out changedAnyParamNames, out deleteParamIndex);
                addNewParameter = GUIUtils.Button(addParamGUI, true, GUIStyles.miniButton);
                GUIUtils.EndBox(0);
                GUIUtils.EndBox(0);      

                //delete pack
                GUIUtils.StartBox(1);
                deletePack = false;
                if (GUIUtils.Button(deletePackGUI, false, GUIStyles.button, EditorColors.red, EditorColors.white )) deletePack = EditorUtility.DisplayDialog( sDeletePack, deletePackSureMsg, sDeletePack, sCancel );
                GUIUtils.EndBox(1);
              
            }

            static readonly GUIContent genIDsGUI = new GUIContent(sGenerateIDs);
            const string showWarningsTxt = "<b>{0}</b> Warnings ( {1} )";
            const string sGenerateIDs = "Generate IDs";
            const string genIDsSureMsg = "Generating IDs will rename assets without IDs, are you sure?";
            static bool showWarnings;

            public static void DrawErrorsAndWarnings (string[] errors, string[] warnings, int noIDcount, out bool generateNewIDs) {
                generateNewIDs = false;
                if (errors.Length == 0 && warnings.Length == 0) return;
                
                GUIUtils.StartBox(1);
                
                GUIStyle s = EditorStyles.helpBox;
                bool origRichText = s.richText;
                s.richText = true;
                
                for (int i = 0; i < errors.Length; i++) EditorGUILayout.HelpBox(errors[i], MessageType.Error);
                
                if (warnings.Length > 0) {

                    GUIContent c = new GUIContent( string.Format( showWarningsTxt, showWarnings ? " V " : " > ", warnings.Length ) );
                    if (GUIUtils.Button(c, true, GUIStyles.button, EditorColors.yellow, EditorColors.black)) {
                        showWarnings = !showWarnings;
                    }
                    if (showWarnings) {
                        for (int i = 0; i < warnings.Length; i++) {
                            EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
                        }
                        if (noIDcount != 0) {
                            if (GUIUtils.Button(genIDsGUI, false, GUIStyles.button, EditorColors.green, EditorColors.black)) {
                                generateNewIDs = EditorUtility.DisplayDialog(sGenerateIDs, genIDsSureMsg, sGenerateIDs, sCancel);
                            }
                            
                        }
                    }
                }
                s.richText = origRichText;
                GUIUtils.EndBox(1);
            }
        }

        const string packsField = "packs";
        EditorProp packs;
        string[] warningStrings, errorStrings;
        GUIContent[] tabNames;
        GUIContent addPackGUI = new GUIContent("Add New Pack");
        int curPackI, noIDsCount;

        void OnEnable () {
            packs = new EditorProp( serializedObject.FindProperty( packsField) );
            Reinitialize();
        }
        void Reinitialize () {
            tabNames = GUI.GetNameGUIs(packs);
            GetErrorsAndWarnings(packs, (packs.arraySize > 0) ? packs[curPackI] : null, out errorStrings, out warningStrings, out noIDsCount);
        }
        public override void OnInspectorGUI () {
            //base.OnInspectorGUI();
            GUIUtils.StartCustomEditor();

            bool genIDs;
            GUI.DrawErrorsAndWarnings(errorStrings, warningStrings, noIDsCount, out genIDs);
            
            //choose packs
            GUIUtils.StartBox(1);
            EditorGUILayout.BeginHorizontal();
            bool changedPack = packs.arraySize != 0 && GUIUtils.Tabs(tabNames, ref curPackI);
            bool addPack = GUIUtils.Button(addPackGUI, false, GUIStyles.toolbarButton, EditorColors.green, EditorColors.black);
            EditorGUILayout.EndHorizontal();
            GUIUtils.EndBox(1);
            
            int deleteParamI = -1;
            bool changedName = false, changedAssetType = false, changedDir = false, changedExtension = false, addNewParam = false, deletePack = false, changedAnyParamNames = false;
            if (packs.arraySize != 0) GUI.DrawPack(packs[curPackI], out deletePack, out changedName, out changedAssetType, out changedDir, out changedExtension, out changedAnyParamNames, out deleteParamI, out addNewParam);
            
            if (addPack) AddNewPackToPacksList();
            
            if (genIDs) GenerateIDsForPack(packs[curPackI]);
            
            if (addNewParam) AddParameterToPack(packs[curPackI]);
            
            if (deleteParamI >= 0) DeleteParameterFromPack(packs[curPackI], deleteParamI);
            
            if (deletePack) packs.DeleteAt(curPackI);

            int c = packs.arraySize;
            if (curPackI >= c) curPackI = c-1;

            if (genIDs || changedPack || changedName || changedAssetType || changedDir || changedExtension || deletePack || deleteParamI >= 0 || addNewParam || addPack) Reinitialize();
            
            GUIUtils.EndCustomEditor(this, false);
        }
    }
}