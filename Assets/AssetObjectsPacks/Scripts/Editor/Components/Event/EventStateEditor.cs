using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace AssetObjectsPacks {

    public static class EventStateEditor 
    {

        
        public const string assetObjectsField = "assetObjects", nameField = "name", conditionsBlockField = "conditionBlock", subStatesField = "subStates";
        public const string isNewField = "isNew";

        public static EditorProp GetEventStateByPath(EditorProp baseEventState, string path) {
            if (path.IsEmpty()) return baseEventState;
            string[] split = path.Split('/');
            int l = split.Length;
            EditorProp lastState = baseEventState;
            for (int i = 0; i < l; i++) lastState = GetEventStateByName(lastState[subStatesField], split[i]);
            return lastState;
        }

        
        public static void ResetNewRecursive (EditorProp state, bool doSubs, int depth = 0) {
            state[isNewField].SetValue(false);
            if (doSubs || depth == 0) {
                for (int i = 0; i < state[subStatesField].arraySize; i++) ResetNewRecursive(state[subStatesField][i], doSubs, depth + 1);
            }            
        }

        public static EditorProp GetAOatPoolID (EditorProp state, int poolIndex) {
            return state[assetObjectsField][poolIndex - state[subStatesField].arraySize];
        }
        public static void DuplicateEventState(EditorProp parentState, int poolIndex) {
            EditorProp state = parentState[subStatesField][poolIndex];
            CopyEventState(parentState[subStatesField].InsertAtIndex(poolIndex + 1, state[nameField].stringValue + " Copy"), state, false, false );    
        }
        public static void DuplicateIndiciesInState (EditorProp parentState, HashSet<int> indicies) {

            foreach (var i in indicies) {
                //ao
                if (i >= parentState[subStatesField].arraySize) {
                    AssetObjectEditor.DuplicateAO (parentState[assetObjectsField], i - parentState[subStatesField].arraySize);
                }
                else {
                    DuplicateEventState(parentState, i);
                }
            }
        }

        public static void CopyList (EditorProp list, EditorProp copyList, System.Action<EditorProp, EditorProp> copyFn ) {
            list.Clear();
            for (int i = 0; i < copyList.arraySize; i++) copyFn(list.AddNew(), copyList[i]);
        }

        public static void CopyEventState(EditorProp es, EditorProp toCopy, bool doName, bool doAOs) {
            es.CopySubProps(toCopy, doName ? new string[] { nameField, conditionsBlockField, isNewField } : new string[] { conditionsBlockField, isNewField });
            if (doAOs) CopyList(es[assetObjectsField], toCopy[assetObjectsField], AssetObjectEditor.CopyAssetObject);
            es[subStatesField].Clear();
            for (int i = 0; i < toCopy[subStatesField].arraySize; i++) CopyEventState(es[subStatesField].AddNew(), toCopy[subStatesField][i], true, doAOs);
        }

        public static void GetValues(EditorProp state, int atIndex, out int id, out string elName, out bool isNewDir, out bool isCopy, out bool isSubstate) {
            isNewDir = false;
            elName = null;
            isSubstate = atIndex < state[subStatesField].arraySize;
            isCopy = false;
            if (isSubstate) {
                id = -1;
                isNewDir = state[subStatesField][atIndex][isNewField].boolValue;
                elName = state[subStatesField][atIndex][nameField].stringValue;
            }
            else {
                EditorProp ao = GetAOatPoolID(state, atIndex);
                id = AssetObjectEditor.GetID(ao);
                isCopy = AssetObjectEditor.GetIsCopy(ao);
            }
        }
        
        public static EditorProp GetEventStateByName ( EditorProp subStates, string name ) {
            for (int i = 0; i < subStates.arraySize; i++) {
                if (subStates[i][nameField].stringValue == name) return subStates[i];
            }
            Debug.LogError("couldnt find event state: " + name);
            return null;
        }
        
        static void MakeNewEventStateDefault (EditorProp newEventState) {
            newEventState[isNewField].SetValue(true);
            newEventState[conditionsBlockField].SetValue("");
            newEventState[subStatesField].Clear();
            newEventState[assetObjectsField].Clear();
        }
        public static void CheckAllAOsForNullObjects (EditorProp eventState, System.Func<int, Object> getObjForID ) {
            for (int i = 0; i < eventState[assetObjectsField].arraySize; i++) {
                AssetObjectEditor.CheckForNullObject(eventState[assetObjectsField][i], getObjForID);
            }
            for (int i = 0; i < eventState[subStatesField].arraySize; i++) {
                CheckAllAOsForNullObjects(eventState[subStatesField][i], getObjForID);      
            }
        }
        public static void UpdatEventStatesAgainstDefaults(EditorProp state, int packIndex) {
            for (int i = 0; i < state[assetObjectsField].arraySize; i++) {
                AssetObjectEditor.MakeAssetObjectDefault(state[assetObjectsField][i], packIndex, false);
            }

            for (int i = 0; i < state[subStatesField].arraySize; i++) {

                UpdatEventStatesAgainstDefaults(state[subStatesField][i], packIndex);   
            }
        }

        public static void ResetBaseState (EditorProp baseEventState) {
            baseEventState[assetObjectsField].Clear();
            baseEventState[subStatesField].Clear();
        }
        public static int GetEventTotalCount (EditorProp state) {
            return state[assetObjectsField].arraySize + state[subStatesField].arraySize;
        }
        
        //for building IDs in set ignore for project view build
        public static void GetAllEventIDs (EditorProp state, bool useRepeats, HashSet<int> ret) {
            
            for (int i = 0; i < state[assetObjectsField].arraySize; i++) {
                int id = AssetObjectEditor.GetID(state[assetObjectsField][i]);
                if (!useRepeats  && ret.Contains(id)) continue;
                ret.Add( id );
            }
            for (int i = 0; i < state[subStatesField].arraySize; i++) GetAllEventIDs(state[subStatesField][i], useRepeats, ret);
        }
        public static void GetAllEventIDs (EditorProp baseEventState, string atDir, bool useRepeats, HashSet<int> ret) {
            GetAllEventIDs(GetEventStateByPath(baseEventState, atDir), useRepeats, ret);
        }
                
        public static bool AddIDsToState (EditorProp state, IEnumerable<int> ids, int packIndex, System.Func<int, Object> GetObjectRefForID) {
            if (ids.Count() == 0) return false;            
            bool reset_i = true;            
            foreach (var id in ids) {
                AssetObjectEditor.InitializeNewAssetObject(state[assetObjectsField].AddNew(), id, GetObjectRefForID(id), reset_i, packIndex);    
                reset_i = false;
            }
            return true;
        }
        
        public static void NewEventState (EditorProp parentState) {
            ResetNewRecursive(parentState, false);
            EditorProp newState = parentState[subStatesField].AddNew("New Event State");
            MakeNewEventStateDefault(newState);
        }
        public static bool DeleteIndiciesFromState (EditorProp baseState, EditorProp state, IEnumerable<int> deleteIndicies) {                    
            if (deleteIndicies.Count() == 0) return false;
            int deleteOption = -1;
            for (int i = GetEventTotalCount(state) - 1; i >= 0; i--) {
                if (deleteIndicies.Contains(i)) {
                    if (i >= state[subStatesField].arraySize) state[assetObjectsField].DeleteAt(i - state[subStatesField].arraySize);
                    else DeleteState(baseState, state, state[subStatesField][i], ref deleteOption);
                }
            }
            return true;
        }
                
        static void AddAOsAndSubAOsToBaseList(EditorProp baseState, EditorProp state) {
            for (int i = 0; i < state[assetObjectsField].arraySize; i++) {
                AssetObjectEditor.CopyAssetObject(baseState[assetObjectsField].AddNew(), state[assetObjectsField][i]);
            }
            for (int i = 0; i < state[subStatesField].arraySize; i++) {
                AddAOsAndSubAOsToBaseList(baseState, state[subStatesField][i]);
            }
        }
        static bool DeleteState (EditorProp baseState, EditorProp parentState, EditorProp state, ref int preDeleteSelection) {
            if (preDeleteSelection == -1) {
                preDeleteSelection = EditorUtility.DisplayDialogComplex(
                    "Delete State", 
                    "Delete state(s) and asset objects? If keeping asset objects they will be moved to base state", 
                    "Delete And Keep", "Cancel", "Delete All"
                );
            }
            switch(preDeleteSelection) {
                case 1: return false;
                //Debug.Log("Deleted All");
                case 2: break;
                //Debug.Log("Deleted Keep");
                case 0: AddAOsAndSubAOsToBaseList(baseState, state); break;
            }

            int atIndex = -1;
            for (int i = 0; i < parentState[subStatesField].arraySize; i++) {
                if (parentState[subStatesField][i][nameField].stringValue == state[nameField].stringValue) {
                    atIndex = i;
                    break;
                }
            }
            parentState[subStatesField].DeleteAt(atIndex);


            return true;
        }

        public static void QuickRenameNewEventState (EditorProp parentState, int index, string newName) {
            EditorProp newState = parentState[subStatesField][index];

            newState.SetChanged();

            newState[isNewField].SetValue(false);
            //Debug.Log("set new false");

            if (newName.Contains("*")) {
                string[] split = newName.Split('*');
                newState[nameField].SetValue(split[0]);
                newState[conditionsBlockField].SetValue(split[1]);
            }
            else newState[nameField].SetValue(newName);


        }
        
        public static void MoveAOsToEventState(EditorProp baseState, IEnumerable<int> indicies, string origDir, string targetDir)
        {
            if (indicies.Count() == 0) return;
            //Debug.Log("moving selection from " + origDir + " to " + targetDir);
            EditorProp origState = GetEventStateByPath(baseState, origDir);
            EditorProp targState = GetEventStateByPath(baseState, targetDir);
            for (int i = GetEventTotalCount(origState) - 1; i >= 0; i--) {
                if (indicies.Contains(i)) {
                    if (i >= origState[subStatesField].arraySize) {
                        int aoIndex = i-origState[subStatesField].arraySize;
                        AssetObjectEditor.CopyAssetObject(targState[assetObjectsField].AddNew(), origState[assetObjectsField][aoIndex]);
                        origState[assetObjectsField].DeleteAt(aoIndex);
                    }
                    else {
                        int stateIndex = i;
                        CopyEventState(targState[subStatesField].AddNew(), origState[subStatesField][stateIndex], true, true);
                        origState[subStatesField].DeleteAt(stateIndex);
                    }
                }
            }
        }


        public static class GUI {
            static readonly GUIContent muteGUI = new GUIContent("", "Mute");
            static readonly GUIContent soloGUI = new GUIContent("", "Solo");

            public static void DrawEventStateSoloMuteElement (EditorProp state, int poolIndex, Color32 soloOn, Color32 soloOff, Color32 muteOn, Color32 muteOff) {
                
                if (poolIndex < state[subStatesField].arraySize) {
                    return;
                }

                int i = poolIndex - state[subStatesField].arraySize;
                EditorProp ao = state[assetObjectsField][i];

                bool changedMute;

                bool newMute = GUIUtils.SmallToggleButton(muteGUI, AssetObjectEditor.GetMute(ao), muteOn, muteOff, out changedMute );
                if (changedMute) {
                    AssetObjectEditor.SetMute(ao, newMute);
                    if (newMute) AssetObjectEditor.SetSolo(ao, false);
                }
                bool changedSolo;
                bool newSolo = GUIUtils.SmallToggleButton(soloGUI, AssetObjectEditor.GetSolo(ao), soloOn, soloOff, out changedSolo );
                if (changedSolo) {
                    AssetObjectEditor.SetSolo(ao, newSolo);
                    if (newSolo) {
                        AssetObjectEditor.SetMute(ao, false);
                        for (int x = 0; x < state[assetObjectsField].arraySize; x++) {
                            if (x == i) continue;
                            AssetObjectEditor.SetSolo(state[assetObjectsField][x], false);
                        }
                    }
                }
                
            }
            
            static bool DrawStateName (EditorProp state, bool drawingBase, out string changeName) {
                UnityEngine.GUI.enabled = !drawingBase;
                if (drawingBase) {
                    state[nameField].SetValue("Base State");
                    state[conditionsBlockField].SetValue("");
                }
                bool changedName = GUIUtils.DrawTextProp(state[nameField], new GUIContent("State Name"), GUIUtils.TextFieldType.Delayed, true, GUILayout.Width(64));
                UnityEngine.GUI.enabled = true;
                changeName = state[nameField].stringValue;
                return changedName;
            }
                
            public static void DrawEventState(EditorProp baseState, EditorProp parentState, EditorProp state, out bool deletedState, out bool changedName, out string changeName) {
                
                bool drawingBase = parentState == null;
                
                EditorGUILayout.BeginHorizontal();
                
                //name
                changedName = DrawStateName (state, drawingBase, out changeName);
                //delete button
                deletedState = !drawingBase && GUIUtils.SmallDeleteButton("Delete State");
                
                EditorGUILayout.EndHorizontal();

                //conditions
                if (!drawingBase) {
                    GUIUtils.Space();
                    GUIUtils.DrawMultiLineStringProp(state[conditionsBlockField], new GUIContent("Conditions Block"), true, GUILayout.MinHeight(32));
                }
                
                if (deletedState) {
                    int deleteOption = -1;
                    deletedState = DeleteState(baseState, parentState, state, ref deleteOption);
                }
            }
        }
    }
}