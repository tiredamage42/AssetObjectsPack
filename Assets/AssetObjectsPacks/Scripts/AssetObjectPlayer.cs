
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace AssetObjectsPacks {

    public class AssetObjectEventPlayer : MonoBehaviour
    {

        public List<AssetObjectEventPlaylist.Performance> current_playlists = new List<AssetObjectEventPlaylist.Performance>();
        
        //public System.Action<AssetObject, AssetObjectEvent, Action> on_play_event;


        Dictionary<string, Action<AssetObject, Action>> var2playevent = new Dictionary<string, Action<AssetObject, Action>>();


        public void SubscribeToEventVariation(string variation_key, Action<AssetObject, Action> on_play_event) {
            var2playevent[variation_key] = on_play_event;
        }





        Action on_event_end;
        float duration_timer, current_duration;
        bool playing_event;
        void Update () {
            if (!playing_event || current_duration < 0)
                return;

            duration_timer += Time.deltaTime;
            if (duration_timer >= current_duration) {
                Debug.Log("end use after " + current_duration + " (s)");
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
        void EndEventDummy () {}

        public void PlayEvent (AssetObjectEvent asset_object_event, Action on_event_end) {
            this.on_event_end = on_event_end;
            
            //current_duration = asset_object_event.duration;
            
            playing_event = true;
            
            duration_timer = 0;


            for (int i = 0; i < asset_object_event.eventPacks.Length; i++) {
                AssetObjectEventPack ep = asset_object_event.eventPacks[i];
                
                string k = AssetObjectsManager.instance.packs.FindPackByID( ep.assetObjectPackID).name;
                List<AssetObject> objs = new List<AssetObject>(ep.assetObjects);
                
                for (int x = 0; x < asset_object_event.behaviors.Length; x++) {
                    AssetObjectEventBehavior b = asset_object_event.behaviors[x];
                    if (b.event_pack_name == k) {
                        objs = asset_object_event.behaviors[i].FilterEventAssets(this, objs);
                    }
                }

                Action end_event = EndEvent;


                //if we're on a timer dont give control to the receiving component
                if (current_duration >= 0 || i != asset_object_event.main_pack_index) {
                    end_event = EndEventDummy;
                }

                AssetObject o = objs.RandomChoice();

                if (i == asset_object_event.main_pack_index) {
                    current_duration = o.duration;
                }

                var2playevent[k](o, end_event);



            }


            

            if (var2playevent.Count != 0) {

                //Action end_event = EndEvent;

                //if we're on a timer dont give control to the receiving component
                //if (current_duration >= 0) {
                //    end_event = EndEventDummy;
                //}
                //on_play_event(objs.RandomChoice(), asset_object_event, end_event);





            }
            else {
                Debug.LogWarning(name + " isnt doing anything, no on play event specified");
            }        
        }
    }
}

