using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class EventPlayer : MonoBehaviour
    {
        public class EventPlayEnder {
            public Action cb;
            public void EndPlay () {
                if (cb != null) {
                    cb();
                    cb = null;
                }
            }
            public EventPlayEnder (Action cb) { this.cb = cb; }
        }

        [HideInInspector] public bool cueMoving;
        
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
        public bool AttemptedEnd(int layer) {
            return GetUpdateLayer(layer).attemptedEnd;
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
            bool endPlayOverriden;
            public bool attemptedEnd;

            Action<bool> endPlayAttemptCallback;
            HashSet<Action<bool>> endPlayCallbacks = new HashSet<Action<bool>>();
            HashSet<Event> additionalEvents = new HashSet<Event>();
            HashSet<string> skipPlays = new HashSet<string>();
            
            
            public void SkipPlay (string packName) {
                skipPlays.Add(packName);
            }
            
            public void OverrideEventToPlay(Event overrideEvent) {
                additionalEvents.Add(overrideEvent);
            }

            public void SubscribeToPlayEnd(Action<bool> onPlayEnd) {
                endPlayCallbacks.Add(onPlayEnd);
            }

            public void Update () {
                if (current_duration < 0 || endPlayOverriden) 
                    return;
                duration_timer += Time.deltaTime;
                if (duration_timer >= current_duration) {
                    //Debug.Log("end event timer");
                    EndPlayAttempt(true);
                }
            }

            public void Interrupt () {
                //returns false if end play overriden
                if (!EndPlayAttempt(true)) {
                    //take away control from last play ender
                    lastPlayEnder.cb = null;
                    EndPlay(true);
                }
                
            }
            void EndPlay (bool success) {

                attemptedEnd = false;
                
//                Debug.Log("endevent");
                foreach (var cb in endPlayCallbacks) {
                    cb(success);
                }
                endPlayCallbacks.Clear();
                endPlayOverriden = false;
                current_duration = -1;
                duration_timer = 0;

                endPlayAttemptCallback = null;
                lastPlayEnder = null;
            }


            bool EndPlayAttempt (bool success) {
                attemptedEnd = true;

                if (!endPlayOverriden) {
                    EndPlay(success);
                    return true;
                }

                if (endPlayAttemptCallback != null) {
                    endPlayAttemptCallback(success);
                    endPlayAttemptCallback = null;
                }
                return false;
            }


            
                
            EventPlayEnder lastPlayEnder;

            string endPlayOverrideReason;
            public EventPlayEnder OverrideEndPlay (Action<bool> onEndAttempt, string endPlayOverrideReason) {
                if (endPlayOverriden) {
                    Debug.LogWarning(endPlayOverrideReason + " is trying to override end event thats already overriden by: " + this.endPlayOverrideReason);
                    return null;
                }
                //Debug.Log("overriding end event");
                endPlayOverriden = true;
                this.endPlayOverrideReason = endPlayOverrideReason;
                endPlayAttemptCallback = onEndAttempt;

                lastPlayEnder = new EventPlayEnder(
                    () => EndPlay(true)
                );

                return lastPlayEnder;
                //return () => EndPlay(true);
            }

            public void PlayEvents (
                Dictionary<string, PlayerMessage> pack2playEvents,
                Dictionary<string, CustomParameter> paramDict, 
                Event[] events, float overrideDuration, bool asInterrupter){

                    attemptedEnd = false;
            
                //Debug.Log("playing cue");
                duration_timer = 0;
                current_duration = overrideDuration;


                bool endPlayAttemptHandled = current_duration >= 0;

                additionalEvents.AddRange(events);

                int l = additionalEvents.Count;
                bool successPlay = true;
                int i = 0;
                foreach (var e in additionalEvents) {                    
                    bool success = PlayEvent(pack2playEvents, paramDict, e, i == 0, asInterrupter, endPlayAttemptHandled);
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
                        //if (endPlayOverriden) {
                        //    Debug.Log("reason " + endPlayOverrideReason);
                        //}
                        
                        //Debug.Log("zero events not event handled early out");
                        EndPlayAttempt(true);
                    }
                }
        }

        
        bool PlayEvent ( 
            Dictionary<string, PlayerMessage> pack2playEvents,
            Dictionary<string, CustomParameter> paramDict, 
            Event e, bool isMain, bool asInterrupter, bool endPlayAttemptHandled) {
        
            string packName = AssetObjectsManager.instance.packs.FindPackByID( e.assetObjectPackID, out _).name;

            bool skipPlay = skipPlays.Contains(packName);
            if (skipPlay) {
                skipPlays.Remove(packName);
                //Debug.Log("skipping play");
                return true;   
            }

            List<AssetObject> filteredList = e.GetParamFilteredObjects(paramDict);

            if (filteredList.Count == 0) {
                //Debug.Log("none found");
                return false;// !isMain;
            }
            
            AssetObject o = filteredList.RandomChoice();

            if (isMain && !endPlayAttemptHandled) {
                current_duration = o["Duration"].GetValue<float>();
            }

            HashSet<Action> endUseSuccessCBs = new HashSet<Action>();
            
            if (!endPlayAttemptHandled) {
                //give control to object when it's the main event
                //and when the duration is < 0 and not overriden
                
                if (current_duration < 0 && isMain) {
                    endUseSuccessCBs.Add( () => { EndPlayAttempt(true); } );
                }
            }
                
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
        

        public EventPlayEnder OverrideEndPlay (int layer, Action<bool> onEndAttempt, string endPlayOverrideReason) {
            return GetUpdateLayer(layer).OverrideEndPlay(onEndAttempt, endPlayOverrideReason);
            
        }


        /*
            override with null to skip cue playback
        */
        public void SkipPlay (int layer, string packName) {
            GetUpdateLayer(layer).SkipPlay(packName);
        }

        //public const int interruptableLayer = -999;
        public void InterruptLayer (int layer, string reason) {
            Debug.Log("interrupting layer: " + layer + " : " + reason);
            GetUpdateLayer(layer).Interrupt();
            //GetUpdateLayer(interruptableLayer).Interrupt();
        }


        //public void InterruptLayers () {
        //    foreach (var key in updateLayers.Keys) {
        //        updateLayers[key].Interrupt();
        //    }
        //}

        public void OverrideEventToPlay(int layer, Event overrideEvent) {
            GetUpdateLayer(layer).OverrideEventToPlay(overrideEvent);
        }

        bool ChcekLinkedPlayers() {
            if (pack2playEvents.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return false;
            }
            return true;
        }

        public void PlayEvents (Event[] events, int layer, float overrideDuration, bool asInterrupter){
            if (!ChcekLinkedPlayers()) return;
            
            GetUpdateLayer(layer).PlayEvents(pack2playEvents, paramDict, events, overrideDuration, asInterrupter);
        }

/*
 */
        public void InterruptPerformances () {
            foreach (var p in current_playlists) {
                p.InterruptPerformance();
            }
            current_playlists.Clear();
        }



    }
}