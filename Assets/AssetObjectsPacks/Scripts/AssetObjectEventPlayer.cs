using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace AssetObjectsPacks {

    public class AssetObjectEventPlayer : MonoBehaviour
    {

        public AssetObjectParam[] playerParams;
        Dictionary<string, AssetObjectParam> param_dict = new Dictionary<string, AssetObjectParam>();

        void RebuildDictionary () {
            param_dict.Clear();
            for (int i = 0; i < playerParams.Length; i++) param_dict.Add(playerParams[i].name, playerParams[i]);
        }
        public AssetObjectParam this [string paramName] {
            get {
                if (param_dict.Count != playerParams.Length) {
                    RebuildDictionary();
                }
                return param_dict[paramName];
            }
        }



        public List<AssetObjectEventPlaylist.Performance> current_playlists = new List<AssetObjectEventPlaylist.Performance>();
        Dictionary<string, Action<AssetObject, Action>> pack2playevent = new Dictionary<string, Action<AssetObject, Action>>();

        public void SubscribeToEventVariation(string pack, Action<AssetObject, Action> on_play_event) {
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
                //Debug.Log("end use after " + current_duration + " (s)");

                EndEvent();
            }
        }
        void EndEvent () {
                
            if (on_event_end != null) {
                //Debug.Log("event ending");
                on_event_end();
                on_event_end = null;
            }
            playing_event = false;
        }
        void EndEventDummy () {
            //Debug.Log("event ending dummy");
                
        }

        public void PlayEvent (AssetObjectEvent asset_object_event, Action on_event_end) {

            
            if (pack2playevent.Count == 0) {
                Debug.LogWarning(name + " isnt doing anything, no on play events specified");
                return;
            }

            this.on_event_end = on_event_end;
            
            //current_duration = asset_object_event.duration;
            
            playing_event = true;
            
            duration_timer = 0;

            int l = asset_object_event.eventPacks.Length;

            for (int i = 0; i < l; i++) {
                AssetObjectEventPack ep = asset_object_event.eventPacks[i];
                
                string k = AssetObjectsManager.instance.packs.FindPackByID( ep.assetObjectPackID).name;




                //List<AssetObject> objs = new List<AssetObject>(ep.assetObjects);

                AssetObject[] objs = ep.assetObjects.Where( ao => ao.PassesConditionCheck( playerParams )  ).ToArray();
                

                /*

                for (int x = 0; x < asset_object_event.behaviors.Length; x++) {
                    AssetObjectEventBehavior b = asset_object_event.behaviors[x];
                    if (b.event_pack_name == k) {
                        objs = asset_object_event.behaviors[i].FilterEventAssets(this, objs);
                    }
                }
                 */                

                AssetObject o = objs.RandomChoice();

                
                if (i == 0) {
                    current_duration = o["Duration"].floatValue;
                }

                //if we're on a timer dont give control to the receiving component
                Action end_event = EndEvent;
                if (current_duration >= 0 || i != 0) {
                    end_event = EndEventDummy;
                }
                
                pack2playevent[k](o, end_event);
            }
        }
    }
}

