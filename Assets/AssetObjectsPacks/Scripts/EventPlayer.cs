using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class EventPlayer : MonoBehaviour
    {
        Dictionary<string, CustomParameter> param_dict = new Dictionary<string, CustomParameter>();

        public void AddParameters (IEnumerable<CustomParameter> parameters) {
            foreach (var p in parameters) AddParameter(p);
        }
        public void AddParameter (CustomParameter parameter) {
            param_dict.Add(parameter.name, parameter);
        }
        public CustomParameter this [string paramName] {
            get {
                CustomParameter parameter;
                if (!param_dict.TryGetValue(paramName, out parameter)) {
                    Debug.LogWarning(paramName + " : param name doesnt exist");
                    return null;
                }
                return parameter;   
            }
        }

        public List<Playlist.Performance> current_playlists = new List<Playlist.Performance>();
        Dictionary<string, Action<AssetObject, bool, List<Action>>> pack2playEvents = new Dictionary<string, Action<AssetObject, bool, List<Action>>>();
        Dictionary<string, List<Action>> pack2endUseCallbacks = new Dictionary<string, List<Action>>();
        Dictionary<string, List<Action>> pack2failEvents = new Dictionary<string, List<Action>>();
        
        public void LinkAsPlayer(string packName, Action<AssetObject, bool, List<Action>> onPlayEvent) {
            pack2playEvents[packName] = onPlayEvent;
        }

        void SubscribeToEvent(Dictionary<string, List<Action>> dict, string packName, Action action) {
            List<Action> actions;
            if (dict.TryGetValue(packName, out actions)) {
                actions.Add(action);
            }
            else {
                dict[packName] = new List<Action> () {action};
            }
        }
        public void SubscribeToAssetObjectUseEnd(string packName, Action onEndUse) {
            SubscribeToEvent(pack2endUseCallbacks, packName, onEndUse);
        }
        public void SubscribeToEventFail(string packName, Action onEventFail) {
            SubscribeToEvent(pack2failEvents, packName, onEventFail);
        }
        

        public Action cueEventEndCallback;

        float duration_timer, current_duration = -1;
        //public bool playing_event;
        
        void Update () {
            if (endAfterPlay) {
                EndEvent();
                endAfterPlay = false;
            }
            //if (!playing_event || current_duration < 0) //maybe if override end event as well
            if (current_duration < 0 || endEventOverriden) 
                return;

            duration_timer += Time.deltaTime;
            if (duration_timer >= current_duration) {
                Debug.LogError("END EVENT TIMER");
                EndEvent();
            }
        }
        void EndEvent () {

            //Debug.Log("Edne vent player");
            if (cueEventEndCallback != null) {
                //Debug.Log("end cue event");
                cueEventEndCallback();
                cueEventEndCallback = null;
            }
            endEventOverriden = false;
            //playing_event = false;
            current_duration = -1;
            overrideEvents.Clear();
        }

        bool endEventOverriden;


        public Action OverrideEndEvent () {
            endEventOverriden = true;
            return EndEvent;
        }
        bool endAfterPlay;
        public void EndAfterPlay () {
            endAfterPlay = true;
        }
        public void SkipPlay (string packName) {
            OverrideEventToPlay(packName, null);
        }


        Dictionary<string, Event> overrideEvents = new Dictionary<string, Event>();
        /*
            override with null to skip cue playback
        */
        public void OverrideEventToPlay(string packName, Event overrideEvent) {
            overrideEvents.Add(packName, overrideEvent);
        }

        bool ChcekLinkedPlayers() {
            if (pack2playEvents.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return false;
            }
            //playing_event = true;
            duration_timer = 0;
            return true;
        }

        public void PlayEvents_Cue (Event[] events, Action cueEventEndCallback){
            if (!ChcekLinkedPlayers()) return;

            //display options in cue editor
            bool asInterrupter = true;

            this.cueEventEndCallback = cueEventEndCallback;
            int l = events.Length;
            for (int i = 0; i < l; i++) _PlayEvent(events[i], i == 0, false, asInterrupter);
        }

        public void InterruptPerformances () {
            foreach (var p in current_playlists) {
                p.InterruptPerformance();
            }
            current_playlists.Clear();
        }
        //when no callbacks needed, just simple state switches, etc...
        public void PlayEvent (Event e, bool interruptPlaylists, bool simple, bool asInterrupter) {
            if (!ChcekLinkedPlayers()) return;
            if (interruptPlaylists) {
                InterruptPerformances();
            }
            _PlayEvent(e, true, simple, asInterrupter);
        }
     
        void _PlayEvent (Event e, bool isMain, bool simple, bool asInterrupter) {
            string packName = AssetObjectsManager.instance.packs.FindPackByID( e.assetObjectPackID, out _).name;

            Event overrideEvent;
            if (overrideEvents.TryGetValue(packName, out overrideEvent)) {
                overrideEvents.Remove(packName);

                if (!simple) {
                    if (overrideEvent == null) return;   
                    Debug.Log("overriding: " + e.name + " with: " + overrideEvent.name);
                    e = overrideEvent;
                }
            }
            
            List<AssetObject> filteredList = e.GetParamFilteredObjects(param_dict);
            if (CheckErrors(packName, filteredList.Count)) 
                return;
            
            AssetObject o = filteredList.RandomChoice();

            if (isMain && !endEventOverriden && !simple) {
                current_duration = o["Duration"].GetValue<float>();
                if (current_duration >= 0) {
                    Debug.Log(o.objRef.name + " set duration to " + current_duration);
                }

            }

            List<Action> onEndUseCallbacks;
            if (pack2endUseCallbacks.TryGetValue(packName, out onEndUseCallbacks)) 
                pack2endUseCallbacks.Remove(packName);
            else {
                if ( !simple ) {
                    onEndUseCallbacks = new List<Action>();
                }
            } 
                
            //give control to object when it's the main event
            //and when the duration is < 0 and not overriden

            if (!simple) {
                if (!endEventOverriden && current_duration < 0 && isMain) onEndUseCallbacks.Add(EndEvent);
            }
            
                
            pack2playEvents[packName](o, asInterrupter, onEndUseCallbacks);
            
        }
            
        bool CheckErrors(string packName, int listCount) {
            List<Action> onFailEvents = new List<Action>();
            if (pack2failEvents.TryGetValue(packName, out onFailEvents)) 
                pack2failEvents.Remove(packName);
            if (listCount == 0) {
                foreach (var fe in onFailEvents) 
                    fe();
                return true;
            }
            return false;
        }


        
    }
}

