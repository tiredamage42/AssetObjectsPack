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
        Dictionary<string, Action<AssetObject, List<Action>>> pack2playEvents = new Dictionary<string, Action<AssetObject, List<Action>>>();
        Dictionary<string, List<Action>> pack2endEvents = new Dictionary<string, List<Action>>();
        Dictionary<string, List<Action>> pack2failEvents = new Dictionary<string, List<Action>>();
        
        public void SubscribeToEventPlay(string packName, Action<AssetObject, List<Action>> onPlayEvent) {
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
        public void SubscribeToEventEnd(string packName, Action onEndEvent) {
            SubscribeToEvent(pack2endEvents, packName, onEndEvent);
        }
        public void SubscribeToEventFail(string packName, Action onEventFail) {
            SubscribeToEvent(pack2failEvents, packName, onEventFail);
        
        }

        public Action cueEventEndCallback;
        public float duration_timer, current_duration;
        //public bool playing_event;
        void Update () {
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


        Dictionary<string, Event> overrideEvents = new Dictionary<string, Event>();
        public void OverrideEventToPlay(string packName, Event overrideEvent) {
            overrideEvents.Add(packName, overrideEvent);
        }

        bool CheckPack2PlayEvents() {
            if (pack2playEvents.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return false;
            }
            //playing_event = true;
            duration_timer = 0;
            return true;
        }

        public void PlayEvents_Cue (Event[] events, Action cueEventEndCallback){
            if (!CheckPack2PlayEvents()) return;
            this.cueEventEndCallback = cueEventEndCallback;
            int l = events.Length;
            for (int i = 0; i < l; i++) _PlayEvent(events[i], i == 0);
        }

        public void InterruptPerformances () {
            for (int i = 0; i < current_playlists.Count; i++) {
                current_playlists[i].InterruptPerformance();
            }
            current_playlists.Clear();
        }
        public void PlayEvent (Event e, bool interruptPlaylists) {
            if (!CheckPack2PlayEvents()) return;
            if (interruptPlaylists) {
                InterruptPerformances();
            }


            _PlayEvent(e, true);
        }
            
        void _PlayEvent (Event e, bool isMain) {
            string packName = AssetObjectsManager.instance.packs.FindPackByID( e.assetObjectPackID, out _).name;
            e = CheckForOverride (packName, e);
            if (e == null) return;   
            List<AssetObject> filteredList = e.GetParamFilteredObjects(param_dict);
            if (CheckErrors(packName, filteredList.Count)) return;
            AssetObject o = filteredList.RandomChoice();

            if (isMain) current_duration = o["Duration"].FloatValue;

            List<Action> onEndEvents = new List<Action>();
            if (pack2endEvents.TryGetValue(packName, out onEndEvents)) pack2endEvents.Remove(packName);
                
            //give control to object when it's the main event
            //and when the duration is < 0 and not overriden
            
            if (!endEventOverriden && current_duration < 0 && isMain) onEndEvents.Add(EndEvent);                    
                
            pack2playEvents[packName](o, onEndEvents);
            
        }
            
        Event CheckForOverride (string packName, Event original) {
            Event overrideEvent;
            if (overrideEvents.TryGetValue(packName, out overrideEvent)) {
                overrideEvents.Remove(packName);
                if (overrideEvent == null) return null;   
                Debug.Log("overriding: " + original.name + " with: " + overrideEvent.name);
                return overrideEvent;
            }
            return original;
        }
        bool CheckErrors(string packName, int listCount) {
            List<Action> onFailEvents = new List<Action>();
            if (pack2failEvents.TryGetValue(packName, out onFailEvents)) {
                pack2failEvents.Remove(packName);
            }
            if (listCount == 0) {
                foreach (var fe in onFailEvents) fe();
                return true;
            }
            return false;
        }


        
    }
}

