using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class AssetObjectEventPlayer : MonoBehaviour
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



        public List<AssetObjectEventPlaylist.Performance> current_playlists = new List<AssetObjectEventPlaylist.Performance>();
        Dictionary<string, Action<AssetObject, Action>> pack2playevent = new Dictionary<string, Action<AssetObject, Action>>();

        public void SubscribeToEventPlay(string pack, Action<AssetObject, Action> on_play_event) {
            pack2playevent[pack] = on_play_event;
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
            playing_event = false;
        }
        void EndEventDummy () { }

        public void PlayEvents (AssetObjectEventPack[] events, Action on_event_end) {

            
            if (pack2playevent.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return;
            }

            this.on_event_end = on_event_end;
                        
            playing_event = true;
            
            duration_timer = 0;

            int l = events.Length;

            for (int i = 0; i < l; i++) {
                AssetObjectEventPack ep = events[i];

                int packIndex;
                
                string k = AssetObjectsManager.instance.packs.FindPackByID( ep.assetObjectPackID, out packIndex).name;
                
                AssetObject o = ep.assetObjects.Where( ao => ao.PassesConditionCheck( playerParams )  ).ToArray().RandomChoice();

                if (i == 0) current_duration = o["Duration"].FloatValue;
                
                //if we're on a timer dont give control to the receiving component
                Action end_event = EndEvent;
                if (current_duration >= 0 || i != 0) end_event = EndEventDummy;
                
                pack2playevent[k](o, end_event);
            }
        }
    }
}

