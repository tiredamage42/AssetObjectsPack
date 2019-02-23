using UnityEngine;

namespace AssetObjectsPacks {
    public static class EventHelp {
        const string packTypeHelp = @"
            <b>PACK TYPE</b>
                Click on the <b>Change Pack Type</b> button at the top of the Event inspector
                to change the pack the event uses.

                <b>NOTE:</b>
                Changing the pack will reset the event.
        ";
        const string toolbarHelp = @"
            <b>TOOLBAR</b>

            <b>View Tabs:</b>
                Click on the 'Event' tab to view the Asset Objects included in the event.
                Click on the 'Project' tab to view the Asset Objects in the directory specified in the pack.

            <b>Buttons:</b>
                
                <b>Import Settings:</b>
                    Opens import settings editing for selection (if any).
                
                <b>Add/Remove:</b>
                    Remove selection (if any) from Event (In Event View).
                    Add selection (if any) to Event (In Project View).
                
                <b>Hide/Unhide:</b>
                    Hide / Unhide the selection (if any).
                
                <b>Show Hidden:</b>
                    Show hidden elements. Their names will be displayed with
                    yellow, italic text.
                
                <b>Reset Hidden:</b>
                    Unhide all hidden elements.
                
                <b>Folders:</b>
                    show elements in a foldered setup.
                    Reflects the Unity project directory setup of the objects directory of the pack.

            <b>Search:</b>
                Type to search in all object file paths (non case sensitive).
                press 'Return' or 'Enter' when done.

            <b>Current Directory (Foldered View):</b>
                Displays current directory.
                Click the back button to go to the previous directory.
        ";
        const string elementHelp = @"
            <b>ASSET OBJECTS:</b>

            Click on the name to select an Asset Object.

            <b>Parameters and Conditions: (Event View)</b>
                Click the <b>'P'</b> next to each Asset Object to show and edit its parameters
                Click the <b>'C'</b> next to each Asset Object to show and edit its conditions
            
                <b>NOTE:</b> 
                clicking either on the Multi-Editing toolbar will open parameters (or conditions)
                for selection (if any, all shown if no selection).

            <b>Conditions:</b>
                When an Event Player plays an event, 
                each Asset Object will be available for random selection when:

                    1.  it has no conditions
                    2.  if at least one of its conditions are met.

                A condition is considered met when all of the condition's parameters match 
                the corresponding named parameter on the player

                conditions are 'or's, parameters in each conditon are 'and's.
        ";
        const string multi_edit_help = @"
            <b>MULTI-EDITING (EVENT VIEW):</b>

            To multi edit a parameter, change it in the Multi-Object Editing box (at the top).
            then click the button to the right of the parameter name

            if no elements are selected, changes will be made to all shown elements.
            
            <b>Multi-Editing Conditions:</b>
                <b>'Add'</b> adds the changed conditions list to the each asset object's conditions list.
                <b>'Replace'</b> replaces each asset object's conditions with the changed conditions list.
        ";
        const string controls_help = @"
            <b>KEYBOARD CONTROLS:</b>

            <b>[ Shift ] / [ Ctrl ] / [ Cmd ]</b> 
                Multiple selections

            <b>[ Del ] / [ Backspace ]</b> 
                Remove selection (if any) from list (In Event View).
            
            <b>[ Enter ] / [ Return ]</b> 
                Add selection (if any) to list (In Project View).
            
            <b>[ H ]</b> 
                Hide / Unhide selection (if any).
            
            <b>[ I ]</b> 
                Open import settings editing for selection (if any).
            
            <b>Arrows:</b>
            <b>[ Left ]</b> 
                Page Back ( Back directory when page 0 and in foldered view ).
            
            <b>[ Right ]</b> 
                Page Fwd.
            
            <b>[ Up ] / [ Down ]</b> 
                Scroll selection
        ";


        public static readonly GUIContent[] helpTabsGUI = new GUIContent[] {
            new GUIContent("Pack Type"),
            new GUIContent("Toolbar"),
            new GUIContent("AssetObjects"),
            new GUIContent("Multi-Editing"),
            new GUIContent("Keyboard Controls"),
        };
        public static readonly string[] helpTexts = new string[] { 
            packTypeHelp, toolbarHelp, elementHelp, multi_edit_help, controls_help,
        };
            















    }
}
