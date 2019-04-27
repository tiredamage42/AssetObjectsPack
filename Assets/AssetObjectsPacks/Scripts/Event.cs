using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {


    public class EventResponse {
        public Dictionary<int, List<AssetObject>> objectsPerPack;
        public bool noMainFound { get { return objectsPerPack.ContainsKey(mainPackID) && objectsPerPack[mainPackID].Count == 0; } }
        
        int mainPackID;
        public EventResponse (HashSet<int> skipPlays) {
            PacksManager pm = AssetObjectsManager.instance.packs;
            int l = pm.packs.Length;
            objectsPerPack = new Dictionary<int, List<AssetObject>>(l);
            for (int i = 0; i < l; i++) {
                if (!skipPlays.Contains( pm.packs[i].id )) {
                    objectsPerPack.Add(pm.packs[i].id, new List<AssetObject>());
                }
            }
        }

        public void Respond (int mainPackID, string eventName) {
            this.mainPackID = mainPackID;
            this.eventName = eventName;
        }

        public string logErrors, logWarnings, eventName;

        #if UNITY_EDITOR
        public void LogErrors () {
            if (logErrors != null && !logErrors.IsEmpty()) {
                Debug.LogError(logErrors);
            }
        }
        public void LogWarnings () {
            if (noMainFound) {
                Debug.LogWarning("Couldnt find any main assets on: " + eventName);
                Debug.LogWarning(logWarnings);
            }
        }
        #endif
    }

    [System.Serializable] public class EventState {
        #if UNITY_EDITOR
        //for editor adding
        public bool isNew;
        public string name;
        #endif
    
        public string conditionBlock;
        public AssetObject[] assetObjects;
        
        public int[] subStatesIDs;
        public int stateID = -1;

        public void GetAssetObjects (EventResponse eventResponse,  Dictionary<string, CustomParameter> parameters) {
            int l = assetObjects.Length;
            foreach (var k in eventResponse.objectsPerPack.Keys) {

                for (int i = 0; i < l; i++) {
                    AssetObject ao = assetObjects[i];
                    if (ao.packID == k) {
                        if (ao.solo) {
                            eventResponse.objectsPerPack[k].Clear();
                            if (CustomScripting.StatementValue(ao.conditionBlock, parameters, ref eventResponse.logErrors, ref eventResponse.logWarnings)) {
                                eventResponse.objectsPerPack[k].Add(assetObjects[i]);
                            }
                            break;
                        }
                        if (!ao.mute) {
                            if (CustomScripting.StatementValue(ao.conditionBlock, parameters, ref eventResponse.logErrors, ref eventResponse.logWarnings)) {
                                eventResponse.objectsPerPack[k].Add(assetObjects[i]);
                            }
                        }
                    }
                }
            }
        }
    }


    [CreateAssetMenu(fileName = "New Asset Object Event", menuName = "Asset Objects Packs/Event", order = 2)]
    [System.Serializable] public class Event : ScriptableObject {

        #if UNITY_EDITOR
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  
        public Vector2Int[] hiddenIDs;
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
            eventResponse.Respond(mainPackID, this.name);
            GetFilteredStates(allStates[0], parameters, eventResponse);
        }
    }
}





