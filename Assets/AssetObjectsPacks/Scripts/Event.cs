using UnityEngine;
using System.Collections.Generic;
namespace AssetObjectsPacks {

    public class EventResponse {
        bool noneFound { get { return chosenObjects.Count == 0; } }
        public List<AssetObject> chosenObjects = new List<AssetObject>();

        public void Respond (string eventName) {
            this.eventName = eventName;
        }

        public string logErrors, logWarnings, eventName;

        public void LogErrors () {
        #if UNITY_EDITOR
            if (logErrors != null && !logErrors.IsEmpty()) {
                Debug.LogError(logErrors);
            }
        #endif
        }
        public void LogWarnings () {
        #if UNITY_EDITOR
            if (noneFound) {
                Debug.LogWarning("Couldnt find any main assets on: " + eventName);
            }
            if (logWarnings != null && !logWarnings.IsEmpty()) {
                Debug.LogWarning(logWarnings);
            }
        #endif
        }
    }

    [System.Serializable] public class EventState {
        #if UNITY_EDITOR
        //for editor adding
        public bool isNew;
        public string name;

        // public int parentID = -1;
        // public string fullPath;
        #endif
    
        public string conditionBlock;
        public AssetObject[] assetObjects;
        
        public int[] subStatesIDs;
        public int stateID = -1;

        public void GetAssetObjects (EventResponse eventResponse,  Dictionary<string, CustomParameter> parameters) {
            int l = assetObjects.Length;

            for (int i = 0; i < l; i++) {
                AssetObject ao = assetObjects[i];
                if (ao.solo) {
                    eventResponse.chosenObjects.Clear();
                    
                    if (CustomScripting.StatementValue(ao.conditionBlock, parameters, ref eventResponse.logErrors, ref eventResponse.logWarnings)) {
                        eventResponse.chosenObjects.Add(assetObjects[i]);
                    }
                    
                    break;
                }
                if (!ao.mute) {
                    
                    if (CustomScripting.StatementValue(ao.conditionBlock, parameters, ref eventResponse.logErrors, ref eventResponse.logWarnings)) {
                        eventResponse.chosenObjects.Add(assetObjects[i]);
                    }
                }
            }
        }
    }


    [CreateAssetMenu(fileName = "New Asset Object Event", menuName = "Asset Objects Packs/Event", order = 2)]
    [System.Serializable] public class Event : ScriptableObject {

        

        #if UNITY_EDITOR
        public int[] hiddenIDs;
        #endif

        public EventState[] allStates;
        public int mainPackID = -1;

        Dictionary<int, int> _id2State;
        Dictionary<int, int> id2State {
            get {
                if (_id2State == null || _id2State.Count == 0) {
                    _id2State = new Dictionary<int, int>();
                    for (int i = 0; i < allStates.Length; i++) {
                        _id2State.Add(allStates[i].stateID, i);
                    }
                }
                return _id2State;
            }
        }
        



        void GetFilteredStates (EventState eventState, Dictionary<string, CustomParameter> parameters, EventResponse eventResponse){
            
            if (CustomScripting.StatementValue(eventState.conditionBlock, parameters, ref eventResponse.logErrors, ref eventResponse.logWarnings)) {
                
                eventState.GetAssetObjects(eventResponse, parameters);
                int l = eventState.subStatesIDs.Length;
                for (int i = 0; i < l; i++) {
                    GetFilteredStates(allStates[id2State[eventState.subStatesIDs[i]]], parameters, eventResponse);
                }
            }
        }
        
        public void GetParamFilteredObjects(Dictionary<string, CustomParameter> parameters, EventResponse eventResponse) {            
            eventResponse.Respond(this.name);
            GetFilteredStates(allStates[0], parameters, eventResponse);
        }
    }
}





