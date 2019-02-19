using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace AssetObjectsPacks {
    public static class HelpGUI {
        const string controls_help = @"
            <b>CONTROLS:</b>

            Click on the name to select an element.

            <b>[ Shift ] / [ Ctrl ] / [ Cmd ]</b> Multiple selections
            <b>[ Del ] / [ Backspace ]</b> Delete selection from list (In List View).
            <b>[ Enter ] / [ Return ]</b> Add selection to list (In Explorer View).
            <b>[ H ]</b> Hide / Unhide selection.
            
            <b>Arrows:</b>
            <b>[ Left ]</b> Page Back ( Folder View Back when page 0 ).
            <b>[ Right ]</b> Page Fwd.
            <b>[ Up ] / [ Down ]</b> Scroll selection
        ";
        const string conditions_help = @"
            <b>CONDITIONS:</b>

            When an Event Player plays an event, 
            each Asset Object will be available for random selection when:

                1.  it has no conditions
                2.  if at least one of its conditions are met.

            A condition is considered met when all of the condition's parameters match 
            the corresponding named parameter on the player

            conditions are 'or's, parameters in each conditon are 'and's.
                
        ";
        const string multi_edit_help = @"
            <b>MULTI-EDITING (LIST VIEW):</b>

            To multi edit a parameter, change it in the multi edit box (at the top).
            then click the blue button to the right of the parameter name

            if no elements are selected, changes will be made to all shown elements.
            
            <b>When multi editing conditions:</b>

            The <b>'Add'</b> button adds the changed conditions list to the each asset objects' conditions list.
            The <b>'Replace'</b> button replaces each asset object's conditions with the changed conditions list.


        ";


        static readonly GUIContent[] helpTabsGUI = new GUIContent[] {
            new GUIContent("Help: Controls"),
            new GUIContent("Help: Conditions"),
            new GUIContent("Help: Multi-Editing"),
        };
        static readonly string[] helpTexts = new string[] { 
            controls_help, conditions_help, multi_edit_help 
        };
        static readonly GUIContent genIDsGUI = new GUIContent(sGenerateIDs);
        
        const string dupPackNamesHelp = "\n\nThere are multiple packs named:\n\n\t<i>'{0}'</i>\n\n";
        const string dupParametersHelp = "\n\n<b>'{0}'</b> has multiple parameters named:\n\n\t<i>'{1}'</i>\n\n";
        const string nullManagerHelp = "\n\nPacks Manager Object could not be found!\n\nIf it was deleted, create a new one.\n\n( Right Click in the Unity project window ->\nCreate -> Asset Objects Packs -> Packs Manager )\n\n";
        const string packNullError = "\n\nPlease choose or create an Asset Object Pack\n\n";
        const string packNameHeader = "\n\n<b>[ {0} ]</b>\n\n";
        const string fieldInvalidPrefix = "<b>{0} ::</b>\n\n\t<i>'{1}'</i>\n\n\tis empty or ";
        const string assetTypeInvalidMsg = fieldInvalidPrefix + "does not exist in the current assembly!\n\tIf you're trying to target a Unity asset type or component,\n\ttry adding <i>'UnityEngine.'</i> before the asset type name.\n\n";
        const string directoryInvalidMsg = fieldInvalidPrefix + "does not exist in the project!\n\n";
        const string extensionsInvalidMsg = fieldInvalidPrefix + "invalid.\n\n\tPlease specify file extensions to look for in the directory, seperated by commas.\n\t<b>E.G.:</b>\n\t\t<i>'.fbx, .wav, .mp3'</i>\n\n";
        const string genIDsHelpString = "\n\n{0} [ '{1}' ] file(s) without proper IDs in the pack directory.\n\n\t<i>'{2}'</i>\n\n";
        const string showWarningsTxt = "<b>{0}</b> Warnings ( {1} )";
        const string sGenerateIDs = "Generate IDs", sCancel = "Cancel";
        const string genIDsSureMsg = "Generating IDs will rename assets without IDs, are you sure?";
        static int helpTab;
        static bool showWarnings;

        public static void DrawToolBarHelp (int space) {
            GUIUtils.Space(space);
            GUIStyle s = EditorStyles.helpBox;
            bool ort = s.richText;
            s.richText = true;
            EditorGUILayout.TextArea(helpTexts[helpTab], s);
            s.richText = ort;
            GUIUtils.Tabs(helpTabsGUI, ref helpTab, true);
            GUIUtils.Space(space);
        }        
        
        public static string[] GetErrorStrings (EditorProp packs, EditorProp pack) {

            List<string> helps = new List<string>();
            if (packs == null) {
                helps.Add(nullManagerHelp);
            }
            if (pack == null) {
                helps.Add(packNullError);            
            }
            else {
                string objectsDirectory = pack[AssetObjectPack.objectsDirectoryField].stringValue;
                string fileExtensions = pack[AssetObjectPack.fileExtensionsField].stringValue;
                string assetType = pack[AssetObjectPack.assetTypeField].stringValue;
                string hlpMsg = string.Format(packNameHeader, pack[AssetObjectPack.nameField].stringValue);
                bool used = false;
                if (!assetType.IsValidTypeString()) {
                    used = true;
                    hlpMsg += string.Format(assetTypeInvalidMsg, "Asset Type", assetType); 
                }
                if (!objectsDirectory.IsValidDirectory()) {
                    used = true;
                    hlpMsg += string.Format(directoryInvalidMsg, "Objects Directory", objectsDirectory) ;            
                }
                if (!FileExtensionValid(fileExtensions)) {
                    used = true;
                    hlpMsg += string.Format(extensionsInvalidMsg, "File Extension(s)", fileExtensions) ;            
                }
                if (used) {
                    helps.Add(hlpMsg);
                }
            }
            return helps.ToArray();
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

        public static string[] GetWarningStrings (EditorProp packs, EditorProp pack, string[] pathsWithoutIDs) {

            List<string> helps = new List<string>();
            if (pack != null){
                string dupName;

                if (packs.ContainsDuplicateNames(out dupName, AssetObjectPack.nameField)) {
                    helps.Add( string.Format(dupPackNamesHelp, dupName));
                }

                if (pack[AssetObjectPack.defaultParametersField].ContainsDuplicateNames(out dupName, CustomParameter.nameField)) {
                    string name = pack[AssetObjectPack.nameField].stringValue;
                    helps.Add( string.Format(dupParametersHelp, name, dupName));
                }
                int l = pathsWithoutIDs.Length;
                if (l != 0) {
                    string objectsDirectory = pack[AssetObjectPack.objectsDirectoryField].stringValue;
                    string fileExtensions = pack[AssetObjectPack.fileExtensionsField].stringValue;
                    helps.Add(string.Format(genIDsHelpString, l, fileExtensions, objectsDirectory));
                }
            }
            

            return helps.ToArray();
        }


        
        public static void DrawErrorsAndWarnings (string[] errors, string[] warnings, string[] pathsWithoutIDs, out bool generateNewIDs) {
            generateNewIDs = false;
            if (errors.Length == 0 && warnings.Length == 0) return;
            
            GUIUtils.StartBox(1);
            
            GUIStyle s = EditorStyles.helpBox;
            bool origRichText = s.richText;
            s.richText = true;
            
            for (int i = 0; i < errors.Length; i++) EditorGUILayout.HelpBox(errors[i], MessageType.Error);
            
            if (warnings.Length > 0) {

                GUIContent c = new GUIContent( string.Format( showWarningsTxt, showWarnings ? " V " : " > ", warnings.Length ) );
                if (GUIUtils.Button(c, true, GUI.skin.button, EditorColors.yellow_color, EditorColors.black_color)) {
                    showWarnings = !showWarnings;
                }
                if (showWarnings) {
                    for (int i = 0; i < warnings.Length; i++) {
                        EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
                    }
                    if (pathsWithoutIDs.Length != 0) {
                        if (GUIUtils.Button(genIDsGUI, false, GUI.skin.button, EditorColors.green_color, EditorColors.black_color)) {
                            generateNewIDs = EditorUtility.DisplayDialog(sGenerateIDs, genIDsSureMsg, sGenerateIDs, sCancel);
                        }
                        
                    }
                }
            }
            s.richText = origRichText;
            GUIUtils.EndBox(1);
        }
    }
}