using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {

    [System.Serializable] public class EventState {
        #if UNITY_EDITOR
        //for editor adding
        public bool isNew;
        public string name;
        #endif
    
        public string conditionBlock;
        public AssetObject[] assetObjects;
        public EventState[] subStates;

        public List<AssetObject> GetAssetObjects () {
            List<AssetObject> r = new List<AssetObject>();
            int l = assetObjects.Length;
            for (int i = 0; i < l; i++) {

                if (assetObjects[i].solo) {
                    r.Clear();
                    r.Add(assetObjects[i]);
                    return r;
                }
                if (!assetObjects[i].mute) {
                    r.Add(assetObjects[i]);
                }
            }
            return r;

        }
    }


    [CreateAssetMenu(fileName = "New Asset Object Event", menuName = "Asset Objects Packs/Event", order = 2)]
    [System.Serializable] public class Event : ScriptableObject {

        #if UNITY_EDITOR
        public const string pack_id_field = "assetObjectPackID";
        public const string multi_edit_instance_field = "multi_edit_instance";
        public const string hiddenIDsField = "hiddenIDs";
        public const string baseStateField = "baseState";
        
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  
        public int[] hiddenIDs;
        #endif
    

        public EventState baseState;
        public int assetObjectPackID = -1;


        void GetFilteredStates (EventState eventState, Dictionary<string, CustomParameter> parameters, List<AssetObject> ret, bool debug) {
            if (CustomScripting.StatementValue(eventState.conditionBlock, parameters, debug, name)) {
                
                ret.AddRange(eventState.GetAssetObjects());
                
                int l = eventState.subStates.Length;
                for (int i = 0; i < l; i++) {
                    GetFilteredStates(eventState.subStates[i], parameters, ret, debug);
                }
            }
        }
        public List<AssetObject> GetParamFilteredObjects(Dictionary<string, CustomParameter> parameters) {
            List<AssetObject> ret = new List<AssetObject>();
            GetFilteredStates(baseState, parameters, ret, false);

            if (ret.Count == 0) {
                Debug.LogWarning("Couldnt find any assets on: " + this.name);
                //GetFilteredStates(baseState, parameters, ret, true);
                //Debug.Break();
            }
            return ret;
        }

    }
}





