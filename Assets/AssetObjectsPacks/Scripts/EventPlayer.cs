using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class EventPlayer : MonoBehaviour
    {

        public CustomParameter[] playerParams;
        Dictionary<string, CustomParameter> param_dict = new Dictionary<string, CustomParameter>();

        void RebuildDictionary () {
            Debug.Log("Rebuilding dictionary");
            param_dict.Clear();
            for (int i = 0; i < playerParams.Length; i++) param_dict.Add(playerParams[i].name, playerParams[i]);
        }
        public CustomParameter this [string paramName] {
            get {
                if (param_dict.Count != playerParams.Length) RebuildDictionary();
                return param_dict[paramName];
            }
        }



        public List<Playlist.Performance> current_playlists = new List<Playlist.Performance>();
        Dictionary<string, Action<AssetObject, Action>> pack2playevent = new Dictionary<string, Action<AssetObject, Action>>();

        public void SubscribeToEventPlay(string packName, Action<AssetObject, Action> on_play_event) {
            pack2playevent[packName] = on_play_event;
        }

        public Action on_event_end;
        public float duration_timer, current_duration;
        public bool playing_event;
        void Update () {
            if (!playing_event || current_duration < 0)
                return;

            duration_timer += Time.deltaTime;
            if (duration_timer >= current_duration) {
                EndEvent();
            }
        }
        void EndEvent () {
                
            if (on_event_end != null) {
                on_event_end();
                on_event_end = null;
            }
            endEventOverriden = false;
            playing_event = false;
            overrideEvents.Clear();

        }
        void EndEventDummy () { 

            
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

        public void PlayEvents (Event[] events, Action on_event_end) {

            
            if (pack2playevent.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return;
            }

            this.on_event_end = on_event_end;
                        
            playing_event = true;
            
            duration_timer = 0;

            int l = events.Length;

            for (int i = 0; i < l; i++) {
                Event ep = events[i];

                int packIndex;
                string packName = AssetObjectsManager.instance.packs.FindPackByID( ep.assetObjectPackID, out packIndex).name;
                
                Event overrideEvent;
                if (overrideEvents.TryGetValue(packName, out overrideEvent)) {
                    Debug.Log("overriding: " + ep.name + " with: " + overrideEvent.name);
                    ep = overrideEvent;
                }


                
                //AssetObject o = ep.assetObjects.Where( ao => ao.PassesConditionCheck( playerParams )  ).ToArray().RandomChoice();
                AssetObject o = ep.GetFilteredStatesList(playerParams).RandomChoice();

                if (i == 0) current_duration = o["Duration"].FloatValue;
                
                //give control to object when it's the zero index event
                //and when the duration is < 0
                //and not overriden
                Action end_event = EndEventDummy;
                if (!endEventOverriden && current_duration < 0 && i == 0) end_event = EndEvent;
                
                pack2playevent[packName](o, end_event);
            }
        }
    }
}

