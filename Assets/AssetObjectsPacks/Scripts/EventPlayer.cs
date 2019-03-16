using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class EventPlayer : MonoBehaviour
    {

        const int defaultPlaylistLayer = -1;


        public bool overrideMovement;
        Dictionary<string, CustomParameter> paramDict = new Dictionary<string, CustomParameter>();

        public void AddParameters (IEnumerable<CustomParameter> parameters) {
            foreach (var p in parameters) AddParameter(p);
        }
        public void AddParameter (CustomParameter parameter) {
            paramDict.Add(parameter.name, parameter);
        }
        public CustomParameter this [string paramName] {
            get {
                CustomParameter parameter;
                if (!paramDict.TryGetValue(paramName, out parameter)) {
                    Debug.LogWarning(paramName + " : param name doesnt exist");
                    return null;
                }
                return parameter;   
            }
        }

        public List<Playlist.Performance> current_playlists = new List<Playlist.Performance>();
        


       public delegate void PlayerMessage (AssetObject chosenObject, bool interrupter, HashSet<Action> onEendUseCallbacks);
        
        Dictionary<string, PlayerMessage> pack2playEvents = new Dictionary<string, PlayerMessage>();
                
        public void LinkAsPlayer(string packName, PlayerMessage onPlayEvent) {
            pack2playEvents[packName] = onPlayEvent;
        }


        public void SubscribeToPlayEnd(int layer, Action<bool> onPlayEnd) {
            GetUpdateLayer(layer).SubscribeToPlayEnd(onPlayEnd);
        }
        
        Dictionary<int, UpdateLayer> updateLayers = new Dictionary<int, UpdateLayer>();

        UpdateLayer GetUpdateLayer(int layer) {
            UpdateLayer updateLayer;
            if (!updateLayers.TryGetValue(layer, out updateLayer)) {
                updateLayer = new UpdateLayer();
                updateLayers[layer] = updateLayer;
            }
            return updateLayer;
        }

        class UpdateLayer {
            float duration_timer, current_duration = -1;
            bool endEventOverriden;
            HashSet<Action<bool>> endPlayCallbacks = new HashSet<Action<bool>>();
            Action<bool> endPlayAttemptCallback;
            
            //Dictionary<string, Event> overrideEvents = new Dictionary<string, Event>();
            HashSet<Event> additionalEvents = new HashSet<Event>();
            HashSet<string> skipPlays = new HashSet<string>();
            
            
            public void SkipPlay (string packName) {
                skipPlays.Add(packName);
                //OverrideEventToPlay(packName, null);
            }
            
            public void OverrideEventToPlay(string packName, Event overrideEvent) {
                additionalEvents.Add(overrideEvent);
                //overrideEvents.Add(packName, overrideEvent);
            }

            public void SubscribeToPlayEnd(Action<bool> onPlayEnd) {
                endPlayCallbacks.Add(onPlayEnd);
            }

            public void Update () {
                if (current_duration < 0 || endEventOverriden) 
                    return;
                duration_timer += Time.deltaTime;
                if (duration_timer >= current_duration) {
                    //Debug.Log("end event timer");
                    EndPlayAttempt(true);
                }
            }

            public void Interrupt () {
                if (!EndPlayAttempt(true)) {
                    EndPlay(true);
                }
                
            }
            void EndPlay (bool success) {
                //Debug.Log("endevent");
                
                foreach (var cb in endPlayCallbacks) {
                    cb(success);
                }
                
                endPlayCallbacks.Clear();
                //skipPlays.Clear();
                
                endEventOverriden = false;
                current_duration = -1;
                duration_timer = 0;

                endPlayAttemptCallback = null;
                //additionalEvents.Clear();
                //overrideEvents.Clear();
            }


            bool EndPlayAttempt (bool success) {

                if (!endEventOverriden) {
                    EndPlay(success);
                    return true;
                }
                if (endPlayAttemptCallback != null) {
                    endPlayAttemptCallback(success);
                    endPlayAttemptCallback = null;
                }
                return false;
            }
                


            public Action OverrideEndEvent (Action<bool> onEndAttempt) {
                if (endEventOverriden) {
                    Debug.LogWarning("trying to override end event thats already overriden");
                    return null;
                }
                //Debug.Log("overriding end event");
                endEventOverriden = true;
                endPlayAttemptCallback = onEndAttempt;
                //endPlayAttemptCallbacks.Add(onEndAttempt);
                return () => EndPlay(true);
            }

            public void PlayEvents_Cue (
                Dictionary<string, PlayerMessage> pack2playEvents,
                Dictionary<string, CustomParameter> paramDict, 
                Event[] events, float overrideDuration){
            
                //Debug.Log("playing cue");
                duration_timer = 0;
                //display options in cue editor
                bool asInterrupter = true;
                current_duration = overrideDuration;


                bool endPlayAttemptHandled = current_duration >= 0;

                
                //bool endEventHandled = endEventOverriden || current_duration >= 0;


                additionalEvents.AddRange(events);

                int l = additionalEvents.Count;
                bool successPlay = true;
                int i = 0;
                //for (int i = 0; i < l; i++) {
                foreach (var e in additionalEvents) {

                    
                    //bool success = _PlayEvent(pack2playEvents, paramDict, events[i], i == 0, asInterrupter, endEventHandled);
                    bool success = _PlayEvent(pack2playEvents, paramDict, e, i == 0, asInterrupter, endPlayAttemptHandled);
                    if (!success) {
                        successPlay = false;
                    }
                    i++;
                }
                additionalEvents.Clear();
                skipPlays.Clear();
                

                if (!successPlay) {
                    if (current_duration < 0) {
                        //Debug.Log("not succes end");
                    EndPlayAttempt(false);
                    }
                }
                else {
                    if (l == 0 && !endPlayAttemptHandled) {
                        Debug.Log("zero events not event handled early out");
                        EndPlayAttempt(true);
                    }
                }
        }
        public void PlayEvent (
            Dictionary<string, PlayerMessage> pack2playEvents,
            Dictionary<string, CustomParameter> paramDict, 
            Event e, float overrideDuration, bool asInterrupter){
                
            duration_timer = 0;
            current_duration = overrideDuration;
            bool endPlayAttemptHandled = current_duration >= 0;
            //bool endPlayAttemptHandled = endEventOverriden || current_duration >= 0;

            if (!_PlayEvent(pack2playEvents, paramDict, e, true, asInterrupter, endPlayAttemptHandled)) {
                EndPlayAttempt(false);
            }
            
        
        }

        
        bool _PlayEvent ( 
            Dictionary<string, PlayerMessage> pack2playEvents,
            Dictionary<string, CustomParameter> paramDict, 
            Event e, bool isMain, bool asInterrupter, bool endPlayAttemptHandled) {
        
            string packName = AssetObjectsManager.instance.packs.FindPackByID( e.assetObjectPackID, out _).name;

            bool skipPlay = skipPlays.Contains(packName);
            if (skipPlay) {
                skipPlays.Remove(packName);
                Debug.Log("skipping play");
                return true;   
            }

            //Event overrideEvent;
            //if (overrideEvents.TryGetValue(packName, out overrideEvent)) {
            //    overrideEvents.Remove(packName);

            //    if (overrideEvent == null) {
            //        Debug.Log("skipping play");
            //        return true;   
            //    }
            //    Debug.Log("overriding: " + e.name + " with: " + overrideEvent.name);
            //    e = overrideEvent;
            //}
            
            List<AssetObject> filteredList = e.GetParamFilteredObjects(paramDict);

            if (filteredList.Count == 0) {
                //Debug.Log("none found");
                return false;// !isMain;
            }
            
            AssetObject o = filteredList.RandomChoice();

            if (isMain && !endPlayAttemptHandled) {
            //if (!endEventHandled) {
            
                current_duration = o["Duration"].GetValue<float>();
            }

            HashSet<Action> endUseSuccessCBs = new HashSet<Action>();
            
            if (!endPlayAttemptHandled) {
                //give control to object when it's the main event
                //and when the duration is < 0 and not overriden
                
                if (current_duration < 0 && isMain) {

                    endUseSuccessCBs.Add( () => EndPlayAttempt(true) );
                }
            }
            //Debug.Log("playing player");
                
            pack2playEvents[packName](o, asInterrupter, endUseSuccessCBs);
            
            return true;   
        }
                
        
        
        }

        void Update () {
            UpdateLayers();
        }
        void UpdateLayers() {
            foreach (var k in updateLayers.Keys) {
                updateLayers[k].Update();
            }
        }
        

        public Action OverrideEndEvent (int layer, Action<bool> onEndAttempt) {
            return GetUpdateLayer(layer).OverrideEndEvent(onEndAttempt);
            
        }


        /*
            override with null to skip cue playback
        */
        public void SkipPlay (int layer, string packName) {
            GetUpdateLayer(layer).SkipPlay(packName);
        }
        public void InterruptLayer (int layer) {
            GetUpdateLayer(layer).Interrupt();
        }
        public void OverrideEventToPlay(int layer, string packName, Event overrideEvent) {
            GetUpdateLayer(layer).OverrideEventToPlay(packName, overrideEvent);
        }

        bool ChcekLinkedPlayers() {
            if (pack2playEvents.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return false;
            }
            return true;
        }

        public void PlayEvents_Cue (int layer, Event[] events, float overrideDuration){
            if (!ChcekLinkedPlayers()) return;
            GetUpdateLayer(layer).PlayEvents_Cue(pack2playEvents, paramDict, events, overrideDuration);
        }

        public void InterruptPerformances () {
            foreach (var p in current_playlists) {
                p.InterruptPerformance();
            }
            current_playlists.Clear();
        }
        public void PlayEvent (int layer, Event e, float overrideDuration, bool asInterrupter){
            if (!ChcekLinkedPlayers()) return;
            //bool interruptPlaylists = layer == -1;

            //if (interruptPlaylists) {
            //    InterruptPerformances();
            //}

            GetUpdateLayer(layer).PlayEvent(pack2playEvents, paramDict, e, overrideDuration, asInterrupter);
        }
    }
}