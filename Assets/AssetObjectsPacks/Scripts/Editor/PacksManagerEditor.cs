using UnityEngine;
using UnityEditor;
namespace AssetObjectsPacks {
    [CustomEditor(typeof(PacksManager))]
    public class PacksManagerEditor : Editor {
        int curPackI;
        EditorProp packs;
        string[] warningStrings, errorStrings, pathsWithoutIDs;
        const string sDeletePack = "Delete Pack", sCancel = "Cancel", deletePackSureMsg = "Are you sure you want to delete this pack?";
        const string invalidDirMsg = "Invalid Selection: '{0}'\nObjects Directory must be in the project!";
        GUIContent[] tabNames;
        GUIContent deletePackGUI = new GUIContent(sDeletePack);
        GUIContent addPackGUI = new GUIContent("Add New Pack");
        static readonly GUIContent defParamsLbl = new GUIContent ("Asset Object Default Parameters:");
        static readonly GUIContent packNameGUI = new GUIContent("Pack Name");
        static readonly GUIContent packAssetTypeGUI = new GUIContent("Asset Type", "The asset type or component to the pack targets");
        static readonly GUIContent packDirGUI = new GUIContent("Objects Directory", "The directory where the assets are held");
        static readonly GUIContent packExtensionGUI = new GUIContent("File Extensions", "The file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'");
        static readonly GUIContent addParamGUI = new GUIContent("Add Parameter");
        static readonly GUIContent deleteParameterGUI = new GUIContent("D", "Delete Parameter");
        static readonly GUIContent openFolderSelectGUI = new GUIContent ("F", "Select Directory");
        static readonly GUILayoutOption pFieldWidth = GUILayout.Width(75);
        static readonly GUILayoutOption packsFieldsWidth = GUILayout.Width(100);

        void OnEnable () {
            packs = new EditorProp( serializedObject.FindProperty(PacksManager.packsField) );
            InitializeTabNames();

            RebuildErrorsAndWarnings(packs, (packs.arraySize > 0) ? packs[curPackI] : null, out errorStrings, out warningStrings, out pathsWithoutIDs);
            
        }
        void InitializeTabNames () {
            tabNames = new GUIContent[packs.arraySize].Generate( i => { return new GUIContent(packs[i][AssetObjectPack.nameField].stringValue); } );
        }
        static bool PackNameValid(EditorProp packs, string name) {
            for (int i = 0; i < packs.arraySize; i++) {
                if (packs[i][AssetObjectPack.nameField].stringValue == name) return false;
            }
            return true;
        }
        
        static void DefaultPackParameters (EditorProp defParams) {
            defParams.Clear();
            EditorProp newParam = defParams.AddNew();
            newParam[CustomParameter.nameField].SetValue( "Duration" );
            newParam[CustomParameter.typeField].SetEnumValue( (int)CustomParameter.ParamType.FloatValue );
            newParam[CustomParameter.ParamType.FloatValue.ToString()].SetValue( -1.0f );
            newParam[CustomParameter.hintField].SetValue( "Nagative values for object duration" );
        }
        
        static void AddNewPack (EditorProp packs) {
            int[] usedIDs = new int[packs.arraySize].Generate( i => { return packs[i][AssetObjectPack.idField].intValue; } );
            int newID = AssetObjectsEditor.GenerateNewIDList(1, usedIDs)[0];

            string origName = "New Pack";
            string new_name = origName;
            int trying = 0;
            while (!PackNameValid(packs, new_name) && trying <= 999 ) {
                new_name = origName + " " + trying.ToString();
                trying ++;
            }
            
            EditorProp newPack = packs.AddNew();

            //add default params
            DefaultPackParameters(newPack[AssetObjectPack.defaultParametersField]);
            newPack[AssetObjectPack.idField].SetValue( newID );            
            newPack[AssetObjectPack.nameField].SetValue( new_name );
            newPack[AssetObjectPack.objectsDirectoryField].SetValue( string.Empty );
            newPack[AssetObjectPack.assetTypeField].SetValue( string.Empty );
            newPack[AssetObjectPack.fileExtensionsField].SetValue( string.Empty );
        }
        
        static string[] OnGenIDs (EditorProp pack, string[] pathsWithoutIDs) {
            string dir = pack[AssetObjectPack.objectsDirectoryField].stringValue;
            string extensions = pack[AssetObjectPack.fileExtensionsField].stringValue;
            AssetObjectsEditor.GenerateNewIDs(AssetObjectsEditor.GetAllAssetObjectPaths( dir, extensions, false), pathsWithoutIDs);
            return AssetObjectsEditor.GetAllAssetObjectPathsWithoutIDs (dir, extensions);
        }

        static void RebuildErrorsAndWarnings (EditorProp packs, EditorProp pack, out string[] errorStrings, out string[] warningStrings, out string[] pathsWithoutIDs) {
            errorStrings = HelpGUI.GetErrorStrings(packs, pack);
            pathsWithoutIDs = new string[0];
            if (errorStrings.Length == 0) pathsWithoutIDs = AssetObjectsEditor.GetAllAssetObjectPathsWithoutIDs (pack[AssetObjectPack.objectsDirectoryField].stringValue, pack[AssetObjectPack.fileExtensionsField].stringValue);
            warningStrings = HelpGUI.GetWarningStrings(packs, pack, pathsWithoutIDs);
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            GUIUtils.CustomEditor(this,
                () => {

                    bool generateNewIDs;

                    HelpGUI.DrawErrorsAndWarnings(errorStrings, warningStrings, pathsWithoutIDs, out generateNewIDs);
                    
                    //choose packs
                    GUIUtils.StartBox(1);
                    EditorGUILayout.BeginHorizontal();
                    bool changed_pack = packs.arraySize != 0 && GUIUtils.Tabs(tabNames, ref curPackI);
                    bool addedNewPack = GUIUtils.Button(addPackGUI, false, EditorStyles.toolbarButton, EditorColors.green_color, EditorColors.black_color);
                    EditorGUILayout.EndHorizontal();
                    GUIUtils.EndBox(1);
                    
                    int deleteParamIndex = -1;
                    bool changedName = false, changedAssetType = false, changedDir = false, changedExtension = false, addNewParameter = false, deletePack = false, changedAnyParamNames = false;

                    if (packs.arraySize != 0) {
                        DrawPack(packs[curPackI], out changedName, out changedAssetType, out changedDir, out changedExtension);
                        
                        DrawDefaultParameters (packs[curPackI][AssetObjectPack.defaultParametersField], out changedAnyParamNames, out deleteParamIndex, out addNewParameter);

                        
                        GUIUtils.StartBox(2);
                        if (GUIUtils.Button(deletePackGUI, false, GUI.skin.button, EditorColors.red_color, EditorColors.white_color )) {
                            deletePack = EditorUtility.DisplayDialog( sDeletePack, deletePackSureMsg, sDeletePack, sCancel );
                        }
                        GUIUtils.EndBox(2);
                    }

                    if (addedNewPack) AddNewPack(packs);
                    
                    if (generateNewIDs) {

                        pathsWithoutIDs = OnGenIDs(packs[curPackI], pathsWithoutIDs);
                    }
                    
                    if (addNewParameter) AddNewParameterToPack(packs[curPackI]);
                    
                    if (deleteParamIndex >= 0) packs[curPackI][AssetObjectPack.defaultParametersField].DeleteAt(deleteParamIndex);
                    
                    if (deletePack) {
                        packs.DeleteAt(curPackI);
                        curPackI = 0;
                    }

                    if (generateNewIDs || changed_pack || changedName || changedAssetType || changedDir || changedExtension || deletePack || deleteParamIndex >= 0 || addNewParameter || addedNewPack) {
                        if (changedName || deletePack || addedNewPack) InitializeTabNames(); 

                        RebuildErrorsAndWarnings(packs, (packs.arraySize > 0) ? packs[curPackI] : null, out errorStrings, out warningStrings, out pathsWithoutIDs);
                    }
                }
            );
        }

        static void AddNewParameterToPack(EditorProp pack) {
            EditorProp new_param = pack[AssetObjectPack.defaultParametersField].AddNew();
            new_param[CustomParameter.hintField].SetValue("Hint");   
            new_param[CustomParameter.nameField].SetValue("Parameter Name");
        }

        static void DrawPack(EditorProp pack, out bool changedName, out bool changedAssetType, out bool changedDir, out bool changedExtension) {
            
            GUIUtils.StartBox(1);

            //name
            changedName = GUIUtils.DrawDelayedTextProp (pack[AssetObjectPack.nameField], packNameGUI, packsFieldsWidth);
            //asset type
            changedAssetType = GUIUtils.DrawDelayedTextProp (pack[AssetObjectPack.assetTypeField], packAssetTypeGUI, packsFieldsWidth);
            
            //directory
            EditorGUILayout.BeginHorizontal();
            EditorProp dirProp = pack[AssetObjectPack.objectsDirectoryField];
            
            GUI.enabled = false;
            GUIUtils.DrawDelayedTextProp (dirProp, packDirGUI, packsFieldsWidth);
            GUI.enabled = true;
            
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
            changedExtension = GUIUtils.DrawDelayedTextProp (pack[AssetObjectPack.fileExtensionsField], packExtensionGUI, packsFieldsWidth);
            
            GUIUtils.EndBox(1);
        }

        static void DrawDefaultParameters (EditorProp defParams, out bool changedAnyParamNames, out int deleteParamIndex, out bool addNewParameter) {
            GUIUtils.StartBox(0);

            GUIUtils.Label(defParamsLbl, false);
            
            GUIUtils.StartBox(0, EditorColors.dark_color);

            deleteParamIndex = -1;
            changedAnyParamNames = false;

            int l = defParams.arraySize;
            for (int i = 0; i < l; i++) {

                bool delete, changedParamName;
                GUI.enabled = i != 0;
                DrawDefaultParam (defParams[i], out delete, out changedParamName);
                GUI.enabled = true;
        
                if(delete) deleteParamIndex = i;
                changedAnyParamNames = changedAnyParamNames || changedParamName;
            }
    
            addNewParameter = GUIUtils.Button(addParamGUI, true, EditorStyles.miniButton);

            GUIUtils.EndBox(0);
            GUIUtils.EndBox(0);        
        }
        static void DrawDefaultParam(EditorProp parameter, out bool delete, out bool changedName) {
            GUIUtils.StartBox(0);
            
            EditorGUILayout.BeginHorizontal();
            delete = GUIUtils.SmallButton(deleteParameterGUI, EditorColors.red_color, EditorColors.white_color);
            EditorGUILayout.BeginVertical();
            
            //hint
            GUIUtils.DrawProp(parameter[CustomParameter.hintField], GUIUtils.blank_content);
            
            EditorGUILayout.BeginHorizontal();
            //name
            changedName = GUIUtils.DrawDelayedTextProp(parameter[CustomParameter.nameField]);

            //type
            GUIUtils.DrawProp(parameter[CustomParameter.typeField], GUIUtils.blank_content, pFieldWidth);
            
            //value
            GUIUtils.DrawProp(AOParameters.GetParamProperty( parameter), GUIUtils.blank_content, pFieldWidth);
            
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUIUtils.EndBox(0);
        }       
    }
}