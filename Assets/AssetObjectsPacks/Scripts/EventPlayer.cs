using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class EventPlayer : MonoBehaviour
    {
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
        
        
        Dictionary<string, Action<AssetObject, bool, HashSet<Action>>> pack2playEvents = new Dictionary<string, Action<AssetObject, bool, HashSet<Action>>>();
        
        //Dictionary<int, HashSet<Action<bool>>> layer2EndPlay = new Dictionary<int, HashSet<Action<bool>>>();
        
        
        //Dictionary<string, HashSet<Action<bool>>> pack2endUseCallbacks = new Dictionary<string, HashSet<Action<bool>>>();
        //Dictionary<string, List<Action>> pack2failEvents = new Dictionary<string, List<Action>>();
        
        public void LinkAsPlayer(string packName, Action<AssetObject, bool, HashSet<Action>> onPlayEvent) {
            pack2playEvents[packName] = onPlayEvent;
        }
/*
        void SubscribeToEvent(Dictionary<string, HashSet<Action<bool>>> dict, string packName, Action<bool> action) {
            HashSet<Action<bool>> actions;
            if (dict.TryGetValue(packName, out actions)) {
                actions.Add(action);
            }
            else {
                dict[packName] = new HashSet<Action<bool>> () {action};
            }
        }
 */
        //public void SubscribeToAssetObjectUseEnd(string packName, Action<bool> onEndUse) {
        //    SubscribeToEvent(pack2endUseCallbacks, packName, onEndUse);
        //}


        //const string playEndPackName = "@@PLAYENDPACK@@";


        public void SubscribeToPlayEnd(int layer, Action<bool> onPlayEnd) {

            GetUpdateLayer(layer).SubscribeToPlayEnd(onPlayEnd);

            /*
            //SubscribeToEvent(pack2endUseCallbacks, playEndPackName, onPlayEnd);

            HashSet<Action<bool>> actions;
            if (layer2EndPlay.TryGetValue(layer, out actions)) {
                actions.Add(onPlayEnd);
            }
            else {
                layer2EndPlay[layer] = new HashSet<Action<bool>> () {onPlayEnd};
            }
             */
        
        }
        //public void SubscribeToEventFail(string packName, Action onEventFail) {
        //    SubscribeToEvent(pack2failEvents, packName, onEventFail);
        //}
        

        //Action<bool> onEndPlayCallback;

        
        //float duration_timer, current_duration = -1;
        
        //public bool playing_event;

        //List<UpdateLayer> updateLayers = new List<UpdateLayer>();

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

            Dictionary<string, Event> overrideEvents = new Dictionary<string, Event>();
            /*
                override with null to skip cue playback
            */
            public void SkipPlay (string packName) {
                OverrideEventToPlay(packName, null);
            }
            public void OverrideEventToPlay(string packName, Event overrideEvent) {
                overrideEvents.Add(packName, overrideEvent);
            }

        

            public void SubscribeToPlayEnd(Action<bool> onPlayEnd) {
                endPlayCallbacks.Add(onPlayEnd);
            }

            public void Update () {
                if (current_duration < 0 || endEventOverriden) 
                    return;

                duration_timer += Time.deltaTime;
                if (duration_timer >= current_duration) {
                    //Debug.LogError("END EVENT TIMER");
                    EndEvent(true);
                }
            }
            void EndEvent (bool success) {
                foreach (var cb in endPlayCallbacks) {
                    cb(success);
                }
                endPlayCallbacks.Clear();
                
                endEventOverriden = false;
                current_duration = -1;
                duration_timer = 0;
                
                overrideEvents.Clear();
            }

            public Action OverrideEndEvent () {
                if (endEventOverriden) {
                    Debug.LogWarning("trying to override end event");
                    return null;
                }
                endEventOverriden = true;
                return () => EndEvent(true);
            }

            public void PlayEvents_Cue (
                Dictionary<string, Action<AssetObject, bool, HashSet<Action>>> pack2playEvents,
                Dictionary<string, CustomParameter> paramDict, 
            
                Event[] events, float overrideDuration){
                //if (!ChcekLinkedPlayers()) return;
                duration_timer = 0;
                //display options in cue editor
                bool asInterrupter = true;
                current_duration = overrideDuration;
                bool endEventHandled = endEventOverriden || current_duration >= 0;
                int l = events.Length;

                bool successPlay = true;
                for (int i = 0; i < events.Length; i++) {

                    bool success = _PlayEvent(pack2playEvents, paramDict, events[i], i == 0, asInterrupter, endEventHandled);
                    if (!success) {
                        successPlay = false;
                    }
                }

                if (!successPlay) {
                    EndEvent(false);
                }
                else {
                    if (l == 0 && !endEventHandled) {
                        EndEvent(true);
                    }

                }
        }
        public void PlayEvent (
            Dictionary<string, Action<AssetObject, bool, HashSet<Action>>> pack2playEvents,
            Dictionary<string, CustomParameter> paramDict, 
            
            Event e, float overrideDuration, bool asInterrupter){//}, Action<bool> onEndPlayCallback) {
        //public void PlayEvent (Event e, bool interruptPlaylists, bool simple, bool asInterrupter) {
            duration_timer = 0;
            current_duration = overrideDuration;
            bool endEventHandled = endEventOverriden || current_duration >= 0;

            if (!_PlayEvent(pack2playEvents, paramDict, e, true, asInterrupter, endEventHandled)) {
                EndEvent(false);
            }
            
        
        }

        
        bool _PlayEvent ( 
            Dictionary<string, Action<AssetObject, bool, HashSet<Action>>> pack2playEvents,
            Dictionary<string, CustomParameter> paramDict, 
            Event e, bool isMain, bool asInterrupter, bool endEventHandled) {
        //void _PlayEvent (Event e, bool isMain, bool simple, bool asInterrupter, bool endEventHandled) {
        
            string packName = AssetObjectsManager.instance.packs.FindPackByID( e.assetObjectPackID, out _).name;

            Event overrideEvent;
            if (overrideEvents.TryGetValue(packName, out overrideEvent)) {
                overrideEvents.Remove(packName);

                if (overrideEvent == null) return true;   
                Debug.Log("overriding: " + e.name + " with: " + overrideEvent.name);
                e = overrideEvent;
            }
            
            List<AssetObject> filteredList = e.GetParamFilteredObjects(paramDict);





            //HashSet<Action<bool>> onEndUseCallbacks;// = new HashSet<Action<bool>>();
            //if (pack2endUseCallbacks.TryGetValue(packName, out onEndUseCallbacks)) {

            //    pack2endUseCallbacks.Remove(packName);
            //}


            if (filteredList.Count == 0) {
            //    if (onEndUseCallbacks != null) {
            //        foreach (var cb in onEndUseCallbacks) 
            //            cb(false);
             //   }
                return !isMain;
            }
            //return false;
        



            //if (CheckErrors(packName, filteredList.Count)) {
            //    return;
            //}
            
            AssetObject o = filteredList.RandomChoice();


            

            if (isMain && !endEventHandled) {
            //if (isMain && !endEventOverriden && !simple) {
                current_duration = o["Duration"].GetValue<float>();
            }

            //List<Action> onEndUseCallbacks;
            //if (pack2endUseCallbacks.TryGetValue(packName, out onEndUseCallbacks)) 
            //    pack2endUseCallbacks.Remove(packName);


            HashSet<Action> endUseSuccessCBs = new HashSet<Action>();// SuccessCallbacks(onEndUseCallbacks);
                

            
            if (!endEventHandled) {
            //if (!simple) {
                //give control to object when it's the main event
                //and when the duration is < 0 and not overriden
                
                //if (!endEventOverriden && current_duration < 0 && isMain) {
                if (current_duration < 0 && isMain) {
                
                    //Debug.Log("adding end event as ed use callback");








                    //if (onEndUseCallbacks == null) {
                    //    onEndUseCallbacks = new List<Action>();
                    //}
                    //onEndUseCallbacks.Add(EndEvent);

                    endUseSuccessCBs.Add( () => EndEvent(true) );
                }
            }
            
                
            pack2playEvents[packName](o, asInterrupter, endUseSuccessCBs);// onEndUseCallbacks);

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
        /*
        void Update (int layer) {
            //if (endAfterPlay) {
            //    EndEvent();
            //    endAfterPlay = false;
            //}
            //if (!playing_event || current_duration < 0) //maybe if override end event as well
            if (current_duration < 0 || endEventOverriden) 
                return;

            duration_timer += Time.deltaTime;
            if (duration_timer >= current_duration) {
                //Debug.LogError("END EVENT TIMER");
                EndEvent(true);
            }
        }
         */

        /*
        void EndEventSimple () {
            EndEvent(true);
        }
        void EndEvent (int layer, bool success) {
            HashSet<Action<bool>> endPlays;
            if (pack2endUseCallbacks.TryGetValue(playEndPackName, out endPlays)) {
                pack2endUseCallbacks.Remove(playEndPackName);
                foreach (var cb in endPlays) {
                    cb(success);
                }
            }
            


            //if (onEndPlayCallback != null) {
            //    onEndPlayCallback(success);
            //    onEndPlayCallback = null;
            //}
            endEventOverriden = false;
            current_duration = -1;
            duration_timer = 0;
            overrideEvents.Clear();
        }
        */

        //bool endEventOverriden;


        public Action OverrideEndEvent (int layer) {
            return GetUpdateLayer(layer).OverrideEndEvent();
            /*
            if (endEventOverriden) {
                Debug.LogWarning("trying to override end event");
                return null;
            }
            endEventOverriden = true;
            return EndEventSimple;
             */
        }


        //bool endAfterPlay;
        //public void EndAfterPlay () {
        //    endAfterPlay = true;
        //}


        //Dictionary<string, Event> overrideEvents = new Dictionary<string, Event>();
        /*
            override with null to skip cue playback
        */
        public void SkipPlay (int layer, string packName) {

            GetUpdateLayer(layer).SkipPlay(packName);
            //OverrideEventToPlay(packName, null);
        }
        public void OverrideEventToPlay(int layer, string packName, Event overrideEvent) {
            GetUpdateLayer(layer).OverrideEventToPlay(packName, overrideEvent);
            
            //overrideEvents.Add(packName, overrideEvent);
        }

        bool ChcekLinkedPlayers() {
            if (pack2playEvents.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return false;
            }
            //duration_timer = 0;
            return true;
        }

        public void PlayEvents_Cue (Event[] events, float overrideDuration){
            if (!ChcekLinkedPlayers()) return;
            GetUpdateLayer(-1).PlayEvents_Cue(pack2playEvents, paramDict, events, overrideDuration);

            /*
            //display options in cue editor
            bool asInterrupter = true;
            //this.onEndPlayCallback = onEndPlayCallback;
            current_duration = overrideDuration;
            bool endEventHandled = endEventOverriden || current_duration >= 0;
            int l = events.Length;

            bool successPlay = true;
            for (int i = 0; i < events.Length; i++) {

                bool success = _PlayEvent(events[i], i == 0, asInterrupter, endEventHandled);
                if (!success) {
                    successPlay = false;
                }
            }
            if (!successPlay) {
                EndEvent(false);
            }
            else {
                if (l == 0 && !endEventHandled) {
                    EndEvent(true);
                }

            }
             */
        }



        public void InterruptPerformances () {
            foreach (var p in current_playlists) {
                p.InterruptPerformance();
            }
            current_playlists.Clear();
        }
        //when no callbacks needed, just simple state switches, etc...
        public void PlayEvent (int layer, Event e, float overrideDuration, bool asInterrupter){//}, Action<bool> onEndPlayCallback) {
        //public void PlayEvent (Event e, bool interruptPlaylists, bool simple, bool asInterrupter) {
            if (!ChcekLinkedPlayers()) return;
            bool interruptPlaylists = layer == -1;

            if (interruptPlaylists) {
                InterruptPerformances();
            }

            GetUpdateLayer(layer).PlayEvent(pack2playEvents, paramDict, e, overrideDuration, asInterrupter);
/*
            //this.onEndPlayCallback = onEndPlayCallback;
            
            //_PlayEvent(e, true, simple, asInterrupter);

            current_duration = overrideDuration;
            bool endEventHandled = endEventOverriden || current_duration >= 0;

            if (!_PlayEvent(e, true, asInterrupter, endEventHandled)) {
                EndEvent(false);
            }
 */
            
        
        }
     /*
        bool _PlayEvent (Event e, bool isMain, bool asInterrupter, bool endEventHandled) {
        //void _PlayEvent (Event e, bool isMain, bool simple, bool asInterrupter, bool endEventHandled) {
        
            string packName = AssetObjectsManager.instance.packs.FindPackByID( e.assetObjectPackID, out _).name;

            Event overrideEvent;
            if (overrideEvents.TryGetValue(packName, out overrideEvent)) {
                overrideEvents.Remove(packName);

                if (overrideEvent == null) return true;   
                Debug.Log("overriding: " + e.name + " with: " + overrideEvent.name);
                e = overrideEvent;
            }
            
            List<AssetObject> filteredList = e.GetParamFilteredObjects(param_dict);





            HashSet<Action<bool>> onEndUseCallbacks;// = new HashSet<Action<bool>>();
            if (pack2endUseCallbacks.TryGetValue(packName, out onEndUseCallbacks)) {

                pack2endUseCallbacks.Remove(packName);
            }


            if (filteredList.Count == 0) {
                if (onEndUseCallbacks != null) {
                    foreach (var cb in onEndUseCallbacks) 
                        cb(false);
                }
                return !isMain;
            }
            //return false;
        



            //if (CheckErrors(packName, filteredList.Count)) {
            //    return;
            //}
            
            AssetObject o = filteredList.RandomChoice();


            

            if (isMain && !endEventHandled) {
            //if (isMain && !endEventOverriden && !simple) {
                current_duration = o["Duration"].GetValue<float>();
            }

            //List<Action> onEndUseCallbacks;
            //if (pack2endUseCallbacks.TryGetValue(packName, out onEndUseCallbacks)) 
            //    pack2endUseCallbacks.Remove(packName);


            HashSet<Action> endUseSuccessCBs = SuccessCallbacks(onEndUseCallbacks);
                

            
            if (!endEventHandled) {
            //if (!simple) {
                //give control to object when it's the main event
                //and when the duration is < 0 and not overriden
                
                //if (!endEventOverriden && current_duration < 0 && isMain) {
                if (current_duration < 0 && isMain) {
                
                    //Debug.Log("adding end event as ed use callback");








                    //if (onEndUseCallbacks == null) {
                    //    onEndUseCallbacks = new List<Action>();
                    //}
                    //onEndUseCallbacks.Add(EndEvent);

                    endUseSuccessCBs.Add(EndEventSimple);
                }
            }
            
                
            pack2playEvents[packName](o, asInterrupter, endUseSuccessCBs);// onEndUseCallbacks);

            return true;
            
        }

        HashSet<Action> SuccessCallbacks (HashSet<Action<bool>> cbs) {
            if (cbs == null || cbs.Count == 0) {
                return new HashSet<Action>();
            }

            //List<Action> r = new List<Action>();

            //for (int i = 0; i < cbs.Count; i++) {
            //    r.Add ( () => cbs[i](true) );
            //}

            return cbs.Generate<Action, Action<bool>>(cb => () => cb(true) ).ToHashSet();

            //return r;

        }
         */
     
            /*
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

             */

        
    }
}

