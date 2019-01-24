
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace AssetObjectsPacks {

    public class AssetObjectEventPlayer//<O> : MonoBehaviour
    : MonoBehaviour
    //where E : AssetObjectEven<E>
    //where P : AssetObjectEvenPlayer<E, P>
    {
        /*
        P _s = null;
        P self {
            get {
                if (_s == null) _s = (P)this;
                return _s;
            }
        }
         */

        

        public System.Action<AssetObject, AssetObjectEvent, Action> on_play_event;

        Action on_event_end;
        float duration_timer, current_duration;
        bool playing_event;
        void Update () {
            
            if (!playing_event || current_duration < 0) {
                return;
            }
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
            List<AssetObject> objs = new List<AssetObject>(asset_object_event.assetObjects);


            for (int i = 0; i < asset_object_event.behaviors.Length; i++) {
                objs = asset_object_event.behaviors[i].FilterEventAssets(this, objs);
            }
            
            current_duration = asset_object_event.duration;
            this.on_event_end = on_event_end;
            playing_event = true;
            duration_timer = 0;

            if (on_play_event != null) {

                Action end_event = EndEvent;
                if (current_duration >= 0) {
                    end_event = EndEventDummy;
                }
                on_play_event(objs.RandomChoice(), asset_object_event, end_event);
            }
            else {
                Debug.LogWarning(name + " isnt doing anything, no on play event specified");
            }        

            //OnUseAssetObjectHolder(asset_object_event, objs.RandomChoice(), on_end_use);
        }
        //protected abstract void OnUseAssetObjectHolder (E asset_object_event, O chosen_obj, Action on_end_use);
    }
}

