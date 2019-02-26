using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetObjectsPacks {
    public static class PackEditor 
    {
        const string defaultParametersField = "defaultParameters";
        const string nameField = "name";
        const string dirField = "dir";
        const string assetTypeField = "assetType";
        const string extensionsField = "extensions";
        const string idField = "id";

        public static void GetErrorsAndWarnings (int packIndex, out string[] errors, out string[] warnings, out int noIDsCount) {
            EditorProp packs = PacksManagerEditor.GetPacksList();            
            GetErrorsAndWarnings (packs, packIndex < 0 ? null : packs[packIndex], out errors, out warnings, out noIDsCount);
        }
        public static void GetErrorsAndWarnings (EditorProp packs, EditorProp pack, out string[] errors, out string[] warnings, out int noIDsCount) {
            List<string> err = new List<string>(), wrns = new List<string>();
            noIDsCount = 0;
            
            if (packs == null) err.Add("\n\nPacks Manager Object could not be found!\n\nIf it was deleted, create a new one.\n\n( Right Click in the Unity project window ->\nCreate -> Asset Objects Packs -> Packs Manager )\n\n");
            if (pack == null) err.Add("\n\nPlease choose or create an Asset Object Pack\n\n");            
            else {
                string name, dir, extensions, assetType;
                GetValues(pack, out name, out dir, out extensions, out assetType);


                string hlpMsg = string.Format("\n\n<b>[ {0} ]</b>\n\n", name);
                bool hasError = false;


                string[] values = new string[] { assetType, dir, extensions, };
                bool[] validChecks = new bool[] { assetType.IsValidTypeString(), dir.IsValidDirectory(), FileExtensionValid(extensions), };
                string[] displayNames = new string[] { "Asset Type", "Objects Directory", "File Extension(s)" };
                const string prefix = "<b>{0} ::</b>\n\n\t<i>'{1}'</i>\n\n\tis empty or ";
                string[] invalidFieldMsgs = new string[] {
                    prefix + "does not exist in the current assembly!\n\tIf you're trying to target a Unity asset type or component,\n\ttry adding <i>'UnityEngine.'</i> before the asset type name.\n\n",
                    prefix + "does not exist in the project!\n\n",
                    prefix + "invalid.\n\n\tPlease specify file extensions to look for in the directory, seperated by commas.\n\t<b>E.G.:</b>\n\t\t<i>'.fbx, .wav, .mp3'</i>\n\n",
                };
                
                for (int i = 0; i < invalidFieldMsgs.Length; i++) {
                    if (!validChecks[i]) {
                        hlpMsg += string.Format(invalidFieldMsgs[i], displayNames[i], values[i]); 
                        hasError = true;
                    }
                }
                
                if (hasError)
                    err.Add(hlpMsg);
                else
                    noIDsCount = AssetObjectsEditor.GetAllAssetObjectPathsWithoutIDs(dir, extensions).Length;
                
                string dupName;
                if (packs.ContainsElementsWithDuplicateNames(out dupName)) wrns.Add( string.Format("\n\nThere are multiple packs named:\n\n\t<i>'{0}'</i>\n\n", dupName) );
                if (pack[defaultParametersField].ContainsElementsWithDuplicateNames(out dupName)) wrns.Add ( string.Format("\n\n<b>'{0}'</b> has multiple parameters named:\n\n\t<i>'{1}'</i>\n\n", name, dupName) );
                if (noIDsCount != 0) wrns.Add(string.Format("\n\n{0} [ '{1}' ] file(s) without proper IDs in the pack directory.\n\n\t<i>'{2}'</i>\n\n", noIDsCount, extensions, dir));
            }
            errors = err.ToArray();
            warnings = wrns.ToArray();
        }
        static bool FileExtensionValid(string extensions) {
            if (extensions.Contains(",")) {
                string[] split = extensions.Split(',');
                for (int i = 0; i < split.Length; i++) {
                    if (!FileExtensionValid(split[i])) return false;
                }
                return true;
            }
            return extensions.StartsWith(".");
        }

        static void GetValues (EditorProp pack, out string name, out string dir, out string extensions, out string assetType) {
            name = pack[nameField].stringValue;
            dir = pack[dirField].stringValue;
            extensions = pack[extensionsField].stringValue;
            assetType = pack[assetTypeField].stringValue;       
        }
        
        static bool PackNameValid(EditorProp packs, string name) {
            for (int i = 0; i < packs.arraySize; i++) {
                if (packs[i][nameField].stringValue == name) return false;
            }
            return true;
        }
        static void AdjustParametersToPack (EditorProp parameters, EditorProp pack, bool clear) {
            if (clear) CustomParameterEditor.ClearAndRebuildParameters(parameters, pack[defaultParametersField]);
            else CustomParameterEditor.UpdateParametersToReflectDefaults(parameters, pack[defaultParametersField]);
        }

        static void AddNewPackToPacksList (EditorProp packs) {
            int newID = AssetObjectsEditor.GenerateNewIDList(1, new HashSet<int>().Generate( packs.arraySize, i => packs[i][idField].intValue ))[0];

            string origName = "New Pack";
            string new_name = origName;
            int trying = 0;
            while (!PackNameValid(packs, new_name) && trying <= 999 ) {
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
            newPack[dirField].SetValue( string.Empty );
            newPack[assetTypeField].SetValue( string.Empty );
            newPack[extensionsField].SetValue( string.Empty );
        }
        public static void AdjustParametersToPack (EditorProp parameters, int packIndex, bool clear) {
            if (packIndex < 0) return;
            AdjustParametersToPack (parameters, PacksManagerEditor.GetPacksList()[packIndex], clear);    
        }
        static void GenerateIDsForPack (int packIndex) {
            string name, dir, extensions, assetType;
            GetValues(PacksManagerEditor.GetPacksList()[packIndex], out name, out dir, out extensions, out assetType);
            AssetObjectsEditor.GenerateNewIDs(AssetObjectsEditor.GetAllAssetObjectPaths( dir, extensions, false), AssetObjectsEditor.GetAllAssetObjectPathsWithoutIDs(dir, extensions));
        }
        public static void GetValues (int packIndex, out string name, out string dir, out string extensions, out string assetType) {
            GetValues(packIndex < 0 ? null : PacksManagerEditor.GetPacksList()[packIndex], out name, out dir, out extensions, out assetType);
        }

        public static class GUI {

            static bool showWarnings;

            static GUIContent[] GetNameGUIs (EditorProp packs) {
                return new GUIContent[packs.arraySize].Generate( i => new GUIContent(packs[i][nameField].stringValue) );
            }
            public static GUIContent[] GetDefaultParamNameGUIs(int packIndex) {
                EditorProp pack = PacksManagerEditor.GetPacksList()[packIndex];
                return new GUIContent[pack[defaultParametersField].arraySize].Generate( i => CustomParameterEditor.GUI.GetNameGUI(pack[defaultParametersField][i]) );
            }
            public static void DrawPacks (EditorProp packs, ref int packIndex) {
                GUIUtils.StartBox(1);
                EditorGUILayout.BeginHorizontal();
                if (packs.arraySize != 0) GUIUtils.Tabs(GetNameGUIs(packs), ref packIndex);
                if (GUIUtils.Button(new GUIContent("Add New Pack"), false, GUIStyles.toolbarButton, Colors.green, Colors.black)) AddNewPackToPacksList(packs);
                EditorGUILayout.EndHorizontal();
                GUIUtils.EndBox(1);
                if (packs.arraySize != 0) {
                    if (DrawPack(packs[packIndex])) packs.DeleteAt(packIndex);
                }
                if (packIndex >= packs.arraySize) packIndex = packs.arraySize-1;
            }
            static bool DrawPack(EditorProp pack){
                GUILayoutOption packsFieldsWidth = GUILayout.Width(100);

                GUIUtils.StartBox(1);
                //name
                GUIUtils.DrawTextProp(pack[nameField], new GUIContent("Pack Name"), packsFieldsWidth, false);
                //asset type
                GUIUtils.DrawTextProp(pack[assetTypeField], new GUIContent("Asset Type", "The asset type or component to the pack targets"), packsFieldsWidth, false);
                //directory
                GUIUtils.DrawDirectoryField(pack[dirField], new GUIContent("Objects Directory", "The directory where the assets are held"), packsFieldsWidth, true);
                //file extensions
                GUIUtils.DrawTextProp(pack[extensionsField], new GUIContent("File Extensions", "The file extensions to look for in the directory, seperated by commas.\nExample: '.fbx, .wav, .mp3'"), packsFieldsWidth, false);
                GUIUtils.EndBox(1);
                
                //default params
                GUIUtils.StartBox(0);
                GUIUtils.Label(new GUIContent ("Asset Object Default Parameters:"), false);    

                GUIUtils.StartBox(0, Colors.darkGray);
                CustomParameterEditor.GUI.DrawParamsList(pack[defaultParametersField], true, null, out _);
                GUIUtils.EndBox(0);
                
                GUIUtils.EndBox(0);      

                //delete pack
                bool delete = false;
                GUIUtils.StartBox(1);
                if (GUIUtils.Button(new GUIContent("Delete Pack"), false, GUIStyles.button, Colors.red, Colors.white )) {
                    if (EditorUtility.DisplayDialog("Delete Pack", "Are you sure you want to delete this pack?", "Delete Pack", "Cancel")) {
                        delete = true;
                    }
                }
                GUIUtils.EndBox(1);
                return delete;
            }

            public static bool DrawErrorsAndWarnings (string[] errors, string[] warnings, int noIDcount, int packIndex) {
                if (errors.Length == 0 && warnings.Length == 0) return false;
                bool genIDs = false;
                GUIUtils.StartBox(0);
                
                GUIStyle s = EditorStyles.helpBox;
                bool origRichText = s.richText;
                s.richText = true;
                
                for (int i = 0; i < errors.Length; i++) EditorGUILayout.HelpBox(errors[i], MessageType.Error);
                
                if (warnings.Length > 0) {

                    GUIContent c = new GUIContent( string.Format( "<b>{0}</b> Warnings ( {1} )", showWarnings ? " V " : " > ", warnings.Length ) );
                    if (GUIUtils.Button(c, true, GUIStyles.button, Colors.yellow, Colors.black)) {
                        showWarnings = !showWarnings;
                    }
                    if (showWarnings) {
                        for (int i = 0; i < warnings.Length; i++) EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
                        if (noIDcount != 0) {
                            if (GUIUtils.Button(new GUIContent("Generate IDs"), false, GUIStyles.button, Colors.green, Colors.black)) {
                                if (EditorUtility.DisplayDialog("Generate IDs", "Generating IDs will rename assets without IDs, are you sure?", "Generate IDs", "Cancel")) {
                                    GenerateIDsForPack(packIndex);
                                    genIDs = true;
                                }
                            }
                        }
                    }
                }
                s.richText = origRichText;
                GUIUtils.EndBox(0);
                return genIDs;
            }   
        }       
    }
}