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
        //public bool AttemptedEnd(int layer) {
        //    return GetUpdateLayer(layer).attemptedEnd;
        //}
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
            Action<bool> endPlayAttemptCallback;
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
                    Debug.Log("end event timer :: " + current_duration);
                    EndPlayAttempt(true);
                }
            }

            public void Interrupt (int layer, string reason) {
                // if (playing) {


                // }
                Debug.Log("interrupting layer: " + layer + " : " + reason);
            
                //returns false if end play overriden
                if (!EndPlayAttempt(true)) {
                    //take away control from last play ender
                    lastPlayEnder.LoseControl();
                    EndPlay(true);
                }
                
            }
            bool playing;
            void EndPlay (bool success) {

                playing = false;
                
                Debug.Log("endevent in player");
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

                lastPlayEnder = new EventPlayEnder( () => EndPlay(true) );

                return lastPlayEnder;
            }

            public void PlayEvents (
                MiniTransform transforms, int myLayer, EventPlayer myPlayer,
                Dictionary<int, PlayerMessage> pack2playEvents,
                Dictionary<string, CustomParameter> paramDict, 
                Event[] events, float overrideDuration, bool asInterrupter
            ){

                bool wasPlaying = playing;


                playing = true;
                   
                //Debug.Log("playing cue");
                duration_timer = 0;
                current_duration = overrideDuration;
                if (current_duration >= 0) {
                    Debug.Log("set current duration to :: " + current_duration);
                }


                bool endPlayAttemptHandled = current_duration >= 0;

                int l = 0;
                if (events != null) {
                    l = events.Length;
                }

                bool successPlay = true;
                
                if (events != null) {

                    int i = 0;
                    foreach (var e in events) {                    
                        bool success = PlayEvent(transforms, myLayer, myPlayer, pack2playEvents, paramDict, e, i == 0, asInterrupter, endPlayAttemptHandled);
                        if (!success) {
                            successPlay = false;
                        }
                        i++;
                    }
                }
                //additionalEvents.Clear();
                skipPlays.Clear();
                
                if (!successPlay) {
                    if (current_duration < 0) {
                        Debug.Log("not succes end");        
                        EndPlayAttempt(false);
                    }
                }
                else {
                    if (l == 0 && !endPlayAttemptHandled) {
                        //if (endPlayOverriden) Debug.Log("reason " + endPlayOverrideReason);
                        Debug.Log("zero events not event handled early out");
                        EndPlayAttempt(true);
                    }
                }
            }









        bool PlayEvent (MiniTransform transforms, int myLayer, EventPlayer myPlayer,
            Dictionary<int, PlayerMessage> pack2playEvents,
            Dictionary<string, CustomParameter> paramDict, 
            Event e, bool isMainEvent, bool asInterrupter, bool endPlayAttemptHandled) {


            EventResponse eventResponse = new EventResponse(skipPlays);

            e.GetParamFilteredObjects(paramDict, eventResponse);

            eventResponse.LogErrors();
            eventResponse.LogWarnings();

            bool mainFound = !eventResponse.noMainFound;

            foreach (var k in eventResponse.objectsPerPack.Keys) {

                bool isMainPack = k == e.mainPackID;
                
                if (eventResponse.objectsPerPack[k].Count > 0) {
                    AssetObject o = eventResponse.objectsPerPack[k].RandomChoice();

                    if (!endPlayAttemptHandled) {

                        if (isMainPack && isMainEvent) {
                            current_duration = o["Duration"].GetValue<float>();
                            // Debug.Log("in PLAYEVENT set current duration to :: " + current_duration);
                        }
                    }

                    HashSet<Action> endUseSuccessCBs = new HashSet<Action>();
            
                    if (!endPlayAttemptHandled) {
                        //give control to object when it's the main event
                        //and when the duration is < 0 and not overriden
                        
                        if (isMainEvent && isMainPack) {
                        
                            endUseSuccessCBs.Add( () => { EndPlayAttempt(true); } );
                        }
                    }

                    string stepBlock = o.messageBlock;
                    string logErrors = "";
                    CustomScripting.ExecuteMessageBlock (myLayer, myPlayer, stepBlock, Vector3.zero, ref logErrors);
                    
                    if (!logErrors.IsEmpty()) {
                        logErrors = "broadcast message from asset object: " + o.objRef.name + logErrors;
                        Debug.LogError(logErrors);
                    }

                    if (!pack2playEvents.ContainsKey(k)) {
                        Debug.LogError("skipping " + PacksManager.ID2Name(k) + " not connected to player");
                        continue;
                    }

    
                    pack2playEvents[k](o, asInterrupter, transforms, endUseSuccessCBs);
            
                }

            }
            
            return mainFound;   
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
            GetUpdateLayer(layer).Interrupt(layer, reason);
        }



        bool ChcekLinkedPlayers() {
            if (pack2playEvents.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return false;
            }
            return true;
        }

        public void PlayEvents (MiniTransform transforms, Event[] events, int layer, float overrideDuration, bool asInterrupter){
            if (!ChcekLinkedPlayers()) return;
            
            GetUpdateLayer(layer).PlayEvents(transforms, layer, this, pack2playEvents, paramDict, events, overrideDuration, asInterrupter);
        }

        public void InterruptPerformances () {
            foreach (var p in currentPlaylists) {
                p.InterruptPerformance();
            }
            currentPlaylists.Clear();
        }



    }
}