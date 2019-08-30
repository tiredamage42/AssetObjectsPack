using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class EventPlayer : MonoBehaviour
    {
        public class EventPlayEnder {
            Action cb;
            public void EndPlay (string reason) {
                if (cb != null) {
                    //Debug.Log(reason);
                    cb();
                    LoseControl();
                }
            }
            public EventPlayEnder (Action cb) { 
                this.cb = cb; 
            }
            public void LoseControl () {
                cb = null;
            }
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

        public HashSet<Playlists.Performance> currentPlaylists = new HashSet<Playlists.Performance>();
        
        public delegate void PlayerMessage (AssetObject chosenObject, bool interrupter, MiniTransform transforms, HashSet<Action> onEendUseCallbacks);
        Dictionary<int, PlayerMessage> pack2playEvents = new Dictionary<int, PlayerMessage>();
                
        public void LinkAsPlayer(string packName, PlayerMessage onPlayEvent) {
            int packID = PacksManager.Name2ID(packName);
            pack2playEvents[packID] = onPlayEvent;
        }
        public void SubscribeToPlayEnd(int layer, Action<bool> onPlayEnd) {
            GetUpdateLayer(layer).SubscribeToPlayEnd(onPlayEnd);
        }
        
        Dictionary<int, UpdateLayer> updateLayers = new Dictionary<int, UpdateLayer>();
        
        UpdateLayer GetUpdateLayer(int layer) {
            UpdateLayer updateLayer;
            if (!updateLayers.TryGetValue(layer, out updateLayer)) {
                updateLayer = new UpdateLayer(layer);
                updateLayers[layer] = updateLayer;
            }
            return updateLayer;
        }

        static string ConstructStringForDebug (int layer) {
            return "Layer: " + layer + " ";
        }

        class UpdateLayer {
            public UpdateLayer (int layer) {
                myLayer = layer;
                SetDuration(-1);
            }
            int myLayer;
            float duration_timer, current_duration = -1;
            bool endPlayOverriden, playing;
            
            
            Action<bool> onPlayerEndPlay;
            HashSet<Action<bool>> endPlayCallbacks = new HashSet<Action<bool>>();
            HashSet<int> skipPlays = new HashSet<int>();
            
            public void SkipPlay (string packName) {
                skipPlays.Add(PacksManager.Name2ID(packName));
            }
            public void SubscribeToPlayEnd(Action<bool> onPlayEnd) {
                endPlayCallbacks.Add(onPlayEnd);
            }

            public void Update () {
                if (current_duration < 0 || endPlayOverriden) 
                    return;
                duration_timer += Time.deltaTime;
                if (duration_timer >= current_duration) {
                    EndPlayAttempt(true, "timer is up");
                }
            }

            public void Interrupt ( string reason) {
                if (playing) {

                    Debug.Log(ConstructStringForDebug(myLayer) + "interrupting : " + reason);
                    
                    //returns false if end play overriden
                    if (!EndPlayAttempt(true, "interrupt: " + reason)) {
                        //take away control from last play ender
                        lastPlayEnder.LoseControl();
                        EndPlay(true);
                    }
                }
            
            }
            void EndPlay (bool success) {
                playing = false;
                endPlayOverriden = false;
                SetDuration(-1);
                duration_timer = 0;

                onPlayerEndPlay = null;
                lastPlayEnder = null;
                
                // Debug.Log("endevent in player");
                foreach (var cb in endPlayCallbacks) {
                    cb(success);
                }
                endPlayCallbacks.Clear();
            }


            bool EndPlayAttempt (bool success, string reason) {
                
                if (!endPlayOverriden) {
                    // Debug.Log(ConstructStringForDebug(myLayer) + "End PlayAttempt: not overriden" + reason);
                    EndPlay(success);
                    return true;
                }

                if (onPlayerEndPlay != null) {
                    // Debug.Log(ConstructStringForDebug(myLayer) + "End PlayAttempt: onPlayerEndPlay attempt callback" + reason);
                    onPlayerEndPlay(success);
                    onPlayerEndPlay = null;
                }
                return false;
            }


            
            void EndPlayCallback () {
                EndPlay(true);
            }
            EventPlayEnder lastPlayEnder;
            string endPlayOverrideReason;
            public EventPlayEnder OverrideEndPlay (Action<bool> onPlayerEndPlay, string endPlayOverrideReason) {
                if (endPlayOverriden) {
                    Debug.LogWarning(endPlayOverrideReason + " is trying to override end event thats already overriden by: " + this.endPlayOverrideReason);
                    return null;
                }
                // Debug.Log(ConstructStringForDebug(myLayer) + " overriding end event because of " + endPlayOverrideReason);
                endPlayOverriden = true;
                this.endPlayOverrideReason = endPlayOverrideReason;
                this.onPlayerEndPlay = onPlayerEndPlay;

                lastPlayEnder = new EventPlayEnder( EndPlayCallback );

                return lastPlayEnder;
            }

            void SetDuration (float duration) {
                current_duration = duration;
            }

            public void PlayEvents (
                string eventsHolder,
                MiniTransform transforms, EventPlayer myPlayer,
                Dictionary<int, PlayerMessage> pack2playEvents,
                Dictionary<string, CustomParameter> paramDict, 
                Event[] events, float overrideDuration, bool asInterrupter
            ){

                //Debug.Log("playing cue");

                playing = true;
                   
                duration_timer = 0;
                
                SetDuration(overrideDuration);
                // if (current_duration >= 0) Debug.Log("set current duration to :: " + current_duration);
                
                bool endAfterDuration = current_duration >= 0;
                // bool endPlayAttemptHandled = current_duration >= 0;// || endPlayOverriden;

                int l = 0;
                if (events != null) l = events.Length;
                
                bool successPlay = true;
                
                if (events != null) {
                    
                    bool isMainEvent = true;
                    foreach (var e in events) {                    

                        bool success = PlayEvent(transforms, myPlayer, pack2playEvents, paramDict, e, isMainEvent, asInterrupter, endAfterDuration);
                        
                        if (isMainEvent && !success) {
                            successPlay = false;
                        }
                        isMainEvent = false;
                    }
                }

                skipPlays.Clear();
                
                if (!successPlay) {
                    if (current_duration < 0) {
                        // Debug.Log("not succes end " + eventsHolder);        
                        EndPlayAttempt(false, " coulnt find any");
                    }
                }
                else {
                    if (l == 0 && !endAfterDuration){// !endPlayAttemptHandled) {
                        //if (endPlayOverriden) Debug.Log("reason " + endPlayOverrideReason);
                        // Debug.Log("zero events not event handled early out " + eventsHolder);
                        EndPlayAttempt(true, "no events specified");
                    }
                }
            }









        bool PlayEvent (
            MiniTransform transforms, 
            EventPlayer myPlayer,
            Dictionary<int, PlayerMessage> pack2playEvents,
            Dictionary<string, CustomParameter> paramDict, 
            Event e, bool isMainEvent, bool asInterrupter, bool endAfterDuration){
                

            int packID = e.mainPackID;

            if (skipPlays.Contains(packID)) {
                return false;
            }
                

            EventResponse eventResponse = new EventResponse();//skipPlays);

            e.GetParamFilteredObjects(paramDict, eventResponse);

            eventResponse.LogErrors();
            eventResponse.LogWarnings();

            // bool mainFound = !eventResponse.noMainFound;

            // foreach (var k in eventResponse.objectsPerPack.Keys) {

                // bool isMainPack = k == e.mainPackID;


                // List<AssetObject> list = eventResponse.objectsPerPack[k];
                List<AssetObject> list = eventResponse.chosenObjects;
                
                
                if (list.Count > 0) {
                
                    AssetObject o = list.RandomChoice();

                    if (!endAfterDuration) {
                        // if (isMainPack && isMainEvent) {
                        if (isMainEvent) {

                            SetDuration(o["Duration"].GetValue<float>());
                
                        
                            // current_duration = o["Duration"].GetValue<float>();
                        }
                    }

                    HashSet<Action> endUseSuccessCBs = new HashSet<Action>();
            
                    if (!endAfterDuration) {
                        //give control to object when it's the main event
                        //and when the duration is < 0 and not overriden
                        
                        // if (isMainEvent && isMainPack) {
                        if (isMainEvent) {
                        
                            endUseSuccessCBs.Add( () => { EndPlayAttempt(true, "controlled"); } );
                        }
                    }

                    string logErrors = "";
                    CustomScripting.ExecuteMessageBlock (myLayer, myPlayer, o.messageBlock, Vector3.zero, ref logErrors);
                    
                    if (!logErrors.IsEmpty()) {
                        logErrors = "Broadcast message from asset object: " + o.objRef.name + "\n" + logErrors;
                        Debug.LogError(logErrors);
                    }

                    int k = packID;

                    if (!pack2playEvents.ContainsKey(k)) {
                        Debug.LogError("skipping " + PacksManager.ID2Name(k) + " not connected to player");
                        return false;
                        
                    }
    
                    pack2playEvents[k](o, asInterrupter, transforms, endUseSuccessCBs);
            
                }

            
            return list.Count > 0;// mainFound;   
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

        public void InterruptLayer (int layer, string reason) {
            GetUpdateLayer(layer).Interrupt(reason);
        }

        bool ChcekLinkedPlayers() {
            if (pack2playEvents.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no listener components on gameObject");
                return false;
            }
            return true;
        }

        public void PlayEvents (string reason, string eventsHolder, MiniTransform transforms, Event[] events, int layer, float overrideDuration, bool asInterrupter){
            if (!ChcekLinkedPlayers()) 
                return;
            
            // Debug.Log("playing events from  " + eventsHolder + " on layer " + layer + " context: " + reason);

            GetUpdateLayer(layer).PlayEvents(eventsHolder, transforms, this, pack2playEvents, paramDict, events, overrideDuration, asInterrupter);
        }

        public void InterruptPerformances () {
            foreach (var p in currentPlaylists) {
                p.InterruptPerformance();
            }
            currentPlaylists.Clear();
        }
    }
}